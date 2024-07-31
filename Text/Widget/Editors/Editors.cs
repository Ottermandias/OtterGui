using ImGuiNET;
using OtterGui.Text.HelperObjects;
using OtterGui.Widgets;

namespace OtterGui.Text.Widget.Editors;

/// <summary>
/// Utility and extension methods related to <see cref="IEditor{T}"/>.
/// </summary>
public static class Editors
{
    /// <summary>
    /// Provides a default editor suitable for <see cref="float"/> values.
    /// </summary>
    public static readonly IEditor<float> DefaultFloat = DragEditor<float>.CreateFloat(null, null, 0.1f, 0.0f, 3, default(ReadOnlySpan<byte>), 0);

    /// <summary>
    /// Provides a default editor suitable for <see cref="int"/> values.
    /// </summary>
    public static readonly IEditor<int> DefaultInt = DragEditor<int>.CreateInteger(null, null, 0.1f, 0.0f, default(ReadOnlySpan<byte>), 0);

    /// <summary>
    /// Adapts a <see cref="MultiStateCheckbox{T}"/> as an <see cref="IEditor{T}"/>.
    /// </summary>
    /// <typeparam name="T"> The type of the editable value. </typeparam>
    /// <param name="inner"> A <see cref="MultiStateCheckbox{T}"/>. </param>
    /// <returns> An <see cref="IEditor{T}"/> adapter for the given checkbox. </returns>
    public static IEditor<T> AsEditor<T>(this MultiStateCheckbox<T> inner) where T : unmanaged
        => new MultiStateCheckboxEditor<T>(inner);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ComponentHelper PrepareMultiComponent(int numComponents)
    {
        var spacing        = ImGui.GetStyle().ItemInnerSpacing.X;
        var componentWidth = (ImGui.CalcItemWidth() - (numComponents - 1) * spacing) / numComponents;
        return new(spacing, componentWidth);
    }

    internal static FormatBuffer GenerateIntegerFormat<T>(bool hex, scoped ReadOnlySpan<byte> unit) where T : unmanaged, INumber<T>
    {
        var buffer = new FormatBuffer();
        var writer = new SpanTextWriter(buffer);
        try
        {
            AppendIntegerFormat<T>(ref writer, hex);
            if (unit.Length > 0)
            {
                writer.TryAppend((byte)' ');
                TryAppendPrintfLiteral(ref writer, unit);
            }
        }
        finally
        {
            writer.EnsureNullTerminated();
        }

        return buffer;
    }

    internal static FormatBuffer GenerateIntegerFormat<T>(bool hex, scoped ReadOnlySpan<char> unit) where T : unmanaged, INumber<T>
    {
        var buffer = new FormatBuffer();
        var writer = new SpanTextWriter(buffer);
        try
        {
            AppendIntegerFormat<T>(ref writer, hex);
            if (unit.Length > 0)
            {
                writer.TryAppend((byte)' ');
                TryAppendPrintfLiteral(ref writer, unit);
            }
        }
        finally
        {
            writer.EnsureNullTerminated();
        }

        return buffer;
    }

    internal static FormatBuffer GenerateFloatFormat<T>(byte precision, scoped ReadOnlySpan<byte> unit) where T : unmanaged, INumber<T>
    {
        var buffer = new FormatBuffer();
        var writer = new SpanTextWriter(buffer);
        try
        {
            AppendFloatFormat<T>(ref writer, precision);
            if (unit.Length > 0)
            {
                writer.TryAppend((byte)' ');
                TryAppendPrintfLiteral(ref writer, unit);
            }
        }
        finally
        {
            writer.EnsureNullTerminated();
        }

        return buffer;
    }

    internal static FormatBuffer GenerateFloatFormat<T>(byte precision, scoped ReadOnlySpan<char> unit) where T : unmanaged, INumber<T>
    {
        var buffer = new FormatBuffer();
        var writer = new SpanTextWriter(buffer);
        try
        {
            AppendFloatFormat<T>(ref writer, precision);
            if (unit.Length > 0)
            {
                writer.TryAppend((byte)' ');
                TryAppendPrintfLiteral(ref writer, unit);
            }
        }
        finally
        {
            writer.EnsureNullTerminated();
        }

        return buffer;
    }

