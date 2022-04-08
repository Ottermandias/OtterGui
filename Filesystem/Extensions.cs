using System;
using System.Collections.Generic;
using System.Text;

namespace OtterGui.Filesystem;

public static class Extensions
{
    // A filesystem name may not contain forward-slashes, as they are used to split paths.
    // The empty string as name signifies the root, so it can also not be used.
    public static string FixName(this string name)
    {
        var fix = name.Replace('/', '\\').Trim();
        return fix.Length == 0 ? "<None>" : fix;
    }

    // Split a path string into directories.
    // Empty entries will be skipped.
    public static string[] SplitDirectories(this string path)
        => path.Split('/', StringSplitOptions.RemoveEmptyEntries);

    // Move an item in a list from index 1 to index 2.
    // The indices are clamped to the valid range.
    // Other list entries are shifted accordingly.
    public static bool Move<T>(this IList<T> list, int idx1, int idx2)
    {
        idx1 = Math.Clamp(idx1, 0, list.Count - 1);
        idx2 = Math.Clamp(idx2, 0, list.Count - 1);
        if (idx1 == idx2)
            return false;

        var tmp = list[idx1];
        // move element down and shift other elements up
        if (idx1 < idx2)
            for (var i = idx1; i < idx2; i++)
                list[i] = list[i + 1];
        // move element up and shift other elements down
        else
            for (var i = idx1; i > idx2; i--)
                list[i] = list[i - 1];

        list[idx2] = tmp;
        return true;
    }
}
