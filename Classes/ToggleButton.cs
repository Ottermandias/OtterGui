using ImGuiNET;
using OtterGui.Internal;
using OtterGui.Internal.Enums;
using OtterGui.Internal.Structs;

namespace OtterGui.Classes;

public static class ToggleButton
{
    private const ImGuiButtonFlags AlignTextBaseLine = (ImGuiButtonFlags)(1 << 15);

    public static unsafe bool Draw(string label, string left, string right, ref bool value)
        => Draw(label, left, right, ref value, Vector2.Zero);

    public static unsafe bool ButtonEx(string label, Vector2 sizeArg, ImGuiButtonFlags flags, ImDrawFlags corners)
    {
        // Copied from ImGui proper.
        var window = ImGuiNativeAdditions.GetCurrentWindow();
        if (window->SkipItems)
            return false;

        var style     = ImGui.GetStyle();
        var id        = (ImGuiId)ImGui.GetID(label);
        var textSize  = ImGui.CalcTextSize(label, true);
        var screenPos = window->DC.CursorPos;
        // Try to vertically align buttons that are smaller/have no padding so that text baseline matches (bit hacky, since it shouldn't be a flag)
        if (flags.HasFlag(AlignTextBaseLine) && style.FramePadding.Y < window->DC.CurrLineTextBaseOffset)
            screenPos.Y += window->DC.CurrLineTextBaseOffset - style.FramePadding.Y;

        ImGuiNativeAdditions.CalcItemSize(out var size, sizeArg, textSize.X + style.FramePadding.X * 2, textSize.Y + style.FramePadding.Y * 2);
        var boundingBox = new ImRect(screenPos, screenPos + size);
        ImGuiNativeAdditions.ItemSize(boundingBox, style.FramePadding.Y);
        if (!ImGuiNativeAdditions.ItemAdd(boundingBox, id, null, 0))
            return false;

        // Custom.
        var clicked = ImGuiNativeAdditions.ButtonBehavior(boundingBox, id, out var hovered, out var held, flags);
        var color   = ImGui.GetColorU32(held ? ImGuiCol.ButtonActive : hovered ? ImGuiCol.ButtonHovered : ImGuiCol.Button);
        ImGuiNativeAdditions.RenderNavHighlight(boundingBox, id, 0);
        ImGui.GetWindowDrawList().AddRectFilled(boundingBox.Min, boundingBox.Max, color, style.FrameRounding, corners);
        WidgetUtils.RenderTextClipped(ImGui.GetWindowDrawList(), label, boundingBox.Min + style.FramePadding,
            boundingBox.Max - style.FramePadding, style.ButtonTextAlign, textSize, boundingBox, true);
        return clicked;
    }

    public static unsafe bool Draw(string label, string left, string right, ref bool value, Vector2 size)
    {
        var window = ImGuiNativeAdditions.GetCurrentWindow();
        if (window->SkipItems)
            return false;

        var id      = (ImGuiId)ImGui.GetID(label);
        var idLeft  = (ImGuiId)ImGui.GetID(label + 'l');
        var idRight = (ImGuiId)ImGui.GetID(label + 'r');

        var style     = ImGui.GetStyle();
        var leftSize  = ImGui.CalcTextSize(left);
        var rightSize = ImGui.CalcTextSize(right);
        var height    = size.Y <= 0 ? ImGui.GetFrameHeight() : size.Y;
        var width     = size.X <= 0 ? Math.Max(leftSize.X, rightSize.X) * 2 + 4 * style.FramePadding.X + style.ItemSpacing.X : size.X;
        var itemSize  = new Vector2(width, height);

        var screenPos   = window->DC.CursorPos;
        var boundingBox = new ImRect(screenPos, screenPos + itemSize);

        var center                = new Vector2(screenPos.X + width / 2, screenPos.Y);
        var checkBoundingBoxLeft  = new ImRect(screenPos, center + itemSize with { X = 0 });
        var checkBoundingBoxRight = new ImRect(center,    boundingBox.Max);

        ImGuiNativeAdditions.ItemSize(boundingBox, style.FramePadding.Y);
        if (!ImGuiNativeAdditions.ItemAdd(checkBoundingBoxLeft, idLeft, null, 0))
            return false;
        if (!ImGuiNativeAdditions.ItemAdd(checkBoundingBoxRight, idRight, null, 0))
            return false;

        var leftClicked = ImGuiNativeAdditions.ButtonBehavior(checkBoundingBoxLeft, idLeft, out var leftHovered, out var leftHeld,
            ImGuiButtonFlags.MouseButtonLeft);
        var rightClicked = ImGuiNativeAdditions.ButtonBehavior(checkBoundingBoxRight, idRight, out var rightHovered, out var rightHeld,
            ImGuiButtonFlags.MouseButtonDefault);

        var colorLeft = value
            ? leftHovered ? ImGui.GetColorU32(ImGuiCol.ButtonHovered) : ImGui.GetColorU32(ImGuiCol.Button)
            : ImGui.GetColorU32(ImGuiCol.ButtonActive);
        var colorRight = !value
            ? rightHovered ? ImGui.GetColorU32(ImGuiCol.ButtonHovered) : ImGui.GetColorU32(ImGuiCol.Button)
            : ImGui.GetColorU32(ImGuiCol.ButtonActive);
        ImGuiNativeAdditions.RenderNavHighlight(checkBoundingBoxLeft, idLeft, 0);
        ImGui.GetWindowDrawList().AddRectFilled(checkBoundingBoxLeft.Min, checkBoundingBoxLeft.Max, colorLeft, style.FrameRounding,
            ImDrawFlags.RoundCornersLeft);

        ImGuiNativeAdditions.RenderNavHighlight(checkBoundingBoxRight, idRight, 0);
        ImGui.GetWindowDrawList().AddRectFilled(checkBoundingBoxRight.Min, checkBoundingBoxRight.Max, colorRight, style.FrameRounding,
            ImDrawFlags.RoundCornersRight);

        var yPos      = checkBoundingBoxLeft.Min.Y + style.FramePadding.Y;
        var xPosLeft  = screenPos.X + (center.X - screenPos.X - leftSize.X) / 2;
        var xPosRight = center.X + (boundingBox.MaxX - center.X - rightSize.X) / 2;
        var color     = ImGui.GetColorU32(ImGuiCol.Text);
        WidgetUtils.AddText(ImGui.GetWindowDrawList(), new Vector2(xPosLeft,  yPos), color, left,  false);
        WidgetUtils.AddText(ImGui.GetWindowDrawList(), new Vector2(xPosRight, yPos), color, right, false);

        if (value && leftClicked)
        {
            value = false;
            return true;
        }

        if (!value && rightClicked)
        {
            value = true;
            return true;
        }

        return false;
    }
}
