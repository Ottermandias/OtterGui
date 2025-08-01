using Dalamud.Bindings.ImGui;
using OtterGui.Text;
using OtterGuiInternal;
using OtterGuiInternal.Enums;
using OtterGuiInternal.Structs;
using OtterGuiInternal.Utility;

namespace OtterGui.Widgets;

public static class ToggleButton
{
    private const ImGuiButtonFlags AlignTextBaseLine = (ImGuiButtonFlags)(1 << 15);

    public static bool ButtonEx(ReadOnlySpan<char> label, Vector2 sizeArg, ImGuiButtonFlags flags, ImDrawFlags corners)
    {
        // Copied from ImGui proper.
        var window = ImGuiInternal.GetCurrentWindow();
        if (window.SkipItems)
            return false;

        var style = ImGui.GetStyle();
        var (visibleEnd, textSize, id) = StringHelpers.ComputeSizeAndId(label);
        var screenPos = window.DC.CursorPos;
        // Try to vertically align buttons that are smaller/have no padding so that text baseline matches (bit hacky, since it shouldn't be a flag)
        if (flags.HasFlag(AlignTextBaseLine) && style.FramePadding.Y < window.DC.CurrLineTextBaseOffset)
            screenPos.Y += window.DC.CurrLineTextBaseOffset - style.FramePadding.Y;

        var size        = ImGuiInternal.CalcItemSize(sizeArg, textSize.X + style.FramePadding.X * 2, textSize.Y + style.FramePadding.Y * 2);
        var boundingBox = new ImRect(screenPos, screenPos + size);
        ImGuiInternal.ItemSize(boundingBox, style.FramePadding.Y);
        if (!ImGuiInternal.ItemAdd(boundingBox, id))
            return false;

        // Custom.
        var clicked = ImGuiInternal.ButtonBehavior(boundingBox, id, out var hovered, out var held, flags);
        var color   = ImGui.GetColorU32(held ? ImGuiCol.ButtonActive : hovered ? ImGuiCol.ButtonHovered : ImGuiCol.Button);
        ImGuiInternal.RenderNavHighlight(boundingBox, id, 0);
        ImGui.GetWindowDrawList().AddRectFilled(boundingBox.Min, boundingBox.Max, color, style.FrameRounding, corners);
        ImGuiInternal.RenderTextClipped(ImGui.GetWindowDrawList(), boundingBox.Min + style.FramePadding, boundingBox.Max - style.FramePadding,
            label[..visibleEnd], style.ButtonTextAlign, textSize, boundingBox, true);
        return clicked;
    }

    public ref struct SplitButtonData
    {
        public required ReadOnlySpan<byte> Label;
        public          ReadOnlySpan<byte> Tooltip;
        public          uint               Background;
        public          uint               Active;
        public          uint               Hovered;
        public          uint               Border;
        public          uint               Text;

        public readonly uint GetColor(bool isHovered, bool isHeld)
            => isHovered ? isHeld ? Active : Hovered : Background;
    }

    private static (float Incline, bool IsHoveredLeft, bool IsHoveredRight) DiagonalCheck(bool isHovered, in ImRect boundingBox)
    {
        var lowerLeft  = new Vector2(boundingBox.Min.X, boundingBox.Max.Y);
        var upperRight = new Vector2(boundingBox.Max.X, boundingBox.Min.Y);
        var incline    = (upperRight.Y - lowerLeft.Y) / (upperRight.X - lowerLeft.X);
        if (!isHovered)
            return (incline, false, false);

        var mousePos = ImGui.GetMousePos();
        var x        = mousePos.X - lowerLeft.X;
        var y        = mousePos.Y - lowerLeft.Y;
        return x * incline >= y ? (incline, true, false) : (incline, false, true);
    }

