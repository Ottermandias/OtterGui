using Dalamud.Bindings.ImGui;
using OtterGui.Text.HelperObjects;
using OtterGuiInternal;

namespace OtterGui.Text;

#pragma warning disable CS1573
public static partial class ImUtf8
{
    /// <summary> Draw text rotated by 90°. </summary>
    /// <param name="text"> The given text as a UTF8 string. Does not have to be null-terminated. </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RotatedText(ReadOnlySpan<byte> text, bool alignToFrame = false)
    {
        var dl             = ImGui.GetWindowDrawList();
        var startVertexIdx = (int)dl.VtxCurrentIdx;
        var screenPos      = ImGui.GetCursorScreenPos();
        dl.PushClipRectFullScreen();
        dl.AddText(screenPos, ImGui.GetColorU32(ImGuiCol.Text), text);
        dl.PopClipRect();

        var textSize     = CalcTextSize(text, false);
        var endVertexIdx = (int)dl.VtxCurrentIdx;
        var startVertex  = ImGui.GetWindowDrawList().VtxBuffer[startVertexIdx];
        var endVertex    = ImGui.GetWindowDrawList().VtxBuffer[endVertexIdx];
        var (offset, dummy) = alignToFrame
            ? (new Vector2(ImGui.GetStyle().FramePadding.Y,                   textSize.X),
                new Vector2(2 * ImGui.GetStyle().FramePadding.Y + textSize.Y, textSize.X))
            : (new Vector2(0, textSize.X), new Vector2(textSize.Y, textSize.X));
        offset += screenPos;
        for (var index = startVertexIdx; index < endVertexIdx; ++index)
        {
            ref var vertex = ref ImGui.GetWindowDrawList().VtxBuffer.Ref(index);
            vertex.Pos = ImGuiInternal.Rotate(vertex.Pos - screenPos, 0, -1) + offset;
        }

        ImGui.Dummy(dummy);
    }

    /// <param name="text"> The given text as a UTF16 string. </param>
    /// <inheritdoc cref="RotatedText(ReadOnlySpan{byte}, bool)"/>
    /// <exception cref="ImUtf8FormatException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RotatedText(ReadOnlySpan<char> text, bool alignToFrame = false)
        => RotatedText(text.Span<LabelStringHandlerBuffer>(), alignToFrame);

    /// <param name="text"> The given text as a formatted string. </param>
    /// <inheritdoc cref="RotatedText(ReadOnlySpan{char}, bool)"/>
    public static void RotatedText(ref Utf8StringHandler<TextStringHandlerBuffer> text, bool alignToFrame = false)
        => RotatedText(text.Span(), alignToFrame);
}
