using System.Reflection.Emit;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using OtterGui.Text.HelperObjects;

namespace OtterGui.Text;

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

public static partial class ImUtf8
{
    // No overloads for formatted string for tooltip because of evaluation.

    /// <summary> Draw a help marker followed by a label, both displaying a tooltip on hover. </summary>
    /// <param name="label"> The label text as a UTF8 string. Does not need to be null-terminated. </param>
    /// <param name="tooltip"> The tooltip text as a UTF8 string. Does not need to be null-terminated. </param>
    /// <param name="icon"> The icon to use. </param>
    /// <param name="withSameLine"> Whether the help marker starts on the same line as the last item. </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void LabeledHelpMarker(ReadOnlySpan<byte> label, ReadOnlySpan<byte> tooltip,
        FontAwesomeIcon icon = FontAwesomeIcon.InfoCircle, bool withSameLine = true)
    {
        if (withSameLine)
            ImGui.SameLine();

        Icon(icon, tooltip, ImGui.GetColorU32(ImGuiCol.TextDisabled));
        SameLineInner();
        Text(label);
        HoverTooltip(tooltip);
    }

    /// <param name="label"> The label text as a UTF16 string. </param>
    /// <inheritdoc cref="LabeledHelpMarker(ReadOnlySpan{byte},ReadOnlySpan{byte},FontAwesomeIcon,bool)"/>
    /// <exception cref="ImUtf8FormatException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void LabeledHelpMarker(ReadOnlySpan<char> label, ReadOnlySpan<byte> tooltip,
        FontAwesomeIcon icon = FontAwesomeIcon.InfoCircle, bool withSameLine = true)
        => LabeledHelpMarker(label.Span<LabelStringHandlerBuffer>(), tooltip, icon, withSameLine);

    /// <param name="label"> The label text as a formatted string. </param>
    /// <inheritdoc cref="LabeledHelpMarker(ReadOnlySpan{char},ReadOnlySpan{byte},FontAwesomeIcon,bool)"/>
    /// <exception cref="ImUtf8FormatException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void LabeledHelpMarker(ref Utf8StringHandler<LabelStringHandlerBuffer> label, ReadOnlySpan<byte> tooltip,
        FontAwesomeIcon icon = FontAwesomeIcon.InfoCircle, bool withSameLine = true)
        => LabeledHelpMarker(label.Span(), tooltip, icon, withSameLine);


    /// <param name="tooltip"> The tooltip text as a UTF16 string. </param>
    /// <inheritdoc cref="LabeledHelpMarker(ReadOnlySpan{byte},ReadOnlySpan{byte},FontAwesomeIcon,bool)"/>
    /// <exception cref="ImUtf8FormatException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void LabeledHelpMarker(ReadOnlySpan<byte> label, ReadOnlySpan<char> tooltip,
        FontAwesomeIcon icon = FontAwesomeIcon.InfoCircle, bool withSameLine = true)
    {
        if (withSameLine)
            ImGui.SameLine();

        Icon(icon, tooltip, ImGui.GetColorU32(ImGuiCol.TextDisabled));
        SameLineInner();
        Text(label);
        HoverTooltip(tooltip);
    }

    /// <param name="tooltip"> The tooltip text as a UTF16 string. </param>
    /// <inheritdoc cref="LabeledHelpMarker(ReadOnlySpan{char},ReadOnlySpan{byte},FontAwesomeIcon,bool)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void LabeledHelpMarker(ReadOnlySpan<char> label, ReadOnlySpan<char> tooltip,
        FontAwesomeIcon icon = FontAwesomeIcon.InfoCircle, bool withSameLine = true)
        => LabeledHelpMarker(label.Span<LabelStringHandlerBuffer>(), tooltip, icon, withSameLine);

    /// <param name="tooltip"> The tooltip text as a formatted string. </param>
    /// <inheritdoc cref="LabeledHelpMarker(ref Utf8StringHandler{LabelStringHandlerBuffer},ReadOnlySpan{byte},FontAwesomeIcon,bool)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void LabeledHelpMarker(ref Utf8StringHandler<LabelStringHandlerBuffer> label, ReadOnlySpan<char> tooltip,
        FontAwesomeIcon icon = FontAwesomeIcon.InfoCircle, bool withSameLine = true)
        => LabeledHelpMarker(label.Span(), tooltip, icon, withSameLine);
}
