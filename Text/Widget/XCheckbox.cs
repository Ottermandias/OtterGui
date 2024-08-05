using ImGuiNET;
using OtterGuiInternal.Utility;

namespace OtterGui.Text.Widget;

#pragma warning disable CS1573
/// <summary> A two-state Checkbox that displays either an X for True values and empty for False values. </summary>
public sealed class XCheckbox : MultiStateCheckbox<bool>
{
    /// <summary> A static instance to draw more easily. </summary>
    public static readonly XCheckbox Instance = new();

    /// <inheritdoc/>
    protected override void RenderSymbol(bool value, Vector2 position, float size)
    {
        if (value)
            SymbolHelpers.RenderCross(ImGui.GetWindowDrawList(), position, ImGui.GetColorU32(ImGuiCol.CheckMark), size);
    }

    /// <inheritdoc/>
    protected override bool NextValue(bool value)
        => !value;

    /// <inheritdoc/>
    protected override bool PreviousValue(bool value)
        => !value;
}
