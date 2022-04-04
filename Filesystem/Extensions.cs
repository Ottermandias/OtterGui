using System;
using System.Collections.Generic;
using System.Text;

namespace OtterGui.Filesystem;

internal static class Extensions
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

    // Obtain the full path of a filesystem path.
    public static string FullName<T>(this FileSystem<T>.IPath path) where T : class
    {
        var sb = new StringBuilder(path.Name.Length * 5);
        Concat(path, sb, '/');
        return sb.ToString();
    }

    // Obtain the full label of a filesystem path, which is just the whole path without separators.
    public static string Label<T>(this FileSystem<T>.IPath path) where T : class
    {
        var sb = new StringBuilder(path.Name.Length * 5);
        Concat(path, sb);
        return sb.ToString();
    }


    // Obtain all labels in the given path in ascending order.
    public static IEnumerable<string> AllLabels<T>(this FileSystem<T>.IPath path) where T : class
    {
        if (path.Name.Length == 0)
            yield break;

        var lastLabel = string.Empty;
        foreach (var label in AllLabels(path.Parent))
        {
            lastLabel = label;
            yield return lastLabel;
        }

        yield return lastLabel + path.Name;
    }

    // Obtain the number of parents a given path has.
    public static int Depth<T>(this FileSystem<T>.IPath path) where T : class
    {
        var count = 0;
        while (path.Name.Length > 0)
        {
            path = path.Parent;
            ++count;
        }

        return count;
    }


    // === Internals ===

    // newName needs to be fixed already.
    internal static void Rename<T>(this FileSystem<T>.IPath path, string newName) where T : class
    {
        switch (path)
        {
            case FileSystem<T>.Folder f:
                f.Name = newName;
                return;
            case FileSystem<T>.Leaf l:
                l.Name = newName;
                return;
        }
    }

    // Concatenate paths with no separators.
    internal static void Concat<T>(FileSystem<T>.IPath path, StringBuilder sb) where T : class
    {
        if (path.Name.Length == 0)
            return;

        Concat(path.Parent, sb);
        sb.Append(path.Name);
    }

    // Concatenate paths with a given separator.
    internal static bool Concat<T>(FileSystem<T>.IPath path, StringBuilder sb, char separator) where T : class
    {
        if (path.Name.Length == 0)
            return false;

        if (Concat(path.Parent, sb, separator))
            sb.Append(separator);
        sb.Append(path.Name);
        return true;
    }
}
