using Dalamud.Interface;
using Dalamud.Bindings.ImGui;
using OtterGui.Raii;
using OtterGui.Text.HelperObjects;

namespace OtterGui.Text;

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

public static partial class ImUtf8
{
    // No overloads for formatted string for tooltip because of evaluation.

    /// <summary> Draw a help marker right-aligned on the same line as the last object, usually for a selectable, and display a tooltip when this item is hovered. </summary>
    /// <param name="tooltip"> The tooltip text as a UTF8 string. Does not need to be null-terminated. </param>
    /// <param name="icon"> The icon to use. </param>
    public static void SelectableHelpMarker(ReadOnlySpan<byte> tooltip, FontAwesomeIcon icon = FontAwesomeIcon.InfoCircle)
    {
        var hovered = ImGui.IsItemHovered();
        ImGui.SameLine();
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            using var color = ImRaii.PushColor(ImGuiCol.Text, ImGui.GetColorU32(ImGuiCol.TextDisabled));
            TextRightAligned(FontAwesomeIcon.InfoCircle.Bytes().Span, ImGui.GetStyle().ItemSpacing.X);
        }

        if (!hovered)
            return;

        using var tt = Tooltip();
        Text(tooltip);
    }

    /// <param name="tooltip"> The tooltip text as a UTF16 string. </param>
    /// <inheritdoc cref="SelectableHelpMarker(ReadOnlySpan{byte},FontAwesomeIcon)"/>
    /// <exception cref="ImUtf8FormatException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SelectableHelpMarker(ReadOnlySpan<char> tooltip, FontAwesomeIcon icon = FontAwesomeIcon.InfoCircle)
        => SelectableHelpMarker(tooltip.Span<LabelStringHandlerBuffer>(), icon);
}
