using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using OtterGui.Extensions;
using OtterGui.Text;
using OtterGui.Text.EndObjects;
using OtterGuiInternal;
using OtterGuiInternal.Enums;
using OtterGuiInternal.Structs;

namespace OtterGui.Widgets;

public static unsafe class ResizableChild
{
    public static Child Begin(ReadOnlySpan<byte> label, Vector2 size, Action<Vector2> setSize, Vector2 minSize, Vector2 maxSize,
        ImGuiWindowFlags flags = default,
        bool resizeX = true, bool resizeY = false)
    {
        // Work in the child ID.
        using var idStack = ImUtf8.PushId(label);

        // Use two IDs to store state and current resizing value if any.
        var stateId = ImUtf8.GetId("####state"u8);
        var valueId = ImUtf8.GetId("####value"u8);
        var state   = ImGui.GetStateStorage().GetIntRef(stateId, 0);
        var value   = ImGui.GetStateStorage().GetFloatRef(valueId, 0f);
        size = *state switch
        {
            1 => size with { X = *value },
            2 => size with { Y = *value },
            _ => size,
        };

        // Fix border width, use regular color and rounding style.
        const float borderWidth     = 1f;
        const float halfBorderWidth = borderWidth / 2f;
        var         borderColor     = ImGui.GetColorU32(ImGuiCol.Border);
        var         rounding        = ImGui.GetStyle().ChildRounding;
        var         onlyInner       = rounding is 0 ? borderWidth : rounding;
        var         hoverExtend     = 5f * ImUtf8.GlobalScale;
        const float delay           = 0.5f;

        var rectMin  = ImGui.GetCursorScreenPos() + new Vector2(halfBorderWidth);
        var rectMax  = (ImGui.GetCursorScreenPos() + size).Round();
        var drawList = ImGui.GetWindowDrawList();

        // If resizing in X direction is allowed, handle it.
        if (resizeX)
        {
            var id = (ImGuiId)ImUtf8.GetId("####x"u8);
            // Behaves as a splitter, so second size is the remainder.
            var sizeInc      = size.X;
            var sizeDec      = ImGui.GetContentRegionAvail().X - size.X;
            var remainderMin = ImGui.GetContentRegionAvail().X - maxSize.X;

            using var color = ImRaii.PushColor(ImGuiCol.Separator, borderColor);
            var rect = new ImRect(new Vector2(rectMax.X - halfBorderWidth, rectMin.Y + onlyInner),
                new Vector2(rectMax.X + halfBorderWidth,                   rectMax.Y - onlyInner));
            if (ImGuiNativeInterop.SplitterBehavior(rect, id, ImGuiAxis.X, &sizeInc, &sizeDec, minSize.X, remainderMin, hoverExtend, delay, 0))
            {
                // Update internal state.
                *value  = sizeInc;
                size    = size with { X = sizeInc };
                rectMax = (ImGui.GetCursorScreenPos() + size).Round();
                *state  = 1;
            }

            if (ImGui.IsItemDeactivated())
            {
                // Handle updating on deactivation only.
                *state = 0;
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    size.X  = *value;
                    rectMax = (ImGui.GetCursorScreenPos() + size).Round();
                    setSize(size);
                }
            }
        }

        if (resizeY)
        {
            // Same as X just for the other direction. Y takes priority in length.
            var id = (ImGuiId)ImUtf8.GetId("####y"u8);

            var sizeInc      = size.Y;
            var sizeDec      = ImGui.GetContentRegionAvail().Y - size.Y;
            var remainderMin = ImGui.GetContentRegionAvail().Y - maxSize.Y;

            using var color = ImRaii.PushColor(ImGuiCol.Separator, borderColor);
            var rect = new ImRect(new Vector2(rectMin.X + onlyInner, rectMax.Y - halfBorderWidth),
                new Vector2(rectMax.X - onlyInner,                   rectMax.Y + halfBorderWidth));
            if (ImGuiNativeInterop.SplitterBehavior(rect, id, ImGuiAxis.Y, &sizeInc, &sizeDec, minSize.X, remainderMin, hoverExtend, delay, 0))
            {
                *value  = sizeInc;
                size    = size with { Y = sizeInc };
                rectMax = (ImGui.GetCursorScreenPos() + size).Round();
                *state  = 2;
            }

            if (ImGui.IsItemDeactivated())
            {
                *state = 0;
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    size.Y  = *value;
                    rectMax = (ImGui.GetCursorScreenPos() + size).Round();
                    setSize(size);
                }
            }
        }

        if (rounding is 0)
        {
            // With no rounding, simply draw the lines not dealt with by the resizable lines.
            if (!resizeX)
                drawList.PathLineTo(new Vector2(rectMax.X, rectMax.Y));
            drawList.PathLineTo(new Vector2(rectMax.X, rectMin.Y));
            drawList.PathLineTo(rectMin);
            drawList.PathLineTo(new Vector2(rectMin.X, rectMax.Y));
            if (!resizeY)
                drawList.PathLineTo(rectMax);
            drawList.PathStroke(borderColor, ImDrawFlags.None, borderWidth);
            if (resizeX && resizeY)
                drawList.AddRectFilled(rectMax - new Vector2(halfBorderWidth), rectMax + new Vector2(halfBorderWidth), borderColor);
        }
        else
        {
            // Otherwise, draw all required arcs and lines.
            var centerTopRight    = new Vector2(rectMax.X - rounding, rectMin.Y + rounding);
            var centerTopLeft     = new Vector2(rectMin.X + rounding, centerTopRight.Y);
            var centerBottomRight = new Vector2(centerTopRight.X,     rectMax.Y - rounding);
            var centerBottomLeft  = new Vector2(centerTopLeft.X,      centerBottomRight.Y);
            if (!resizeX)
                drawList.PathArcToFast(centerBottomRight, rounding, 3, 0);
            drawList.PathArcToFast(centerTopRight,   rounding, 12, 9);
            drawList.PathArcToFast(centerTopLeft,    rounding, 9,  6);
            drawList.PathArcToFast(centerBottomLeft, rounding, 6,  3);
            if (resizeY)
            {
                drawList.PathStroke(borderColor, ImDrawFlags.None, borderWidth);
                if (resizeX)
                {
                    drawList.PathArcToFast(centerBottomRight, rounding, 3, 0);
                    drawList.PathStroke(borderColor, ImDrawFlags.None, borderWidth);
                }
            }
            else if (!resizeX)
            {
                drawList.PathStroke(borderColor, ImDrawFlags.Closed, borderWidth);
            }
            else
            {
                drawList.PathArcToFast(centerBottomRight, rounding, 3, 0);
                drawList.PathStroke(borderColor, ImDrawFlags.None, borderWidth);
            }
        }

        idStack.Pop();
        using var c     = ImRaii.PushColor(ImGuiCol.Border, 0);
        var       child = ImUtf8.Child(label, size, true, flags);
        return child;
    }
}
