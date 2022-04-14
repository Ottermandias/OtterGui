using System;
using System.Collections.Generic;
using ImGuiNET;
using OtterGui.Filesystem;
using OtterGui.Raii;

namespace OtterGui.FileSystem.Selector;

public partial class FileSystemSelector<T, TStateStorage> where T : class where TStateStorage : struct
{
    public delegate void SelectionChangeDelegate(T? oldSelection, T? newSelection, in TStateStorage state);

    // The currently selected leaf, if any.
    protected FileSystem<T>.Leaf? SelectedLeaf;

    // The currently selected value, if any.
    public T? Selected
        => SelectedLeaf?.Value;

    // Fired after the selected leaf changed.
    public event SelectionChangeDelegate? SelectionChanged;

    public void ClearSelection()
    {
        if (SelectedLeaf == null)
            return;

        var old = SelectedLeaf?.Value;
        SelectedLeaf = null;
        SelectionChanged?.Invoke(old, null, default);
    }

    protected readonly FileSystem<T> FileSystem;

    public virtual SortMode SortMode
        => SortMode.Lexicographical;

    // Used by Add and AddFolder buttons.
    private string _newName = string.Empty;

    private readonly string _label = string.Empty;

    public string Label
    {
        get => _label;
        init
        {
            _label    = value;
            MoveLabel = $"{value}Move";
        }
    }

    // Default color for tree expansion lines.
    protected virtual uint FolderLineColor
        => 0xFFFFFFFF;

    // Default color for folder names.
    protected virtual uint ExpandedFolderColor
        => 0xFFFFFFFF;

    protected virtual uint CollapsedFolderColor
        => 0xFFFFFFFF;

    public FileSystemSelector(FileSystem<T> fileSystem, string label = "##FileSystemSelector")
    {
        FileSystem = fileSystem;
        _state     = new List<StateStruct>(FileSystem.Root.TotalDescendants);
        Label      = label;
        InitDefaultContext();
        InitDefaultButtons();
        EnableFileSystemSubscription();
    }

    // Default flags to use for custom leaf nodes.
    protected const ImGuiTreeNodeFlags LeafFlags = ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.Bullet;

    // Customization point: Should always create a tree node using LeafFlags (with possible selection.)
    // But can add additional icons or buttons if wanted.
    // Everything drawn in here is wrapped in a group.
    protected virtual void DrawLeafName(FileSystem<T>.Leaf leaf, in TStateStorage state, bool selected)
    {
        var       flag = selected ? ImGuiTreeNodeFlags.Selected | LeafFlags : LeafFlags;
        using var _    = ImRaii.TreeNode(leaf.Name, flag);
    }

    public void Draw(float width)
    {
        using var group = ImRaii.Group();
        if (DrawList(width))
        {
            ImGui.PopStyleVar();
            if (width < 0)
                width = ImGui.GetWindowWidth() - width;
            DrawButtons(width);
        }
    }
}
