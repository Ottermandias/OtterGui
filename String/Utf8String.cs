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
    IComparisonOperators<StringU8, StringU8, bool>, ISpanFormattable, IUtf8SpanFormattable
{
    internal const           int                                 ArrayPoolSize    = 4 * 1024 * 1024;
    internal const           int                                 HoleEstimate     = 128;
    internal static readonly ArrayPool<byte>                     ArrayPool        = ArrayPool<byte>.Shared;
    private static readonly  ReadOnlyMemory<byte>                EmptyData        = new([0], 0, 0);
    private static readonly  ConcurrentDictionary<nint, Manager> AssemblyManagers = [];

    public static readonly   StringU8 Empty      = new(EmptyData);
    internal static readonly StringU8 NullString = new("<NULL>"u8);

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
    // ReSharper disable once EntityNameCapturedOnly.Local
    public StringU8(IFormatProvider? provider, [InterpolatedStringHandlerArgument(nameof(provider))] ref Utf8InterpolatedStringHandler text)
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

        var data  = ArrayPool.Rent(utf16.Length * 4 + 1);
        var count = Encoding.UTF8.GetBytes(utf16, data);
        var bytes = new byte[count + 1];
        bytes[count] = 0;
        data.AsSpan(0, count).CopyTo(bytes);
        _value = bytes;
        ArrayPool.Return(data);
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
        return $"{nameof(_value)}: {_value}";
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
    public ref struct Utf8InterpolatedStringHandler
    {
        private readonly byte[]                                 _array;
        private          Utf8.TryWriteInterpolatedStringHandler _handler;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Utf8InterpolatedStringHandler(int literalLength, int formattedCount, out bool shouldAppend)
        {
            _array   = ArrayPool.Rent(ArrayPoolSize);
            _handler = new Utf8.TryWriteInterpolatedStringHandler(literalLength, formattedCount, _array, out shouldAppend);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Utf8InterpolatedStringHandler(int literalLength, int formattedCount, IFormatProvider? formatProvider, out bool shouldAppend)
        {
            _array   = ArrayPool.Rent(ArrayPoolSize);
            _handler = new Utf8.TryWriteInterpolatedStringHandler(literalLength, formattedCount, _array, formatProvider, out shouldAppend);
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

            ArrayPool.Return(_array);
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
        // ReSharper disable once MethodOverloadWithOptionalParameter
        public bool AppendFormatted(scoped ReadOnlySpan<char> value, int alignment = 0, string? format = null)
            => _handler.AppendFormatted(value, alignment, format);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormatted(scoped ReadOnlySpan<byte> utf8Value)
            => _handler.AppendFormatted(utf8Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once MethodOverloadWithOptionalParameter
        public bool AppendFormatted(scoped ReadOnlySpan<byte> utf8Value, int alignment = 0, string? format = null)
            => _handler.AppendFormatted(utf8Value, alignment, format);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormatted(string? value)
            => _handler.AppendFormatted(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once MethodOverloadWithOptionalParameter
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

    public static StringU8 Join(byte separator, params IReadOnlyCollection<StringU8> strings)
    {
        if (strings.Count is 0)
            return Empty;
        if (strings.Count is 1)
            return strings.First();

        var size  = (strings.Count - 1) * strings.Sum(s => s.Length) + 1;
        var array = new byte[size];
        array[size] = 0;
        var idx = 0;
        foreach (var word in strings.SkipLast(1))
        {
            word._value.CopyTo(array.AsMemory(idx));
            idx          += word.Length;
            array[idx++] =  separator;
        }

        strings.Last()._value.CopyTo(array.AsMemory(idx));

        return new StringU8(array);
    }

    public static StringU8 Join(ReadOnlySpan<byte> separator, params IReadOnlyCollection<StringU8> strings)
    {
        if (strings.Count is 0)
            return Empty;
        if (strings.Count is 1)
            return strings.First();

        var size  = (strings.Count - 1) * separator.Length * strings.Sum(s => s.Length) + 1;
        var array = new byte[size];
        array[size] = 0;
        var idx = 0;
        foreach (var word in strings.SkipLast(1))
        {
            word._value.CopyTo(array.AsMemory(idx));
            idx += word.Length;
            separator.CopyTo(array.AsSpan(idx));
            idx += separator.Length;
        }

        strings.Last()._value.CopyTo(array.AsMemory(idx));

        return new StringU8(array);
    }

    public static unsafe StringU8 Join(char separator, params IReadOnlyCollection<StringU8> strings)
    {
        var count = Encoding.UTF8.GetByteCount(&separator, 1);
        if (count is 1)
            return Join((byte)separator, strings);

        Span<byte> sep = stackalloc byte[4];
        sep = sep[..count];
        Encoding.UTF8.GetBytes(new ReadOnlySpan<char>(&separator, 1), sep);

        return Join(sep, strings);
    }

    public static StringU8 Join(ReadOnlySpan<char> separator, params IReadOnlyCollection<StringU8> strings)
    {
        var count = Encoding.UTF8.GetByteCount(separator);
        if (count is 1)
            return Join((byte)separator[0], strings);

        var array = ArrayPool.Rent(count);
        Encoding.UTF8.GetBytes(separator, array);

        var ret = Join(array.AsSpan(0, count), strings);
        ArrayPool.Return(array);
        return ret;
    }

    [OverloadResolutionPriority(50)]
    public static StringU8 Join(byte separator, params IReadOnlyCollection<object?> strings)
    {
        if (strings.Count is 0)
            return Empty;

        var array = ArrayPool.Rent(strings.Count * (1 + HoleEstimate));
        if (strings.Count is 1)
            return ToU8String(ref array, strings.First());

        var idx = 0;
        foreach (var text in strings.SkipLast(1))
        {
            AppendU8String(ref array, ref idx, text);
            var span = array.AsSpan(idx);
            if (span.Length < 1)
                ExchangeArray(ref array, array.Length * 2, idx);
            array[idx++] = separator;
        }

        AppendU8String(ref array, ref idx, strings.Last());
        return new StringU8(AddNull(array.AsSpan(0, idx)));
    }

    [OverloadResolutionPriority(50)]
    public static StringU8 Join(ReadOnlySpan<byte> separator, params IReadOnlyCollection<object?> strings)
    {
        if (strings.Count is 0)
            return Empty;

        var array = ArrayPool.Rent(strings.Count * (separator.Length + HoleEstimate));
        if (strings.Count is 1)
            return ToU8String(ref array, strings.First());

        var idx = 0;
        foreach (var text in strings.SkipLast(1))
        {
            AppendU8String(ref array, ref idx, text);
            while (!separator.TryCopyTo(array.AsSpan(idx)))
                ExchangeArray(ref array, array.Length * 2, idx);
            idx += separator.Length;
        }

        AppendU8String(ref array, ref idx, strings.Last());
        return new StringU8(AddNull(array.AsSpan(0, idx)));
    }

    [OverloadResolutionPriority(50)]
    public static unsafe StringU8 Join(char separator, params IReadOnlyCollection<object?> strings)
    {
        var count = Encoding.UTF8.GetByteCount(&separator, 1);
        if (count is 1)
            return Join((byte)separator, strings);

        Span<byte> sep = stackalloc byte[4];
        sep = sep[..count];
        Encoding.UTF8.GetBytes(new ReadOnlySpan<char>(&separator, 1), sep);

        return Join(sep, strings);
    }

    [OverloadResolutionPriority(50)]
    public static StringU8 Join(ReadOnlySpan<char> separator, params IReadOnlyCollection<object?> strings)
    {
        var count = Encoding.UTF8.GetByteCount(separator);
        if (count is 1)
            return Join((byte)separator[0], strings);

        var array = ArrayPool.Rent(count);
        Encoding.UTF8.GetBytes(separator, array);

        var ret = Join(array.AsSpan(0, count), strings);
        ArrayPool.Return(array);
        return ret;
    }

    [OverloadResolutionPriority(100)]
    public static StringU8 Join(byte separator, params IReadOnlyCollection<string?> strings)
    {
        if (strings.Count is 0)
            return Empty;

        var array = ArrayPool.Rent(strings.Count + strings.Sum(s => s?.Length ?? NullString.Length) * 4);
        if (strings.Count is 1)
            return ToU8String(ref array, strings.First());

        var idx = 0;
        foreach (var text in strings.SkipLast(1))
        {
            AppendU8String(ref array, ref idx, text);
            var span = array.AsSpan(idx);
            if (span.Length < 1)
                ExchangeArray(ref array, array.Length * 2, idx);
            array[idx++] = separator;
        }

        AppendU8String(ref array, ref idx, strings.Last());
        return new StringU8(AddNull(array.AsSpan(0, idx)));
    }

    [OverloadResolutionPriority(100)]
    public static StringU8 Join(ReadOnlySpan<byte> separator, params IReadOnlyCollection<string?> strings)
    {
        if (strings.Count is 0)
            return Empty;

        var array = ArrayPool.Rent(strings.Count * separator.Length + strings.Sum(s => s?.Length ?? NullString.Length) * 4);
        if (strings.Count is 1)
            return ToU8String(ref array, strings.First());

        var idx = 0;
        foreach (var text in strings.SkipLast(1))
        {
            AppendU8String(ref array, ref idx, text);
            while (!separator.TryCopyTo(array.AsSpan(idx)))
                ExchangeArray(ref array, array.Length * 2, idx);
            idx += separator.Length;
        }

        AppendU8String(ref array, ref idx, strings.Last());
        return new StringU8(AddNull(array.AsSpan(0, idx)));
    }

    [OverloadResolutionPriority(100)]
    public static unsafe StringU8 Join(char separator, params IReadOnlyCollection<string?> strings)
    {
        var count = Encoding.UTF8.GetByteCount(&separator, 1);
        if (count is 1)
            return Join((byte)separator, strings);

        Span<byte> sep = stackalloc byte[4];
        sep = sep[..count];
        Encoding.UTF8.GetBytes(new ReadOnlySpan<char>(&separator, 1), sep);

        return Join(sep, strings);
    }

    [OverloadResolutionPriority(100)]
    public static StringU8 Join(ReadOnlySpan<char> separator, params IReadOnlyCollection<string?> strings)
    {
        var count = Encoding.UTF8.GetByteCount(separator);
        if (count is 1)
            return Join((byte)separator[0], strings);

        var array = ArrayPool.Rent(count);
        Encoding.UTF8.GetBytes(separator, array);

        var ret = Join(array.AsSpan(0, count), strings);
        ArrayPool.Return(array);
        return ret;
    }


    private static void ExchangeArray(ref byte[] array, int minSize, int copyExistingLength)
    {
        if (minSize < array.Length)
            return;

        var newSize  = (int)BitOperations.RoundUpToPowerOf2((uint)minSize);
        var oldArray = array;
        array = ArrayPool.Rent(newSize);
        oldArray.AsMemory(0, copyExistingLength).CopyTo(array);
        ArrayPool.Return(oldArray);
    }

    private static StringU8 ToU8String(ref byte[] array, object? text)
    {
        int bytes;
        if (text is IUtf8SpanFormattable format)
        {
            while (!format.TryFormat(array, out bytes, string.Empty, null))
                ExchangeArray(ref array, array.Length * 2, 0);
            return new StringU8(AddNull(array.AsSpan(0, bytes)));
        }

        var asString = text is null ? null : text as string ?? text.ToString();
        if (asString is null)
            return NullString;

        while (!Encoding.UTF8.TryGetBytes(asString, array, out bytes))
            ExchangeArray(ref array, array.Length * 2, 0);
        return new StringU8(AddNull(array.AsSpan(0, bytes)));
    }

    private static void AppendU8String(ref byte[] array, ref int idx, object? text)
    {
        int bytes;
        if (text is IUtf8SpanFormattable format)
        {
            while (!format.TryFormat(array.AsSpan(idx), out bytes, string.Empty, null))
                ExchangeArray(ref array, array.Length * 2, idx);
        }
        else
        {
            var asString = text is null ? null : text as string ?? text.ToString();

            if (asString is null)
            {
                bytes = NullString.Length;
                while (!NullString._value.TryCopyTo(array.AsMemory(idx)))
                    ExchangeArray(ref array, array.Length * 2, idx);
            }
            else
            {
                while (!Encoding.UTF8.TryGetBytes(asString, array.AsSpan(idx), out bytes))
                    ExchangeArray(ref array, array.Length * 2, idx);
            }
        }

        idx += bytes;
    }

    private static StringU8 ToU8String(ref byte[] array, string? text)
    {
        if (text is null)
            return NullString;

        int bytes;
        while (!Encoding.UTF8.TryGetBytes(text, array, out bytes))
            ExchangeArray(ref array, array.Length * 2, 0);
        return new StringU8(AddNull(array.AsSpan(0, bytes)));
    }

    private static void AppendU8String(ref byte[] array, ref int idx, string? text)
    {
        int bytes;
        if (text is null)
        {
            bytes = NullString.Length;
            while (!NullString._value.TryCopyTo(array.AsMemory(idx)))
                ExchangeArray(ref array, array.Length * 2, idx);
        }
        else
        {
            while (!Encoding.UTF8.TryGetBytes(text, array.AsSpan(idx), out bytes))
                ExchangeArray(ref array, array.Length * 2, idx);
        }

        idx += bytes;
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
        => ToString();

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => Encoding.UTF8.TryGetChars(_value.Span, destination, out charsWritten);

    public bool TryFormat(Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (!_value.Span.TryCopyTo(destination))
        {
            bytesWritten = 0;
            return false;
        }

        bytesWritten = Length;
        return true;
    }
}