    /// <remarks> Takes 3 to 7 bytes. </remarks>
    private static void AppendIntegerFormat<T>(ref SpanTextWriter writer, bool hex) where T : unmanaged, INumber<T>
        => writer.Append(ImUtf8.Type<T>() switch
        {
            ImGuiDataType.U8  => hex ? "%02hhX"u8  : "%hhu"u8,
            ImGuiDataType.S8  => hex ? "%02hhX"u8  : "%hhd"u8,
            ImGuiDataType.U16 => hex ? "%04hX"u8   : "%hu"u8,
            ImGuiDataType.S16 => hex ? "%04hX"u8   : "%hd"u8,
            ImGuiDataType.U32 => hex ? "%08lX"u8   : "%lu"u8,
            ImGuiDataType.S32 => hex ? "%08lX"u8   : "%ld"u8,
            ImGuiDataType.U64 => hex ? "%016llX"u8 : "%llu"u8,
            ImGuiDataType.S64 => hex ? "%016llX"u8 : "%lld"u8,
            _                 => throw new NotSupportedException($"Unsupported integer type {typeof(T)}"),
        });

    /// <remarks> Takes 4 bytes. </remarks>
    private static void AppendFloatFormat<T>(ref SpanTextWriter writer, byte precision) where T : unmanaged, INumber<T>
    {
        if (ImUtf8.Type<T>() is not ImGuiDataType.Float and not ImGuiDataType.Double)
            throw new NotSupportedException($"Unsupported floating-point type {typeof(T)}");

        writer.Append([(byte)'%', (byte)'.', (byte)(48 + Math.Min(precision, (byte)9)), (byte)'f']);
    }

    /// <remarks> Takes <code>literal.Length</code> to <code>2 * literal.Length</code> bytes. </remarks>
    public static bool TryAppendPrintfLiteral(ref SpanTextWriter writer, scoped ReadOnlySpan<byte> literal)
    {
        for (; ; )
        {
            var pos = literal.IndexOf((byte)'%');
            if (pos < 0)
                break;

            if (!writer.TryAppend(literal[..pos], out _))
                return false;
            if (!writer.TryAppend([(byte)'%', (byte)'%'], out _))
                return false;
            literal = literal[(pos + 1)..];
        }
        
        return writer.TryAppend(literal, out _);
    }

    /// <remarks> Takes <code>literal.Length</code> to <code>2 * literal.Length</code> bytes. </remarks>
    public static bool TryAppendPrintfLiteral(ref SpanTextWriter writer, scoped ReadOnlySpan<char> literal)
    {
        for (; ; )
        {
            var pos = literal.IndexOf('%');
            if (pos < 0)
                break;

            if (!writer.TryAppend(literal[..pos], out _))
                return false;
            if (!writer.TryAppend([(byte)'%', (byte)'%'], out _))
                return false;
            literal = literal[(pos + 1)..];
        }

        return writer.TryAppend(literal, out _);
    }

    [InlineArray(Size)]
    public struct FormatBuffer
    {
        // This should be plenty for most formats: for the formats defined above, that leaves 23 bytes for the unit.
        public const int Size = 32;

        private byte _element0;

        public FormatBuffer(scoped ReadOnlySpan<byte> format)
        {
            var writer = new SpanTextWriter(this);
            try
            {
                writer.TryAppend(format, out _, true);
            }
            finally
            {
                writer.EnsureNullTerminated();
            }
        }

        public FormatBuffer(scoped ReadOnlySpan<char> format)
        {
            var writer = new SpanTextWriter(this);
            try
            {
                writer.TryAppend(format, out _, true);
            }
            finally
            {
                writer.EnsureNullTerminated();
            }
        }
    }

    [InlineArray(Size)]
    public struct IdBuffer
    {
        // "###", widest integer (11), "\0", rounded up to power of 2
        public const int Size = 16;

        private byte _element0;
    }

    public struct ComponentHelper
    {
        public readonly float    Spacing;
        public readonly float    ComponentWidth;
        public          IdBuffer Id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ComponentHelper(float spacing, float componentWidth)
        {
            Spacing        = spacing;
            ComponentWidth = componentWidth;

            Id[0] = (byte)'#';
            Id[1] = (byte)'#';
            Id[2] = (byte)'#';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetupComponent(int index)
        {
            if (index > 0)
                ImGui.SameLine(0.0f, Spacing);

            ImGui.SetNextItemWidth(MathF.Round(ComponentWidth * (index + 1)) - MathF.Round(ComponentWidth * index));

            if (!index.TryFormat(Id[3..], out var bytesWritten))
                throw new ImUtf8FormatException();
            Id[3 + bytesWritten] = 0;
        }
    }
}
