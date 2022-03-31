using System.Linq;

namespace OtterGui.Filesystem;

public partial class FileSystem<T>
{
    private Result RenameChild(IPath child, string newName)
    {
        if (child.Name.Length == 0)
            return Result.InvalidOperation;

        newName = newName.FixName();
        if (newName == child.Name)
            return Result.SuccessNothingDone;

        var newIdx = Search(child.Parent, newName);
        if (newIdx >= 0)
            return Result.ItemExists;

        var currentIdx = Search(child.Parent, child.Name);
        child.Parent._children.Move(currentIdx, ~newIdx);
        child.Rename(newName);
        return Result.Success;
    }

    private Result MoveChild(IPath child, Folder newParent, out Folder oldParent, out int newIdx)
    {
        oldParent = child.Parent;
        newIdx    = 0;
        if (child.Name.Length == 0)
            return Result.InvalidOperation;

        if (newParent == oldParent)
            return Result.SuccessNothingDone;

        newIdx = Search(newParent, child.Name);
        if (newIdx >= 0)
            return Result.ItemExists;


        RemoveChild(oldParent, child, Search(oldParent, child.Name));
        SetChild(newParent, child, newIdx);
        return Result.Success;
    }

    private (Result, Folder, string) CreateAllFolders(string path)
    {
        if (path.Length == 0)
            return (Result.SuccessNothingDone, Root, string.Empty);

        var split = path.Split();
        if (split.Length == 1)
            return (Result.SuccessNothingDone, Root, string.Empty);

        var (result, folder) = CreateAllFolders(path.Split().SkipLast(1));
        return (result, folder, split[^1]);
    }

    private static void RemoveChild(Folder parent, IPath child, int idx)
    {
        parent._children.RemoveAt(idx);
        switch (child)
        {
            case Folder f:
                parent.TotalChildren -= f.TotalChildren + 1;
                parent.TotalLeaves   -= f.TotalLeaves;
                break;
            case Leaf:
                --parent.TotalChildren;
                --parent.TotalLeaves;
                break;
        }
    }

    private static void SetChild(Folder parent, IPath child, int idx)
    {
        parent._children.Insert(idx, child);
        switch (child)
        {
            case Folder f:
                f.Parent             =  parent;
                parent.TotalChildren += f.TotalChildren + 1;
                parent.TotalLeaves   += f.TotalLeaves;
                break;
            case Leaf l:
                l.Parent = parent;
                ++parent.TotalChildren;
                ++parent.TotalLeaves;
                break;
        }
    }

    private Result SetChild(Folder parent, IPath child, out int idx)
    {
        idx = Search(parent, child.Name);
        if (idx >= 0)
            return Result.ItemExists;

        idx = ~idx;
        SetChild(parent, child, idx);
        return Result.Success;
    }

    private Result RemoveChild(IPath child)
    {
        if (child.Name.Length == 0)
            return Result.InvalidOperation;

        var idx = Search(child.Parent, child.Name);
        if (idx < 0)
            return Result.SuccessNothingDone;

        RemoveChild(child.Parent, child, idx);
        return Result.Success;
    }

    private Result MergeFolders(Folder from, Folder to)
    {
        if (from == to)
            return Result.SuccessNothingDone;
        if (from.Name.Length == 0)
            return Result.InvalidOperation;

        var result = Result.Success;
        foreach (var child in from._children)
            result = MoveChild(child, to, out _, out _) == Result.Success ? result : Result.PartialSuccess;

        return result == Result.Success ? RemoveChild(from) : result;
    }
}
