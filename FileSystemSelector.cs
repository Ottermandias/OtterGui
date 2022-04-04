using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;
using OtterGui.Filesystem;
using OtterGui.Raii;

namespace OtterGui;

public partial class FileSystemSelector<T, TStateStorage>
{
    private FileSystem<T>.IPath? _firstVisibleItem   = null;
    private int                  _itemsToSkip        = 0;
    private int                  _itemsToDraw        = 0;
    private float                _lastScrollPosition = 0;
    private float                _lastScrollMax      = 0;
    private float                _lastHeight         = 0;
    private float                _lastTextHeight     = 0;
    private int                  _itemsDrawn         = 0;

    private void SetPositionDirty()
    {
        _firstVisibleItem = null;
        _itemsToSkip      = 0;
        _itemsToDraw      = 0;
    }

    private void DrawEndDummy()
    {
        var sum = _itemsToSkip + _itemsDrawn;
        if (sum < FileSystem.Root.TotalDescendants)
            ImGui.Dummy((_visibleDescendants - sum) * ImGui.GetTextLineHeightWithSpacing() * Vector2.UnitY);
    }

    private void DrawStartDummy()
    {
        if (_itemsToSkip > 0)
            ImGui.Dummy(_itemsToSkip * ImGui.GetTextLineHeightWithSpacing() * Vector2.UnitY);
    }

    private void InitData()
    {
        _itemsDrawn = 0;
    }

    private void CheckScrollPosition()
    {
        var scrollY = ImGui.GetScrollY();
        if (scrollY != _lastScrollPosition)
        {
            _lastScrollPosition = scrollY;
            SetPositionDirty();
        }

        var scrollYMax = ImGui.GetScrollMaxY();
        if (scrollYMax != _lastScrollMax)
        {
            _lastScrollMax = scrollYMax;
            SetPositionDirty();
        }

        var textHeight = ImGui.GetTextLineHeightWithSpacing();
        if (textHeight != _lastTextHeight)
        {
            _lastTextHeight = textHeight;
            SetPositionDirty();
        }

        var height = ImGui.GetWindowHeight();
        if (height != _lastHeight)
        {
            _lastHeight = height;
            SetPositionDirty();
        }
    }

    private void PrepareClipping()
    {
        if (_firstVisibleItem != null)
            return;

        _itemsToDraw = Math.Min(_visibleDescendants, (int)(_lastHeight / _lastTextHeight) + 2);
        _itemsToSkip = Math.Max(0, (int)((_visibleDescendants - _itemsToDraw) * _lastScrollPosition / _lastScrollMax) - 1);

        var counter = _itemsToSkip;
        foreach (var child in FileSystem.Root.GetAllDescendants(SortMode))
        {
            var state = _state[child];
            if (state.Filtered || !state.Visible)
                continue;

            if (counter-- > 0)
                continue;

            _firstVisibleItem = child;
            var parents = child.Depth() - 1;
            _itemsToSkip -= parents;
            _itemsToDraw += parents;
            break;
        }
    }

    private bool CheckFirstChild(ref FileSystem<T>.IPath? firstChild, FileSystem<T>.IPath child)
    {
        if (firstChild == null)
            return true;

        if (firstChild != child)
            return false;

        firstChild = null;
        return true;
    }
}

public partial class FileSystemSelector<T, TStateStorage> where T : class where TStateStorage : struct
{
    public const ImGuiTreeNodeFlags LeafFlags = ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.Bullet;

    // Initial setup.
    protected readonly FileSystem<T> FileSystem;

    public virtual SortMode SortMode
        => SortMode.Lexicographical;

    // Used by Add and AddFolder buttons.
    private string _newName = string.Empty;

    // Labels
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

    private uint _treeLineColor = 0xFFC0A040;

    private struct StateStruct
    {
        public TStateStorage StateStorage;
        public bool          Expanded;
        public bool          Filtered;
        public bool          Visible;
    }

    private int _visibleDescendants = -1;
    private int _visibleFolders     = -1;
    private int _expandedFolders    = -1;
    private int _visibleLeaves      = -1;

    protected FileSystem<T>.Leaf? Selected;

    private readonly Dictionary<FileSystem<T>.IPath, StateStruct> _state;

    public FileSystemSelector(FileSystem<T> fileSystem, string label = "##FileSystemSelector")
    {
        FileSystem = fileSystem;
        _state     = new Dictionary<FileSystem<T>.IPath, StateStruct>(FileSystem.Root.TotalDescendants);
        Label      = label;
        InitDefaultContext();
        InitDefaultButtons();
    }

    protected virtual void DrawLeafName(FileSystem<T>.Leaf leaf, bool selected)
    {
        var flag = selected ? ImGuiTreeNodeFlags.Selected | LeafFlags : LeafFlags;
        if (ImGui.TreeNodeEx(leaf.Name, flag))
            ImGui.TreePop();
    }

    protected virtual (Vector2, Vector2) DrawLeaf(FileSystem<T>.Leaf leaf)
    {
        using var group = ImRaii.Group();
        DrawLeafName(leaf, leaf == Selected);
        group.Dispose();

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            Selected = leaf;
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            ImGui.OpenPopup(leaf.Name);

        DragDropSource(leaf);
        DragDropTarget(leaf);
        RightClickContext(leaf);
        return (ImGui.GetItemRectMin(), ImGui.GetItemRectMax());
    }

