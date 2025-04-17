using System.Buffers;
using System.Text.Unicode;
using Lumina.Misc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OtterGui.String;

/// <summary> An UTF8 string container that is not a ref-struct and is guaranteed to be null-terminated. </summary>
/// <remarks> Using this with memory mapped files will lead to undefined behavior, since mapped pointers are used to identify literals. </remarks>
[JsonConverter(typeof(Converter))]
public readonly struct StringU8 : IReadOnlyList<byte>, IEquatable<StringU8>, IComparable<StringU8>,
    IComparisonOperators<StringU8, StringU8, bool>
{
    private static readonly ReadOnlyMemory<byte>                EmptyData        = new([0], 0, 0);
    private static readonly ConcurrentDictionary<nint, Manager> AssemblyManagers = [];

    public static readonly StringU8 Empty = new(EmptyData);

    private readonly ReadOnlyMemory<byte> _value;

    public ReadOnlySpan<byte> Span
        => _value.Span;

    public static implicit operator ReadOnlySpan<byte>(StringU8 s)
        => s.Span;

    public static explicit operator StringU8(ReadOnlySpan<byte> text)
        => new(text);

    [OverloadResolutionPriority(100)]
    public StringU8(byte[] data)
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
    public unsafe StringU8(ReadOnlySpan<byte> data, bool nullTerminated = true)
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
                var manager = AssemblyManagers.GetOrAdd(info.BaseAddress,
                    static (_, i) => new Manager(i.AllocationBase, (int)(i.BaseAddress - i.AllocationBase + i.RegionSize)),
                    info);
                _value = manager.Memory.Slice((int)(ptr - (byte*)info.AllocationBase), data.Length);
            }
        }
    }

    [OverloadResolutionPriority(75)]
    public unsafe StringU8(ReadOnlyMemory<byte> data, bool nullTerminated = true)
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

    [OverloadResolutionPriority(100)]
    public StringU8(ref Utf8InterpolatedStringHandler text)
    {
        _value = text.WriteAndClear();
        _value = _value[..^1];
    }

    [OverloadResolutionPriority(101)]
    public StringU8(IFormatProvider? provider,
        [InterpolatedStringHandlerArgument(nameof(provider))]
        ref Utf8InterpolatedStringHandler text)
        : this(ref text)
    { }

    [OverloadResolutionPriority(50)]
    public StringU8(ReadOnlySpan<char> utf16)
    {
        if (utf16.Length is 0)
        {
            _value = EmptyData;
            return;
        }

        var data  = ArrayPool<byte>.Shared.Rent(utf16.Length * 4 + 1);
        var count = Encoding.UTF8.GetBytes(utf16, data);
        var bytes = new byte[count + 1];
        bytes[count] = 0;
        data.AsSpan(0, count).CopyTo(bytes);
        _value = bytes;
    }

    public unsafe StringU8(byte* ptr)
        : this(FindNullTerminator(ptr))
    { }

    [OverloadResolutionPriority(100)]
    public bool Equals(StringU8 other)
        => Span.SequenceEqual(other);

    [OverloadResolutionPriority(50)]
    public bool Equals(ReadOnlySpan<byte> other)
        => Span.SequenceEqual(other);

    public override bool Equals(object? obj)
        => obj is StringU8 other && Equals(other);

    [OverloadResolutionPriority(100)]
    public int CompareTo(StringU8 other)
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
    private StringU8(ReadOnlyMemory<byte> data)
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

    public int Length
        => _value.Length;

    public byte this[int index]
        => _value.Span[index];

    public static bool operator ==(StringU8 left, StringU8 right)
        => left.Equals(right);

    public static bool operator !=(StringU8 left, StringU8 right)
        => !left.Equals(right);

    public static bool operator >(StringU8 left, StringU8 right)
        => left.CompareTo(right) > 0;

    public static bool operator >=(StringU8 left, StringU8 right)
        => left.CompareTo(right) >= 0;

    public static bool operator <(StringU8 left, StringU8 right)
        => left.CompareTo(right) < 0;

    public static bool operator <=(StringU8 left, StringU8 right)
        => left.CompareTo(right) <= 0;

    [InterpolatedStringHandler]
    public ref partial struct Utf8InterpolatedStringHandler
    {
        private const int ArrayPoolSize = 4 * 1024 * 1024;

        private readonly byte[]                                 _array;
        private          Utf8.TryWriteInterpolatedStringHandler _handler;
        private          bool                                   _success;
        private readonly bool                                   _hasCustomFormatter;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Utf8InterpolatedStringHandler(int literalLength, int formattedCount, out bool shouldAppend)
        {
            _array       = ArrayPool<byte>.Shared.Rent(ArrayPoolSize);
            _handler     = new Utf8.TryWriteInterpolatedStringHandler(literalLength, formattedCount, _array, out _success);
            shouldAppend = _success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Utf8InterpolatedStringHandler(int literalLength, int formattedCount, IFormatProvider? formatProvider, out bool shouldAppend)
        {
            _array       = ArrayPool<byte>.Shared.Rent(ArrayPoolSize);
            _handler     = new Utf8.TryWriteInterpolatedStringHandler(literalLength, formattedCount, _array, formatProvider, out _success);
            shouldAppend = _success;
        }

        public byte[] WriteAndClear()
        {
            if (Utf8.TryWrite([], ref _handler, out var written))
            {
                var ret = new byte[written + 1];
                ret[written] = 0;
                _array.AsSpan(0, written).CopyTo(ret);
                ArrayPool<byte>.Shared.Return(_array);
                return ret;
            }

            ArrayPool<byte>.Shared.Return(_array);
            throw new InvalidOperationException($"Interpolating a Utf8String using more than {ArrayPoolSize} bytes is not supported.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendLiteral(string value)
            => _handler.AppendLiteral(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormatted<TValue>(TValue value)
            => _handler.AppendFormatted(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormatted<TValue>(TValue value, string? format)
            => _handler.AppendFormatted(value, format);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormatted<TValue>(TValue value, int alignment)
            => _handler.AppendFormatted(value, alignment);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormatted<TValue>(TValue value, int alignment, string? format)
            => _handler.AppendFormatted(value, alignment, format);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormatted(scoped ReadOnlySpan<char> value)
            => _handler.AppendFormatted(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormatted(scoped ReadOnlySpan<char> value, int alignment = 0, string? format = null)
            => _handler.AppendFormatted(value, alignment, format);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormatted(scoped ReadOnlySpan<byte> utf8Value)
            => _handler.AppendFormatted(utf8Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormatted(scoped ReadOnlySpan<byte> utf8Value, int alignment = 0, string? format = null)
            => _handler.AppendFormatted(utf8Value, alignment, format);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormatted(string? value)
            => _handler.AppendFormatted(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormatted(string? value, int alignment = 0, string? format = null)
            => _handler.AppendFormatted(value, alignment, format);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormatted(object? value, int alignment = 0, string? format = null)
            => _handler.AppendFormatted(value, alignment, format);
    }

    /// <summary>
    /// Conversion from and to string.
    /// </summary>
    private class Converter : JsonConverter<StringU8>
    {
        public override void WriteJson(JsonWriter writer, StringU8 value, JsonSerializer serializer)
            => writer.WriteValue(value.ToString());

        public override StringU8 ReadJson(JsonReader reader, Type objectType, StringU8 existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var token = JToken.Load(reader).ToString();
            return new StringU8(token);
        }
    }
}
