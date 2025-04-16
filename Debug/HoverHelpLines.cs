using ImGuiNET;

namespace OtterGui.Debug;

public static partial class ImDebug
{
    /// <summary> Draw some checker-boarded help lines across the whole window along the dimensions of the last item when hovering it. </summary>
    /// <param name="color1"> The color of odd pixels. </param>
    /// <param name="color2"> The color of even pixels. </param>
    /// <param name="always"> Draw the lines even when not hovering the item. </param>
    public static void HoverHelpLines(uint color1 = 0xFFFFFFFF, uint color2 = 0xFF000000, bool always = false)
    {
        if (!always && !ImGui.IsItemHovered())
            return;

        var min        = ImGui.GetItemRectMin();
        var max        = ImGui.GetItemRectMax();
        var drawList   = ImGui.GetForegroundDrawList();
        var lineXStart = (int)MathF.Round(ImGui.GetWindowPos().X);
        var lineXEnd   = (int)MathF.Round(lineXStart + ImGui.GetWindowSize().X) + 1;

        var lineYStart = (int)MathF.Round(ImGui.GetWindowPos().Y);
        var lineYEnd   = (int)MathF.Round(lineYStart + ImGui.GetWindowSize().Y) + 1;

        var minX = (int)MathF.Round(min.X);
        var minY = (int)MathF.Round(min.Y);
        var maxX = (int)MathF.Round(max.X);
        var maxY = (int)MathF.Round(max.Y);

        drawList.AddLine(new Vector2(lineXStart, minY),       new Vector2(lineXEnd, minY),     color1);
        drawList.AddLine(new Vector2(lineXStart, maxY),       new Vector2(lineXEnd, maxY),     color1);
        drawList.AddLine(new Vector2(minX,       lineYStart), new Vector2(minX,     lineYEnd), color1);
        drawList.AddLine(new Vector2(maxX,       lineYStart), new Vector2(maxX,     lineYEnd), color1);

        for (var x = (lineXStart + minY) % 2 is 0 ? lineXStart + 1 : lineXStart; x < lineXEnd; x += 2)
            drawList.AddLine(new Vector2(x, minY), new Vector2(x + 1, minY), color2);

        for (var x = (lineXStart + maxY) % 2 is 0 ? lineXStart + 1 : lineXStart; x < lineXEnd; x += 2)
            drawList.AddLine(new Vector2(x, maxY), new Vector2(x + 1, maxY), color2);

        for (var y = (lineYStart + minX) % 2 is 0 ? lineYStart + 1 : lineYStart; y < lineYEnd; y += 2)
            drawList.AddLine(new Vector2(minX, y), new Vector2(minX, y + 1), color2);

        for (var y = (lineYStart + maxX) % 2 is 0 ? lineYStart + 1 : lineYStart; y < lineYEnd; y += 2)
            drawList.AddLine(new Vector2(maxX, y), new Vector2(maxX, y + 1), color2);
    }
}
