using System;
using System.Collections.Generic;
using ImGuiNET;
using OtterGui.Filesystem;
using OtterGui.Raii;

namespace OtterGui;

public partial class FileSystemSelector<T, TStateStorage>
{
    public void SubscribeRightClickFolder(Action<FileSystem<T>.Folder> action, int priority = 0)
        => RemovePrioritizedDelegate(_rightClickOptionsFolder, action, priority);

    public void SubscribeRightClickLeaf(Action<FileSystem<T>.Leaf> action, int priority = 0)
        => RemovePrioritizedDelegate(_rightClickOptionsLeaf, action, priority);

    public void UnsubscribeRightClickFolder(Action<FileSystem<T>.Folder> action)
        => AddPrioritizedDelegate(_rightClickOptionsFolder, action);

    public void UnsubscribeRightClickLeaf(Action<FileSystem<T>.Leaf> action)
        => AddPrioritizedDelegate(_rightClickOptionsLeaf, action);

    private void RightClickContext(FileSystem<T>.Folder folder)
    {
        using var _ = ImRaii.Popup(folder.Name);
        if (!_)
            return;

        foreach (var action in _rightClickOptionsFolder)
            action.Item1.Invoke(folder);
    }

    private void RightClickContext(FileSystem<T>.Leaf leaf)
    {
        using var _ = ImRaii.Popup(leaf.Name);
        if (!_)
            return;

        foreach (var action in _rightClickOptionsLeaf)
            action.Item1.Invoke(leaf);
    }


    private readonly List<(Action<FileSystem<T>.Folder>, int)> _rightClickOptionsFolder = new(4);
    private readonly List<(Action<FileSystem<T>.Leaf>, int)>   _rightClickOptionsLeaf   = new();

    private void InitDefaultContext()
    {
        SubscribeRightClickFolder(DissolveFolder);
        SubscribeRightClickFolder(ExpandAllDescendants,   100);
        SubscribeRightClickFolder(CollapseAllDescendants, 100);
        SubscribeRightClickFolder(RenameFolder,           1000);
        SubscribeRightClickLeaf(RenameLeaf, 1000);
    }

    private void DissolveFolder(FileSystem<T>.Folder folder)
    {
        if (ImGui.MenuItem("Dissolve Folder"))
            _fsActions.Enqueue(() => FileSystem.Merge(folder, folder.Parent));
    }

    private void ExpandAllDescendants(FileSystem<T>.Folder folder)
    {
        if (ImGui.MenuItem("Expand All Descendants"))
            _fsActions.Enqueue(() => ToggleDescendants(folder, 1));
    }

    private void CollapseAllDescendants(FileSystem<T>.Folder folder)
    {
        if (ImGui.MenuItem("Collapse All Descendants"))
            _fsActions.Enqueue(() => ToggleDescendants(folder, 0));
    }

    private void RenameFolder(FileSystem<T>.Folder folder)
    {
        var currentPath = folder.FullName();
        if (ImGui.InputText("##Rename", ref currentPath, 256, ImGuiInputTextFlags.EnterReturnsTrue))
            _fsActions.Enqueue(() =>
            {
                var stateStorage = ImGui.GetStateStorage();
                var state        = stateStorage.GetInt(ImGui.GetID(folder.Label()), 0);
                FileSystem.RenameAndMove(folder, currentPath);
                CopyStateStorage(stateStorage, state, folder);
            });
    }

    private void RenameLeaf(FileSystem<T>.Leaf leaf)
    {
        var currentPath = leaf.FullName();
        if (ImGui.InputText("##Rename", ref currentPath, 256, ImGuiInputTextFlags.EnterReturnsTrue))
            _fsActions.Enqueue(() =>
            {
                var stateStorage = ImGui.GetStateStorage();
                FileSystem.RenameAndMove(leaf, currentPath);
                CopyStateStorage(stateStorage, 1, leaf.Parent);
            });
    }
}
