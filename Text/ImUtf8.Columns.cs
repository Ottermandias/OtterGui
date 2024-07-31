using OtterGui.Text.EndObjects;
using OtterGui.Text.HelperObjects;

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

namespace OtterGui.Text;

public static partial class ImUtf8
{
    /// <summary> Begins a multi-column layout and ends it on leaving scope. </summary>
    /// <param name="count"> The requested number of columns. </param>
    /// <param name="id"> The layout identifier as a UTF8 string. This will not be pushed on the ID stack and is only used to retain user resizing state. HAS to be null-terminated. </param>
    /// <param name="border"> Whether to display borders between columns. </param>
    /// <returns> A disposable object that ends the multi-column layout on disposal. Use with using. </returns>
    /// <remarks> Multi-column layouts cannot be nested without using <see cref="Child(ReadOnlySpan{byte}, Vector2, bool, ImGuiNET.ImGuiWindowFlags)"/>. </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Columns Columns(int count, ReadOnlySpan<byte> id, bool border = true)
        => new(count, id, border);

    /// <inheritdoc cref="Columns(int, ReadOnlySpan{byte}, bool)"/>
    /// <exception cref="ImUtf8FormatException" />
    /// <param name="id"> The layout identifier as a UTF16 string. This will not be pushed on the ID stack and is only used to retain user resizing state. </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Columns Columns(int count, ReadOnlySpan<char> id, bool border = true)
        => new(count, id.Span<LabelStringHandlerBuffer>(), border);

    /// <inheritdoc cref="Columns(int, ReadOnlySpan{byte}, bool)"/>
    /// <exception cref="ImUtf8FormatException" />
    /// <param name="id"> The layout identifier as a formatted string. This will not be pushed on the ID stack and is only used to retain user resizing state. </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Columns Columns(int count, ref Utf8StringHandler<LabelStringHandlerBuffer> id, bool border = true)
        => new(count, id.Span(), border);

    /// <inheritdoc cref="Columns(int, ReadOnlySpan{byte}, bool)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Columns Columns(int count, bool border = true)
        => new(count, default, border);
}
