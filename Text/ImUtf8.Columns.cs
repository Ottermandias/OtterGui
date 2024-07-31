using OtterGui.Text.EndObjects;
using OtterGui.Text.HelperObjects;

namespace OtterGui.Text;

public static unsafe partial class ImUtf8
{
    /// <inheritdoc cref="Columns(int, ReadOnlySpan{byte}, bool)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Columns Columns(int count, bool border = true)
        => new(count, default, border);

    /// <summary> Begins a multi-column layout and ends it on leaving scope. </summary>
    /// <param name="count"> The wanted number of columns. </param>
    /// <param name="id"> The layout identifier. This will not be pushed on the ID stack and is only used to retain user resizing state. HAS to be null-terminated. </param>
    /// <param name="border"> Whether to display borders between columns. </param>
    /// <returns> A disposable object that ends the multi-column layout on disposal. Use with using. </returns>
    /// <remarks> Multi-column layouts cannot be nested without using <see cref="Child(ReadOnlySpan{byte}, Vector2, bool, ImGuiNET.ImGuiWindowFlags)"/>. </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Columns Columns(int count, ReadOnlySpan<byte> id, bool border = true)
        => new(count, id, border);

    /// <inheritdoc cref="Columns(int, ReadOnlySpan{byte}, bool)"/>
    /// <exception cref="ImUtf8FormatException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Columns Columns(int count, ReadOnlySpan<char> id, bool border = true)
        => new(count, id.Span<LabelStringHandlerBuffer>(), border);

    /// <inheritdoc cref="Columns(int, ReadOnlySpan{byte}, bool)"/>
    /// <exception cref="ImUtf8FormatException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Columns Columns(int count, ref Utf8StringHandler<LabelStringHandlerBuffer> id, bool border = true)
        => new(count, id.Span(), border);
}
