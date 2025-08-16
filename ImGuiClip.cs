using Dalamud.Bindings.ImGui;
using OtterGui.Raii;
using OtterGui.Text;

namespace OtterGui;

public static class ImGuiClip
{
    // Get the number of skipped items of a given height necessary for the current scroll bar,
    // and apply the dummy of the appropriate height, removing one item spacing.
    // The height has to contain the spacing.
    public static int GetNecessarySkips(float height)
    {
        var curY  = ImGui.GetScrollY();
        var skips = (int)(curY / height);
        if (skips > 0)
            ImGui.Dummy(new Vector2(1, skips * height - ImGui.GetStyle().ItemSpacing.Y));

        return skips;
    }

    // Get the number of skipped items of a given height necessary for the current scroll bar,
    // but subtracting the current cursor position,
    // and apply the dummy of the appropriate height, removing one item spacing.
    // The height has to contain the spacing.
    public static int GetNecessarySkipsAtPos(float height, float cursorPosY, int maxCount = int.MaxValue)
    {
        var curY  = ImGui.GetScrollY() - cursorPosY;
        var skips = (int)(curY / height);
        if (skips > maxCount)
            skips = maxCount;
        if (skips > 0)
        {
            ImGui.Dummy(new Vector2(1, skips * height - ImGui.GetStyle().ItemSpacing.Y));
            return skips;
        }

        return 0;
    }


    // Draw the dummy for the remaining items computed by ClippedDraw,
    // removing one item spacing.
    public static void DrawEndDummy(int remainder, float height)
    {
        if (remainder > 0)
            ImGui.Dummy(new Vector2(1, remainder * height - ImGui.GetStyle().ItemSpacing.Y));
    }

    // Draw a clipped random-access collection of consistent height lineHeight.
    // Uses ImGuiListClipper and thus handles start- and end-dummies itself.
    public static void ClippedDraw<T>(IReadOnlyList<T> data, Action<T> draw, float lineHeight)
    {
        using var clipper = ImUtf8.ListClipper(data.Count, lineHeight);
        while (clipper.Step())
        {
            for (var actualRow = clipper.DisplayStart; actualRow < clipper.DisplayEnd; actualRow++)
            {
                if (actualRow >= data.Count)
                    return;

                if (actualRow < 0)
                    continue;

                draw(data[actualRow]);
            }
        }
    }

    // Draw a clipped random-access collection of consistent height lineHeight.
    // Uses ImGuiListClipper and thus handles start- and end-dummies itself, but acts on type and index.
    public static void ClippedDraw<T>(IReadOnlyList<T> data, Action<T, int> draw, float lineHeight)
    {
        using var clipper = ImUtf8.ListClipper(data.Count, lineHeight);
        while (clipper.Step())
        {
            for (var actualRow = clipper.DisplayStart; actualRow < clipper.DisplayEnd; actualRow++)
            {
                if (actualRow >= data.Count)
                    return;

                if (actualRow < 0)
                    continue;

                draw(data[actualRow], actualRow);
            }
        }
    }

    // Draw non-random-access data without storing state.
    // Use GetNecessarySkips first and use its return value for skips.
    // startIndex can be set if using multiple separate chunks of data with different filter or draw functions (of the same height).
    // Returns either the non-negative remaining objects in data that could not be drawn due to being out of the visible area,
    // if count was given this will be subtracted instead of counted,
    // or the bitwise-inverse of the next startIndex for subsequent collections, if there is still room for more visible objects.
    public static int ClippedDraw<T>(IEnumerable<T> data, int skips, Action<T> draw, int? count = null, int startIndex = 0)
    {
        if (count != null && count.Value + startIndex <= skips)
            return ~(count.Value + startIndex);

        using var it      = data.GetEnumerator();
        var       visible = false;
        var       idx     = startIndex;
        while (it.MoveNext())
        {
            if (idx >= skips)
            {
                using (var group = ImRaii.Group())
                {
                    using var id = ImRaii.PushId(idx);
                    draw(it.Current);
                }

                // Just checking IsItemVisible caused some issues when not the entire width of the window was visible.
                if (!ImGui.IsRectVisible(ImGui.GetItemRectMin(), ImGui.GetItemRectMin() with { Y = ImGui.GetItemRectMax().Y }))
                {
                    if (visible)
                    {
                        if (count != null)
                            return Math.Max(0, count.Value - idx + startIndex - 1);

                        var remainder = 0;
                        while (it.MoveNext())
                            ++remainder;

                        return remainder;
                    }
                }
                else
                {
                    visible = true;
                }
            }

            ++idx;
        }

        return ~idx;
    }

    // Draw non-random-access data without storing state.
    // Use GetNecessarySkips first and use its return value for skips.
    // startIndex can be set if using multiple separate chunks of data with different filter or draw functions (of the same height).
    // Returns either the non-negative remaining objects in data that could not be drawn due to being out of the visible area,
    // if count was given this will be subtracted instead of counted,
    // or the bitwise-inverse of the next startIndex for subsequent collections, if there is still room for more visible objects.
    public static int ClippedTableDraw<T>(IEnumerable<T> data, int skips, Action<T> draw, int? count = null, int startIndex = 0)
    {
        if (count != null && count.Value + startIndex <= skips)
            return ~(count.Value + startIndex);

        using var it      = data.GetEnumerator();
        var       visible = false;
        var       idx     = startIndex;
        while (it.MoveNext())
        {
            if (idx >= skips)
            {
                using (ImRaii.PushId(idx))
                {
                    using var _ = ImUtf8.Group();
                    draw(it.Current);
                }

                // Just checking IsItemVisible caused some issues when not the entire width of the window was visible.
                if (!ImGui.IsRectVisible(ImGui.GetItemRectMin(), ImGui.GetItemRectMin() with { Y = ImGui.GetItemRectMax().Y }))
                {
                    if (visible)
                    {
                        if (count != null)
                            return Math.Max(0, count.Value - idx + startIndex - 1);

                        var remainder = 0;
                        while (it.MoveNext())
                            ++remainder;

                        return remainder;
                    }
                }
                else
                {
                    visible = true;
                }
            }

            ++idx;
        }

        return ~idx;
    }


    // Draw non-random-access data that gets filtered without storing state.
    // Use GetNecessarySkips first and use its return value for skips.
    // checkFilter should return true for items that should be displayed and false for those that should be skipped.
    // startIndex can be set if using multiple separate chunks of data with different filter or draw functions (of the same height).
    // Returns either the non-negative remaining objects in data that could not be drawn due to being out of the visible area,
    // or the bitwise-inverse of the next startIndex for subsequent collections, if there is still room for more visible objects.
    public static int FilteredClippedDraw<T>(IEnumerable<T> data, int skips, Func<T, bool> checkFilter, Action<T> draw, int startIndex = 0)
        => ClippedDraw(data.Where(checkFilter), skips, draw, null, startIndex);
}