    private void SetVisibleDescendants(FileSystem<T>.Folder folder, bool toggledOpen)
    {
        foreach (var child in folder.Children)
            _state[child] = _state[child] with { Visible = toggledOpen };

        _state[folder] = _state[folder] with { Expanded = true };
    }

    private (Vector2, Vector2) DrawFolder(FileSystem<T>.Folder folder, FileSystem<T>.IPath? firstChild = null)
    {
        using var color   = ImRaii.PushColor(ImGuiCol.Text, _treeLineColor);
        var       recurse = ImGui.TreeNodeEx(folder.Name);

        if (ImGui.IsItemToggledOpen())
            SetVisibleDescendants(folder, recurse);

        color.Pop();
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            ImGui.OpenPopup(folder.Name);
        DragDropSource(folder);
        DragDropTarget(folder);
        RightClickContext(folder);
        var rect = (ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

        if (recurse)
        {
            var offsetX   = -ImGui.GetStyle().IndentSpacing + ImGui.GetTreeNodeToLabelSpacing() / 2;
            var drawList  = ImGui.GetWindowDrawList();
            var lineStart = ImGui.GetCursorScreenPos();
            lineStart.X += offsetX;
            lineStart.Y -= 2 * ImGuiHelpers.GlobalScale;
            var lineEnd = lineStart;
            foreach (var child in folder.GetChildren(SortMode))
            {
                if (!CheckFirstChild(ref firstChild, child))
                    continue;

                if (_itemsDrawn >= _itemsToDraw)
                    break;

                var lineSize = Math.Max(0, ImGui.GetStyle().IndentSpacing - 10 * ImGuiHelpers.GlobalScale);
                var (minRect, maxRect) = DrawChild(child);
                if (minRect.X == 0)
                    continue;

                var midPoint = (minRect.Y + maxRect.Y) / 2f;
                drawList.AddLine(new Vector2(lineStart.X, midPoint), new Vector2(lineStart.X + lineSize, midPoint), _treeLineColor,
                    ImGuiHelpers.GlobalScale);
                lineEnd.Y = midPoint;
            }

            drawList.AddLine(lineStart, lineEnd, _treeLineColor, ImGuiHelpers.GlobalScale);
            ImGui.TreePop();
        }

        return rect;
    }

    private void DrawRootFrom(FileSystem<T>.IPath? firstChild = null)
    {
        foreach (var child in FileSystem.Root.GetChildren(SortMode))
        {
            if (!CheckFirstChild(ref firstChild, child))
                continue;

            if (_itemsDrawn >= _itemsToDraw)
                break;

            DrawChild(child);
        }
    }

    private (Vector2, Vector2) DrawChild(FileSystem<T>.IPath path)
    {
        if (!_state.TryGetValue(path, out var state))
        {
            state        = default;
            _state[path] = state;
        }

        if (state.Filtered)
            return (Vector2.Zero, Vector2.Zero);

        ++_itemsDrawn;
        switch (path)
        {
            case FileSystem<T>.Folder f: return DrawFolder(f);
            case FileSystem<T>.Leaf l:   return DrawLeaf(l);
        }

        return (Vector2.Zero, Vector2.Zero);
    }

    private void DrawFirstItemFolders(FileSystem<T>.Folder folder, FileSystem<T>.IPath child, int depth)
    {
        if (folder.Name.Length == 0)
        {
            DrawStartDummy();
            DrawRootFrom(child);
            return;
        }

        DrawFirstItemFolders(folder.Parent, folder, depth);
        ++_itemsDrawn;
        DrawFolder(folder, child);
    }


    private bool DrawList(float width)
    {
        DrawFilterRow(width);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        using var _     = ImRaii.Child(Label, new Vector2(width, -ImGui.GetFrameHeight()), true);
        if (!_)
            return false;

        style.Pop();
        style.Push(ImGuiStyleVar.IndentSpacing, 15f * ImGuiHelpers.GlobalScale);
        _itemsDrawn = 0;

        ApplyFilters();
        CheckScrollPosition();
        PrepareClipping();
        InitData();

        DrawFirstItemFolders(_firstVisibleItem!.Parent, _firstVisibleItem, 1);
        DrawEndDummy();

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
        HandleActions();
        style.Push(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        return true;
    }

    public void DrawDebugInfo()
    {
        using var group = ImRaii.Group();
        ImGui.Text($"{_visibleDescendants} Visible Descendants");
        ImGui.Text($"{_visibleLeaves} Visible Leaves");
        ImGui.Text($"{_visibleFolders} Visible Folders");
        ImGui.Text($"{_expandedFolders} Expanded Folders");
        ImGui.Text($"{_itemsToSkip} Items to Skip");
        ImGui.Text($"{_itemsToDraw} Items to Draw");
        ImGui.Text($"{_itemsDrawn} Drawn Items");
        ImGui.Text($"{_lastHeight} Last Window Height");
        ImGui.Text($"{_lastTextHeight} Last Text Height (with Spacing)");
        ImGui.Text($"{_lastScrollPosition} Last Scroll Position");
        ImGui.Text($"{_lastScrollMax} Last Scroll Max");
        ImGui.Text($"{_firstVisibleItem?.Name ?? "No"} First Item Visible");
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
