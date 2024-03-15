using System.IO.MemoryMappedFiles;

namespace OtterGui.Log;

public class MemoryMappedBuffer : IDisposable
{
    private const int MinHeaderLength = 4 + 4 + 4 + 4 + 4 + 4 + 8;

    private readonly MemoryMappedFile           _file;
    private readonly MemoryMappedViewAccessor   _header;
    private readonly MemoryMappedViewAccessor[] _lines;

    public readonly  int  Version;
    public readonly  uint LineCount;
    public readonly  uint LineCapacity;
    private readonly uint _lineMask;
    private          bool _disposed;

    protected uint CurrentLineCount
    {
        get => _header.ReadUInt32(16);
        set => _header.Write(16, value);
    }

    protected uint CurrentLinePosition
    {
        get => _header.ReadUInt32(20);
        set => _header.Write(20, value);
    }

    protected MemoryMappedBuffer(string mapName, int version, uint lineCount, uint lineCapacity)
    {
        Version      = version;
        LineCount    = BitOperations.RoundUpToPowerOf2(Math.Clamp(lineCount,    2, int.MaxValue >> 3));
        LineCapacity = BitOperations.RoundUpToPowerOf2(Math.Clamp(lineCapacity, 2, int.MaxValue >> 3));
        _lineMask    = LineCount - 1;
        var fileName     = Encoding.UTF8.GetBytes(mapName);
        var headerLength = (uint)(4 + 4 + 4 + 4 + 4 + 4 + fileName.Length + 1);
        headerLength = (headerLength & 0b111) > 0 ? (headerLength & 0b111) + 0b1000 : headerLength;
        var capacity = LineCount * LineCapacity + headerLength;
        _file = MemoryMappedFile.CreateNew(mapName, capacity, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.None,
            HandleInheritability.Inheritable);
        _header = _file.CreateViewAccessor(0, headerLength);
        _header.Write(0,  headerLength);
        _header.Write(4,  Version);
        _header.Write(8,  LineCount);
        _header.Write(12, LineCapacity);
        _header.WriteArray(24, fileName, 0, fileName.Length);
        _header.Write(fileName.Length + 24, (byte)0);
        _lines = Enumerable.Range(0, (int)LineCount).Select(i
                => _file.CreateViewAccessor(headerLength + i * LineCapacity, LineCapacity, MemoryMappedFileAccess.ReadWrite))
            .ToArray();
    }

    protected MemoryMappedBuffer(string mapName, int? expectedVersion = null, uint? expectedMinLineCount = null,
        uint? expectedMinLineCapacity = null)
    {
        _file = MemoryMappedFile.OpenExisting(mapName, MemoryMappedFileRights.ReadWrite, HandleInheritability.Inheritable);
        using var headerLength  = _file.CreateViewAccessor(0, 4, MemoryMappedFileAccess.Read);
        var       headerLength1 = headerLength.ReadUInt32(0);
        if (headerLength1 < MinHeaderLength)
        {
            _file.Dispose();
            throw new Exception($"Map {mapName} did not contain a valid header.");
        }

        _header      = _file.CreateViewAccessor(0, headerLength1, MemoryMappedFileAccess.ReadWrite);
        Version      = _header.ReadInt32(4);
        LineCount    = _header.ReadUInt32(8);
        LineCapacity = _header.ReadUInt32(12);
        _lineMask    = LineCount - 1;
        if (expectedVersion.HasValue && expectedVersion.Value != Version)
        {
            _file.Dispose();
            throw new Exception($"Map {mapName} has version {Version} instead of {expectedVersion.Value}.");
        }

        if (LineCount < expectedMinLineCount)
        {
            _file.Dispose();
            throw new Exception($"Map {mapName} has line count {LineCount} but line count >= {expectedMinLineCount.Value} is required.");
        }

        if (LineCapacity < expectedMinLineCapacity)
        {
            _file.Dispose();
            throw new Exception(
                $"Map {mapName} has line capacity {LineCapacity} but line capacity >= {expectedMinLineCapacity.Value} is required.");
        }

        var name = ReadString(GetSpan(_header, 24));
        if (name != mapName)
        {
            _file.Dispose();
            throw new Exception($"Map {mapName} does not contain its map name at the expected location.");
        }

        _lines = Enumerable.Range(0, (int)LineCount).Select(i
                => _file.CreateViewAccessor(headerLength1 + i * LineCapacity, LineCapacity, MemoryMappedFileAccess.ReadWrite))
            .ToArray();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Dispose(true);
        GC.SuppressFinalize(this);
        _disposed = true;
    }

    protected static string ReadString(Span<byte> span)
    {
        if (span.IsEmpty)
            throw new Exception("String from empty span requested.");

        var termination = span.IndexOf((byte)'0');
        if (termination < 0)
            throw new Exception($"String in span is not terminated.");

        return Encoding.UTF8.GetString(span[..termination]);
    }

    protected static int WriteString(string text, Span<byte> span, int? maxLength = null)
    {
        var bytes  = Encoding.UTF8.GetBytes(text);
        var length = bytes.Length + 1;
        if (length > span.Length)
            throw new Exception($"String {text} is too long to write into span.");
        if (length > maxLength)
            throw new Exception($"String {text} is longer than allowed max length {maxLength.Value}.");

        bytes.CopyTo(span);
        span[bytes.Length] = 0;
        return length;
    }

    protected Span<byte> GetLine(int i)
    {
        if (i < 0 || i > LineCount)
            return null;

        lock (_header)
        {
            var lineIdx = (CurrentLinePosition + i) & _lineMask;
            if (lineIdx > CurrentLineCount)
                return null;

            return GetSpan(_lines[lineIdx]);
        }
    }


    protected MemoryMappedViewAccessor? GetCurrentLineLocking()
    {
        MemoryMappedViewAccessor view;
        lock (_header)
        {
            var currentLineCount = CurrentLineCount;
            if (currentLineCount == LineCount)
            {
                var currentLinePos = CurrentLinePosition;
                view                = _lines[currentLinePos]!;
                CurrentLinePosition = (currentLinePos + 1) & _lineMask;
            }
            else
            {
                view = _lines[currentLineCount - 1];
                ++CurrentLineCount;
            }

            _header.Flush();
        }

        return view;
    }

    protected static Span<byte> GetSpan(MemoryMappedViewAccessor accessor, int offset = 0)
        => GetSpan(accessor, offset, (int)accessor.Capacity - offset);

    protected static unsafe Span<byte> GetSpan(MemoryMappedViewAccessor accessor, int offset, int size)
    {
        byte* ptr = null;
        accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
        size = Math.Min(size, (int)accessor.Capacity - offset);
        if (size < 0)
            return [];

        var span = new Span<byte>(ptr + offset + accessor.PointerOffset, size);
        return span;
    }

    protected void Dispose(bool disposing)
    {
        _header.Dispose();
        foreach (var line in _lines)
            line.Dispose();
        _file.Dispose();
    }

    ~MemoryMappedBuffer()
        => Dispose(false);
}
