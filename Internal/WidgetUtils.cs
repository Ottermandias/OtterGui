using ImGuiNET;

namespace OtterGui.Internal;

public static class WidgetUtils
{
    public static uint GetFrameBg(bool hovered, bool held)
        => ImGui.GetColorU32((hovered, held) switch
        {
            (true, true)  => ImGuiCol.FrameBgActive,
            (true, false) => ImGuiCol.FrameBgHovered,
            _             => ImGuiCol.FrameBg,
        });

    public static void RenderCross(ImDrawListPtr drawList, Vector2 position, uint color, float size)
    {
        var offset    = ((int)size) & 1;
        var thickness = Math.Max(size / 5, 1);
        var padding   = new Vector2(thickness / 3f);
        size     -= padding.X * 2 + offset;
        position += padding;
        var otherCorner = position + new Vector2(size);
        drawList.AddLine(position, otherCorner, color, thickness);
        position.X    += size;
        otherCorner.X -= size;
        drawList.AddLine(position, otherCorner, color, thickness);
    }

    public static void RenderCheckmark(ImDrawListPtr drawList, Vector2 position, uint color, float size)
    {
        var thickness = Math.Max(size / 5, 1);
        size -= thickness / 2;
        var padding = new Vector2(thickness / 4);
        position += padding;

        var third = size / 3;
        var bx    = position.X + third;
        var by    = position.Y + size - third / 2;
        drawList.PathLineTo(new Vector2(bx - third,        by - third));
        drawList.PathLineTo(new Vector2(bx,                by));
        drawList.PathLineTo(new Vector2(bx + third * 2.0f, by - third * 2.0f));
        drawList.PathStroke(color, 0, thickness);
    }

    public static void RenderDot(ImDrawListPtr drawList, Vector2 position, uint color, float size)
    {
        var padding = size / 7;
        var pos     = position + new Vector2(size / 2);
        size = size / 2 - padding;
        drawList.AddCircleFilled(pos, size, color);
    }

    public static void RenderDash(ImDrawListPtr drawList, Vector2 position, uint color, float size)
    {
        var offset    = ((int)size) & 1;
        var thickness = (int)Math.Max(size / 4, 1) | offset;
        var padding   = thickness / 2;
        position.X += padding;
        position.Y += size / 2;
        size       -= padding * 2;

        var otherCorner = position with { X = position.X + size };
        drawList.AddLine(position, otherCorner, color, thickness);
    }


    private const int MaxStackAlloc = 2047;

    public static unsafe void AddText(ImDrawListPtr drawList, Vector2 position, uint color, string text, bool checkSharp)
    {
        static int GetEnd(ReadOnlySpan<char> text, bool checkSharp)
        {
            if (!checkSharp)
            {
                var nullIdx = text.IndexOf('\0');
                return nullIdx < 0 ? text.Length : nullIdx;
            }

            var idx = 0;
            while (idx >= 0)
            {
                var newIdx = text[idx..].IndexOfAny("\0#");
                if (newIdx < 0)
                    break;

                idx += newIdx;
                if (text[idx] == '\0')
                    return idx;
                if (idx < text.Length - 1 && text[idx + 1] == '#')
                    return idx;
            }

            return text.Length;
        }

        var endIdx = GetEnd(text.AsSpan(), checkSharp);

        var bytes = endIdx * 2 > MaxStackAlloc ? new byte[endIdx * 2] : stackalloc byte[endIdx * 2];

        fixed (byte* start = bytes)
        {
            var numBytes = Encoding.UTF8.GetBytes(text.AsSpan()[..endIdx], bytes);
            ImGuiNative.ImDrawList_AddText_Vec2(drawList.NativePtr, position, color, start, start + numBytes);
        }
    }
}
