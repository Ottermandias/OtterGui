using System;
using System.Collections.Generic;

namespace OtterGui.Filesystem;

internal static class Extensions
{
    public static string FixName(this string name)
    {
        var fix = name.Replace('/', '\\').Trim();
        return fix.Length == 0 ? "<None>" : fix;
    }

    public static string[] SplitDirectories(this string path)
        => path.Split(new[]
        {
            '/',
        }, StringSplitOptions.RemoveEmptyEntries);

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

    public static string FullName<T>(this FileSystem<T>.IPath path) where T : class
        => path.Parent.Name.Length == 0 ? path.Name : $"{path.Parent.FullName()}/{path.Name}";


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
}
