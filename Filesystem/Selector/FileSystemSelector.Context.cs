using System;
using System.Collections.Generic;
using ImGuiNET;
using OtterGui.Filesystem;
using OtterGui.Raii;

namespace OtterGui.FileSystem.Selector;

public partial class FileSystemSelector<T, TStateStorage>
{
    // Add a right-click context menu item to folder context menus at the given priority.
    // Context menu items are sorted from top to bottom on priority, then subscription order.
    public void SubscribeRightClickFolder(Action<FileSystem<T>.Folder> action, int priority = 0)
        => RemovePrioritizedDelegate(_rightClickOptionsFolder, action, priority);

    // Add a right-click context menu item to leaf context menus at the given priority.
    // Context menu items are sorted from top to bottom on priority, then subscription order.
    public void SubscribeRightClickLeaf(Action<FileSystem<T>.Leaf> action, int priority = 0)
        => RemovePrioritizedDelegate(_rightClickOptionsLeaf, action, priority);

    // Remove a right-click context menu item from the folder context menu by reference equality.
    public void UnsubscribeRightClickFolder(Action<FileSystem<T>.Folder> action)
        => AddPrioritizedDelegate(_rightClickOptionsFolder, action);

    // Remove a right-click context menu item from the leaf context menu by reference equality.
    public void UnsubscribeRightClickLeaf(Action<FileSystem<T>.Leaf> action)
        => AddPrioritizedDelegate(_rightClickOptionsLeaf, action);

    // Draw all context menu items for folders.
    private void RightClickContext(FileSystem<T>.Folder folder)
    {
        using var _ = ImRaii.Popup(folder.Name);
        if (!_)
            return;

        foreach (var action in _rightClickOptionsFolder)
            action.Item1.Invoke(folder);
    }

    // Draw all context menu items for leaves.
    private void RightClickContext(FileSystem<T>.Leaf leaf)
    {
        using var _ = ImRaii.Popup(leaf.Name);
        if (!_)
            return;

        foreach (var action in _rightClickOptionsLeaf)
            action.Item1.Invoke(leaf);
    }


    // Lists are sorted on priority, then subscription order.
    private readonly List<(Action<FileSystem<T>.Folder>, int)> _rightClickOptionsFolder = new(4);
    private readonly List<(Action<FileSystem<T>.Leaf>, int)>   _rightClickOptionsLeaf   = new(1);

    private void InitDefaultContext()
    {
        SubscribeRightClickFolder(DissolveFolder);
        SubscribeRightClickFolder(ExpandAllDescendants,   100);
        SubscribeRightClickFolder(CollapseAllDescendants, 100);
        SubscribeRightClickFolder(RenameFolder,           1000);
        SubscribeRightClickLeaf(RenameLeaf, 1000);
    }

    // Default entries for the folder context menu.
    // Protected so they can be removed by inheritors.
    protected void DissolveFolder(FileSystem<T>.Folder folder)
    {
        if (ImGui.MenuItem("Dissolve Folder"))
            _fsActions.Enqueue(() => FileSystem.Merge(folder, folder.Parent));
        ImGuiUtil.HoverTooltip("Remove this folder and move all its children to its parent-folder, if possible.");
    }

    protected void ExpandAllDescendants(FileSystem<T>.Folder folder)
    {
        if (ImGui.MenuItem("Expand All Descendants"))
        {
            var idx = _currentIndex;
            _fsActions.Enqueue(() => ToggleDescendants(folder, idx, 1));
        }

        ImGuiUtil.HoverTooltip("Successively expand all folders that descend from this folder, including itself.");
    }

    protected void CollapseAllDescendants(FileSystem<T>.Folder folder)
    {
        if (ImGui.MenuItem("Collapse All Descendants"))
        {
            var idx = _currentIndex;
            _fsActions.Enqueue(() => ToggleDescendants(folder, idx, 0));
        }

        ImGuiUtil.HoverTooltip("Successively collapse all folders that descend from this folder, including itself.");
    }

    protected void RenameFolder(FileSystem<T>.Folder folder)
    {
        var currentPath = folder.FullName();
        if (ImGui.InputText("##Rename", ref currentPath, 256, ImGuiInputTextFlags.EnterReturnsTrue))
            _fsActions.Enqueue(() =>
            {
                var oldLabel     = folder.Label();
                FileSystem.RenameAndMove(folder, currentPath);
                CopyStateStorage(folder, oldLabel);
            });

        ImGuiUtil.HoverTooltip("Enter a full path here to move or rename the folder. Creates all required parent directories, if possible.");
    }

    protected void RenameLeaf(FileSystem<T>.Leaf leaf)
    {
        var currentPath = leaf.FullName();
        if (ImGui.InputText("##Rename", ref currentPath, 256, ImGuiInputTextFlags.EnterReturnsTrue))
            _fsActions.Enqueue(() =>
            {
                var oldLabel     = leaf.Parent.Label();
                FileSystem.RenameAndMove(leaf, currentPath);
                CopyStateStorage(leaf.Parent, oldLabel);
            });
        ImGuiUtil.HoverTooltip("Enter a full path here to move or rename the leaf. Creates all required parent directories, if possible.");
    }
}
