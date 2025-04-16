using System.Buffers;
using Lumina.Misc;

namespace OtterGui.Utf8;

/// <summary> An UTF8 string container that is not a ref-struct and is guaranteed to be null-terminated. </summary>
/// <remarks> </remarks>
public readonly struct Utf8String : IReadOnlyList<byte>, IEquatable<Utf8String>, IEqualityOperators<Utf8String, Utf8String, bool>,
    IComparable<Utf8String>, IComparisonOperators<Utf8String, Utf8String, bool>
{
    private static readonly ReadOnlyMemory<byte>                EmptyData        = new([0], 0, 0);
    private static readonly ConcurrentDictionary<nint, Manager> AssemblyManagers = [];

    public static readonly Utf8String Empty = new(EmptyData);

    private readonly ReadOnlyMemory<byte> _value;

    public ReadOnlySpan<byte> Span
        => _value.Span;

    public static implicit operator ReadOnlySpan<byte>(in Utf8String s)
        => s.Span;

    [OverloadResolutionPriority(100)]
    public Utf8String(byte[] data)
    {
        if (data.Length is 0)
        {
            _value = EmptyData;
            return;
        }

        if (data[^1] is 0)
            _value = data.Length is 1
                ? EmptyData
                : new Memory<byte>(data, 0, data.Length - 1);
        else
            _value = AddNull(data);
    }

    [OverloadResolutionPriority(50)]
    public unsafe Utf8String(ReadOnlySpan<byte> data, bool nullTerminated = true)
    {
        if (data.Length is 0)
        {
            _value = EmptyData;
            return;
        }

        if (!nullTerminated)
        {
            _value = AddNull(data);
            return;
        }

        fixed (byte* ptr = data)
        {
            if (ptr[data.Length] is not 0)
                throw new ArgumentException("UTF8 string is supposedly null-terminated but is not.");

            if (!Interop.VirtualQuery((nint)ptr, out var info) || !info.Type.HasFlag(Interop.MemoryType.Mapped))
            {
                _value = AddNull(data);
            }
            else
            {
                var manager = AssemblyManagers.GetOrAdd(info.BaseAddress, static (_, i) => new Manager(i.BaseAddress, (int)i.RegionSize),
                    info);
                _value = manager.Memory.Slice((int)(ptr - (byte*)info.BaseAddress), data.Length);
            }
        }
    }

    [OverloadResolutionPriority(75)]
    public unsafe Utf8String(ReadOnlyMemory<byte> data, bool nullTerminated = true)
    {
        if (data.Length is 0)
        {
            _value = EmptyData;
            return;
        }

        if (!nullTerminated)
        {
            _value = AddNull(data.Span);
            return;
        }

        fixed (byte* ptr = data.Span)
        {
            if (ptr[data.Length] is not 0)
                throw new ArgumentException("UTF8 string is supposedly null-terminated but is not.");

            _value = data;
        }
    }

    public unsafe Utf8String(byte* ptr)
        : this(FindNullTerminator(ptr))
    { }

    [OverloadResolutionPriority(100)]
    public bool Equals(Utf8String other)
        => Span.SequenceEqual(other);

    [OverloadResolutionPriority(50)]
    public bool Equals(ReadOnlySpan<byte> other)
        => Span.SequenceEqual(other);

    public override bool Equals(object? obj)
        => obj is Utf8String other && Equals(other);

    [OverloadResolutionPriority(100)]
    public int CompareTo(Utf8String other)
        => Span.SequenceCompareTo(other);

    [OverloadResolutionPriority(50)]
    public int CompareTo(ReadOnlySpan<byte> other)
        => Span.SequenceCompareTo(other);

    public bool StartsWith(ReadOnlySpan<byte> other)
        => Span.StartsWith(other);

    public bool EndsWith(ReadOnlySpan<byte> other)
        => Span.EndsWith(other);


    public override string ToString()
    {
        if (Count is 0)
            return string.Empty;

        return Encoding.UTF8.GetString(Span);
    }

    public override int GetHashCode()
        => (int)Crc32.Get(Span);

    private static ReadOnlyMemory<byte> AddNull(ReadOnlySpan<byte> data)
    {
        var ret = new byte[data.Length + 1];
        ret[data.Length] = 0;
        data.CopyTo(ret);
        return new Memory<byte>(ret, 0, data.Length);
    }

    private static unsafe ReadOnlySpan<byte> FindNullTerminator(byte* ptr)
        => MemoryMarshal.CreateReadOnlySpanFromNullTerminated(ptr);

    [OverloadResolutionPriority(1000)]
    private Utf8String(ReadOnlyMemory<byte> data)
        => _value = data;

    private sealed unsafe class Manager(nint allocationBase, int allocationSize) : MemoryManager<byte>
    {
        protected override void Dispose(bool disposing)
        { }

        public override Span<byte> GetSpan()
            => new((void*)allocationBase, allocationSize);

        public override MemoryHandle Pin(int elementIndex = 0)
            => new((void*)(allocationBase + elementIndex));


        public override void Unpin()
        { }
    }

    public IEnumerator<byte> GetEnumerator()
    {
        for (var i = 0; i < _value.Length; ++i)
            yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public int Count
        => _value.Length;

    public byte this[int index]
        => _value.Span[index];

    public static bool operator ==(Utf8String left, Utf8String right)
        => left.Equals(right);

    public static bool operator !=(Utf8String left, Utf8String right)
        => !left.Equals(right);

    public static bool operator >(Utf8String left, Utf8String right)
        => left.CompareTo(right) > 0;

    public static bool operator >=(Utf8String left, Utf8String right)
        => left.CompareTo(right) >= 0;

    public static bool operator <(Utf8String left, Utf8String right)
        => left.CompareTo(right) < 0;

    public static bool operator <=(Utf8String left, Utf8String right)
        => left.CompareTo(right) <= 0;
}
