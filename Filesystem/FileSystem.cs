using System;
using System.Collections.Generic;

namespace OtterGui.Filesystem;

public enum FileSystemChangeType
{
    ObjectRemoved,
    FolderAdded,
    LeafAdded,
    ObjectMoved,
    FolderMerged,
    PartialMerge,
}

public partial class FileSystem<T> where T : class
{
    public delegate void         ChangeDelegate(FileSystemChangeType type, IPath changedObject, IPath? previousParent, IPath? newParent);
    public event ChangeDelegate? Changed;

    private readonly NameComparer _nameComparer;
    public           Folder       Root = Folder.CreateRoot();

    public FileSystem(IComparer<string>? comparer = null)
        => _nameComparer = new NameComparer(comparer ?? StringComparer.InvariantCultureIgnoreCase);


    // Find a folder using the given comparer.
    private int Search(Folder parent, string name)
        => parent._children.BinarySearch((SearchPath)name, _nameComparer);

    private enum Result
    {
        Success,
        SuccessNothingDone,
        InvalidOperation,
        ItemExists,
        PartialSuccess,
    }

    // Find a specific child by its path from Root.
    // Returns true if the folder was found, and false if not.
    // The out parameter will contain the furthest existing folder.
    public bool Find(string fullPath, out IPath child)
    {
        var split  = fullPath.Split();
        var folder = Root;
        child = Root;
        foreach (var part in split)
        {
            var idx = Search(folder, part);
            if (idx < 0)
            {
                child = folder;
                return false;
            }

            child = folder._children[idx];
            if (child is not Folder f)
                return part == split[^1];

            folder = f;
        }

        return true;
    }


    private (Result, Folder) CreateAllFolders(IEnumerable<string> names)
    {
        var last   = Root;
        var result = Result.SuccessNothingDone;
        foreach (var name in names)
        {
            var folder = new Folder(last, name);
            result = SetChild(last, folder, out var idx);
            if (result == Result.ItemExists)
            {
                if (last._children[idx] is not Folder)
                    return (Result.ItemExists, last);

                result = Result.SuccessNothingDone;
            }

            last = folder;
        }

        return (result, last);
    }


    public (Leaf, int) CreateLeaf(Folder parent, string name, T data)
    {
        var leaf = new Leaf(parent, name, data);
        if (SetChild(parent, leaf, out var idx) == Result.ItemExists)
            throw new Exception($"Could not add leaf {leaf.Name} to {parent.FullName()}: Child of that name already exists.");

        Changed?.Invoke(FileSystemChangeType.LeafAdded, leaf, null, parent);
        return (leaf, idx);
    }

    public (Folder, int) CreateFolder(Folder parent, string name)
    {
        var folder = new Folder(parent, name);
        if (SetChild(parent, folder, out var idx) == Result.ItemExists)
            throw new Exception($"Could not add folder {folder.Name} to {parent.FullName()}: Child of that name already exists.");

        Changed?.Invoke(FileSystemChangeType.LeafAdded, folder, null, parent);
        return (folder, idx);
    }

    public (Folder, int) FindOrCreateFolder(Folder parent, string name)
    {
        var folder = new Folder(parent, name);
        if (SetChild(parent, folder, out var idx) == Result.ItemExists)
        {
            if (parent._children[idx] is Folder f)
                return (f, idx);

            throw new Exception($"The child {name} already exists in {parent.FullName()} but is not a folder.");
        }

        Changed?.Invoke(FileSystemChangeType.FolderAdded, folder, null, parent);
        return (folder, idx);
    }

    public void Delete(IPath child)
    {
        switch (RemoveChild(child))
        {
            case Result.InvalidOperation: throw new Exception("Can not delete root directory.");
            case Result.Success:
                Changed?.Invoke(FileSystemChangeType.ObjectRemoved, child, child.Parent, null);
                return;
        }
    }

    public void Move(IPath child, Folder newParent)
    {
        switch (MoveChild(child, newParent, out var oldParent, out var newIdx))
        {
            case Result.Success:
                Changed?.Invoke(FileSystemChangeType.ObjectMoved, child, oldParent, newParent);
                break;
            case Result.SuccessNothingDone: return;
            case Result.InvalidOperation:   throw new Exception("Can not move root directory.");
            case Result.ItemExists:
                if (child is Folder childFolder && newParent._children[newIdx] is Folder preFolder)
                {
                    Merge(childFolder, preFolder);
                    return;
                }
                else
                {
                    throw new Exception(
                        $"Can not move {child.Name} into {newParent.FullName()} because {newParent._children[newIdx].FullName()} already exists.");
                }
        }
    }


    public void Merge(Folder from, Folder to)
    {
        switch (MergeFolders(from, to))
        {
            case Result.SuccessNothingDone: return;
            case Result.InvalidOperation:   throw new Exception($"Can not merge root directory into {to.FullName()}.");
            case Result.Success:
                Changed?.Invoke(FileSystemChangeType.FolderMerged, from, from, to);
                return;
            case Result.PartialSuccess:
                Changed?.Invoke(FileSystemChangeType.PartialMerge, from, from, to);
                return;
        }
    }
}
