using ImGuiNET;
using OtterGui.Text.HelperObjects;
using OtterGuiInternal;

namespace OtterGui.Text;

#pragma warning disable CS1573
public static unsafe partial class ImUtf8
{
    /// <summary> Draw text rotated by 90°. </summary>
    /// <param name="text"> The given text as a UTF8 string. Does not have to be null-terminated. </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RotatedText(ReadOnlySpan<byte> text, bool alignToFrame = false)
    {
        var dl          = ImGui.GetWindowDrawList().NativePtr;
        var startVertexIdx = (int)dl->_VtxCurrentIdx;
        var screenPos   = ImGui.GetCursorScreenPos();
        fixed (byte* ptr = text)
        {
            ImGuiNative.ImDrawList_PushClipRectFullScreen(dl);
            ImGuiNative.ImDrawList_AddText_Vec2(dl, screenPos, ImGui.GetColorU32(ImGuiCol.Text), ptr, ptr + text.Length);
            ImGuiNative.ImDrawList_PopClipRect(dl);
        }

        var textSize     = CalcTextSize(text, false);
        var endVertexIdx = (int)dl->_VtxCurrentIdx;
        var startVertex  = ImGui.GetWindowDrawList().VtxBuffer[startVertexIdx].NativePtr;
        var endVertex    = ImGui.GetWindowDrawList().VtxBuffer[endVertexIdx].NativePtr;
        var (offset, dummy) = alignToFrame
            ? (new Vector2(ImGui.GetStyle().FramePadding.Y, textSize.X), new Vector2(2 * ImGui.GetStyle().FramePadding.Y + textSize.Y, textSize.X))
            : (new Vector2(0,                               textSize.X), new Vector2(textSize.Y, textSize.X));
        offset += screenPos;
        for (var vertex = startVertex; vertex < endVertex; ++vertex)
            vertex->pos = ImGuiInternal.Rotate(vertex->pos - screenPos, 0, -1) + offset;

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
