using ImGuiNET;
using OtterGuiInternal.Utility;

namespace OtterGui.Text.Widget;

#pragma warning disable CS1573
/// <summary> A two-state Checkbox that displays either a checkmark for True values or an X for False values, with no empty state. </summary>
public sealed class TwoStateCheckbox : MultiStateCheckbox<bool>
{
    /// <summary> A static instance to draw more easily. </summary>
    public static readonly TwoStateCheckbox Instance = new();

    /// <inheritdoc/>
    protected override void RenderSymbol(bool value, Vector2 position, float size)
    {
        if (value)
            SymbolHelpers.RenderCheckmark(ImGui.GetWindowDrawList(), position, ImGui.GetColorU32(ImGuiCol.CheckMark), size);
        else
            SymbolHelpers.RenderCross(ImGui.GetWindowDrawList(), position, ImGui.GetColorU32(ImGuiCol.CheckMark), size);
    }

    /// <inheritdoc/>
    protected override bool NextValue(bool value)
        => !value;

    /// <inheritdoc/>
    protected override bool PreviousValue(bool value)
        => !value;
}