    public static int SplitButton(ImGuiId id, in SplitButtonData left, in SplitButtonData right, Vector2 size, uint sharedBorder)
    {
        // Copied from ImGui proper.
        var window = ImGuiInternal.GetCurrentWindow();
        if (window.SkipItems)
            return 0;

        var     screenPos = window.DC.CursorPos;
        Vector2 rightStart;
        var     style = ImGui.GetStyle();
        if (size.Y is 0)
            size.Y = ImGui.GetFrameHeight();
        if (size.X is 0)
        {
            var sizeLeft  = ImUtf8.CalcTextSize(left.Label).X;
            var sizeRight = ImUtf8.CalcTextSize(right.Label).X;
            size.X = Math.Max(sizeLeft, sizeRight) + size.Y + 2 * style.FramePadding.X;
            rightStart = new Vector2(screenPos.X + size.X - sizeRight - style.FramePadding.X,
                screenPos.Y + size.Y - 1 - ImGui.GetTextLineHeight());
        }
        else
        {
            var sizeRight = ImUtf8.CalcTextSize(right.Label).X;
            rightStart = new Vector2(screenPos.X + size.X - sizeRight - style.FramePadding.X,
                screenPos.Y + size.Y - 1 - ImGui.GetTextLineHeight());
        }

        var boundingBox = new ImRect(screenPos, screenPos + size);
        ImGuiInternal.ItemSize(boundingBox, style.FramePadding.Y);
        if (!ImGuiInternal.ItemAdd(boundingBox, id))
            return 0;

        var ret = ImGuiInternal.ButtonBehavior(boundingBox, id, out var hovered, out var held);
        var (incline, isHoveredLeft, isHoveredRight) = DiagonalCheck(hovered, boundingBox);
        var colorLeft  = left.GetColor(isHoveredLeft, held);
        var colorRight = right.GetColor(isHoveredRight, held);
        ImGuiInternal.RenderNavHighlight(boundingBox, id, 0);

        var drawList = ImGui.GetWindowDrawList();
        if (style.FrameRounding is 0)
        {
            var upperRight = new Vector2(boundingBox.Max.X, boundingBox.Min.Y);
            var lowerLeft  = new Vector2(boundingBox.Min.X, boundingBox.Max.Y);
            drawList.PathLineTo(boundingBox.Min);
            drawList.PathLineTo(lowerLeft);
            drawList.PathLineTo(upperRight);
            drawList.PathLineTo(boundingBox.Min);
            drawList.PathFillConvex(colorLeft);
            if (left.Border is not 0)
            {
                drawList.PathLineTo(boundingBox.Min);
                drawList.PathLineTo(lowerLeft);
                drawList.PathLineTo(upperRight);
                drawList.PathStroke(left.Border, ImDrawFlags.None, Math.Max(style.FrameBorderSize, 1));
            }

            drawList.PathLineTo(upperRight);
            drawList.PathLineTo(boundingBox.Max);
            drawList.PathLineTo(lowerLeft);
            drawList.PathLineTo(upperRight);
            drawList.PathFillConvex(colorRight);
            if (right.Border is not 0)
            {
                drawList.PathLineTo(upperRight);
                drawList.PathLineTo(boundingBox.Max);
                drawList.PathLineTo(lowerLeft);
                drawList.PathStroke(right.Border, ImDrawFlags.None, Math.Max(style.FrameBorderSize, 1));
            }
        }
        else
        {
            var rounding         = style.FrameRounding;
            var upperLeftCenter  = new Vector2(boundingBox.Min.X + rounding, boundingBox.Min.Y + rounding);
            var lowerLeftCenter  = new Vector2(upperLeftCenter.X,            boundingBox.Max.Y - rounding);
            var upperRightCenter = new Vector2(boundingBox.Max.X - rounding, upperLeftCenter.Y);
            var lowerRightCenter = new Vector2(upperRightCenter.X,           lowerLeftCenter.Y);
            drawList.PathArcTo(lowerLeftCenter, rounding, 135f / 180f * MathF.PI, 180 / 180f * MathF.PI);
            drawList.PathArcToFast(upperLeftCenter, rounding, 6, 9);
            drawList.PathArcTo(upperRightCenter, rounding, 270 / 180f * MathF.PI, 315 / 180f * MathF.PI);
            drawList.PathFillConvex(colorLeft);
            if (left.Border is not 0)
            {
                drawList.PathArcTo(lowerLeftCenter, rounding, 135f / 180f * MathF.PI, 180 / 180f * MathF.PI);
                drawList.PathArcToFast(upperLeftCenter, rounding, 6, 9);
                drawList.PathArcTo(upperRightCenter, rounding, 270 / 180f * MathF.PI, 315 / 180f * MathF.PI);
                drawList.PathStroke(left.Border, ImDrawFlags.None, Math.Max(style.FrameBorderSize, 1));
            }

            drawList.PathArcTo(upperRightCenter, rounding, 315 / 180f * MathF.PI, 360 / 180f * MathF.PI);
            drawList.PathArcToFast(lowerRightCenter, rounding, 0, 3);
            drawList.PathArcTo(lowerLeftCenter, rounding, 90 / 180f * MathF.PI, 135 / 180f * MathF.PI);
            drawList.PathFillConvex(colorRight);

            if (right.Border is not 0)
            {
                drawList.PathArcTo(upperRightCenter, rounding, 315 / 180f * MathF.PI, 360 / 180f * MathF.PI);
                drawList.PathArcToFast(lowerRightCenter, rounding, 0, 3);
                drawList.PathArcTo(lowerLeftCenter, rounding, 90 / 180f * MathF.PI, 135 / 180f * MathF.PI);
                drawList.PathStroke(right.Border, ImDrawFlags.None, Math.Max(style.FrameBorderSize, 1));
            }

            if (sharedBorder is not 0)
            {
                const float sqrt2  = 1.4142135623731f;
                var         offset = rounding / sqrt2;
                drawList.PathLineTo(lowerLeftCenter + new Vector2(-offset, offset));
                drawList.PathLineTo(upperRightCenter + new Vector2(offset, -offset));
                drawList.PathStroke(sharedBorder, ImDrawFlags.None, Math.Max(style.FrameBorderSize, 2));
            }
        }

        var textColorLeft  = left.Text is 0 ? ImGui.GetColorU32(ImGuiCol.Text) : left.Text;
        var textColorRight = right.Text is 0 ? ImGui.GetColorU32(ImGuiCol.Text) : right.Text;
        drawList.AddText(left.Label,  boundingBox.Min + new Vector2(style.FramePadding.X, 0), textColorLeft);
        drawList.AddText(right.Label, rightStart,                                             textColorRight);

        if (isHoveredLeft && !left.Tooltip.IsEmpty && left.Tooltip[0] is not 0)
        {
            using var tt = ImUtf8.Tooltip();
            ImUtf8.Text(left.Tooltip);
        }

        if (isHoveredRight && !right.Tooltip.IsEmpty && right.Tooltip[0] is not 0)
        {
            using var tt = ImUtf8.Tooltip();
            ImUtf8.Text(right.Tooltip);
        }

        if (ret)
            return isHoveredLeft ? 1 : isHoveredRight ? 2 : 0;

        return 0;
    }
}
