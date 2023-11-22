using ImGuiNET;
using OtterGui.Internal;
using OtterGui.Internal.Enums;
using OtterGui.Internal.Structs;

namespace OtterGui.Classes;

/// <summary>
/// Draw a checkbox that toggles forward or backward between different states.
/// </summary>
public abstract class MultiStateCheckbox<T>
{
    /// <summary> Render the symbol corresponding to <paramref name="value"/> starting at <paramref name="position"/> and with <paramref name="size"/> as box size. </summary>
    protected abstract void RenderSymbol(T value, Vector2 position, float size);

    protected abstract T NextValue(T value);
    protected abstract T PreviousValue(T value);

    public unsafe bool Draw(string label, T currentValue, out T newValue)
    {
        newValue = currentValue;
        var window = ImGuiNativeAdditions.GetCurrentWindow();
        if (window->SkipItems)
            return false;

        var id = (ImGuiId)ImGui.GetID(label);

        // Calculate the bounding box of the checkbox including the label.
        var squareSize = ImGui.GetFrameHeight();
        var labelSize  = ImGui.CalcTextSize(label, true);
        var style      = ImGui.GetStyle();
        var screenPos  = window->DC.CursorPos;
        var itemSize = new Vector2(squareSize + (labelSize.X > 0 ? style.ItemInnerSpacing.X + labelSize.X : 0),
            labelSize.Y + style.FramePadding.Y * 2);
        var boundingBox = new ImRect(screenPos, screenPos + itemSize);

        // Add the item to internals. Skip it if it is clipped.
        ImGuiNativeAdditions.ItemSize(boundingBox, style.FramePadding.Y);
        if (!ImGuiNativeAdditions.ItemAdd(boundingBox, id, null, 0))
            return false;

        // Handle user interaction.
        var returnValue = ImGuiNativeAdditions.ButtonBehavior(boundingBox, id, out var hovered, out var held, 0);
        var rightClick  = ImGui.IsItemClicked(ImGuiMouseButton.Right);
        if (rightClick)
            newValue = PreviousValue(currentValue);
        else if (returnValue)
            newValue = NextValue(currentValue);

        // Draw the checkbox.
        var checkBoundingBox = new ImRect(screenPos, screenPos + new Vector2(squareSize));
        ImGuiNativeAdditions.RenderNavHighlight(boundingBox, id, 0);
        ImGuiNativeAdditions.RenderFrame(checkBoundingBox.Min, checkBoundingBox.Max, WidgetUtils.GetFrameBg(hovered, held), true,
            style.FrameRounding);

        // Draw the desired symbol into the checkbox.
        var paddingSize = Math.Max(1, (int)(squareSize / 6));
        RenderSymbol(currentValue, screenPos + new Vector2(paddingSize), squareSize - paddingSize * 2);

        // Add the label if there is one visible.
        if (labelSize.X > 0)
        {
            var labelPos = new Vector2(checkBoundingBox.MaxX + style.ItemInnerSpacing.X, checkBoundingBox.MinY + style.FramePadding.Y);
            WidgetUtils.AddText(ImGui.GetWindowDrawList(), labelPos, ImGui.GetColorU32(ImGuiCol.Text), label, true);
        }

        return returnValue || rightClick;
    }
}

public class TristateCheckbox : MultiStateCheckbox<sbyte>
{
    public readonly uint CrossColor;
    public readonly uint CheckColor;
    public readonly uint DotColor;

    private static uint MergeAlpha(uint color)
        => (color & 0x00FFFFFF) | ((uint)((color >> 24) * ImGui.GetStyle().Alpha) << 24);

    public TristateCheckbox(uint crossColor = 0xFF0000FF, uint checkColor = 0xFF00FF00, uint dotColor = 0xFFD0D0D0)
    {
        CrossColor = MergeAlpha(crossColor);
        CheckColor = MergeAlpha(checkColor);
        DotColor   = MergeAlpha(dotColor);
    }

    protected override void RenderSymbol(sbyte value, Vector2 position, float size)
    {
        switch (value)
        {
            case -1:
                WidgetUtils.RenderCross(ImGui.GetWindowDrawList(), position, CrossColor, size);
                break;
            case 1:
                WidgetUtils.RenderCheckmark(ImGui.GetWindowDrawList(), position, CheckColor, size);
                break;
            default:
                WidgetUtils.RenderDot(ImGui.GetWindowDrawList(), position, DotColor, size);
                break;
        }
    }

    protected override sbyte NextValue(sbyte value)
        => value switch
        {
            0 => 1,
            1 => -1,
            _ => 0,
        };

    protected override sbyte PreviousValue(sbyte value)
        => value switch
        {
            0 => -1,
            1 => 0,
            _ => 1,
        };
}
