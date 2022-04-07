using System;
using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;
using OtterGui.Filesystem;
using OtterGui.Raii;

namespace OtterGui.FileSystem.Selector;

public partial class FileSystemSelector<T, TStateStorage>
{
    private int _currentDepth = 0;
    private int _currentIndex = 0;
    private int _currentEnd   = 0;

    private (Vector2, Vector2) DrawStateStruct(StateStruct state)
    {
        return state.Path switch
        {
            FileSystem<T>.Folder f => DrawFolder(f),
            FileSystem<T>.Leaf l   => DrawLeaf(l, state.StateStorage),
            _                      => (Vector2.Zero, Vector2.Zero),
        };
    }

    // Draw a leaf. Returns its item rectangle and manages
    //     - drag'n drop,
    //     - right-click context menu,
    //     - selection.
    private (Vector2, Vector2) DrawLeaf(FileSystem<T>.Leaf leaf, in TStateStorage state)
    {
        using var group = ImRaii.Group();
        DrawLeafName(leaf, state, leaf == SelectedLeaf);
        group.Dispose();

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && SelectedLeaf != leaf)
        {
            var oldData = SelectedLeaf?.Value;
            SelectedLeaf = leaf;
            SelectionChanged?.Invoke(oldData, leaf.Value, state);
        }

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            ImGui.OpenPopup(leaf.Name);

        DragDropSource(leaf);
        DragDropTarget(leaf);
        RightClickContext(leaf);
        return (ImGui.GetItemRectMin(), ImGui.GetItemRectMax());
    }

    // Used for clipping. If we start with an object not on depth 0,
    // we need to add its indentation and the folder-lines for it.
    private void DrawPseudoFolders()
    {
        var first   = _state[_currentIndex]; // The first object drawn during this iteration
        var parents = first.Path.Parents(first.Depth);

        // Push IDs in order and indent.
        foreach (var p in parents)
            ImGui.PushID(p.Name);
        ImGui.Indent(ImGui.GetStyle().IndentSpacing * parents.Count);

        // Get start point for the lines (top of the selector).
        var lineStart = ImGui.GetCursorScreenPos();

        // For each pseudo-parent in reverse order draw its children as usual, starting from _currentIndex.
        for (_currentDepth = parents.Count; _currentDepth > 0; --_currentDepth)
        {
            DrawChildren(lineStart);
            lineStart.X -= ImGui.GetStyle().IndentSpacing;
            ImGui.PopID();
            ImGui.Unindent();
        }
    }

    // Used for clipping. If we end not on depth 0 we need to check
    // whether to terminate the folder lines at that point or continue them to the end of the screen.
    private Vector2 AdjustedLineEnd(Vector2 lineEnd)
    {
        if (_currentIndex != _currentEnd)
            return lineEnd;

        // Continue iterating from the current end.
        for (var idx = _currentEnd; idx < _state.Count; ++idx)
        {
            var state = _state[idx];
            // If we find an object at the same depth, the current folder continues
            // and the line has to go out of the screen.
            if (state.Depth == _currentDepth)
            {
                lineEnd.Y = ImGui.GetWindowHeight() + ImGui.GetWindowPos().Y;
                return lineEnd;
            }

            // If we find an object at a lower depth before reaching current depth,
            // the current folder stops and the line should stop at the last drawn child, too.
            if (state.Depth < _currentDepth)
                return lineEnd;
        }

        // All children are in subfolders of this one, but this folder has no further children on its own.
        return lineEnd;
    }

    // Draw children of a folder or pseudo-folder with a given line start using the current index and end.
    private void DrawChildren(Vector2 lineStart)
    {
        // Folder line stuff.
        var offsetX  = -ImGui.GetStyle().IndentSpacing + ImGui.GetTreeNodeToLabelSpacing() / 2;
        var drawList = ImGui.GetWindowDrawList();
        lineStart.X += offsetX;
        lineStart.Y -= 2 * ImGuiHelpers.GlobalScale;
        var lineEnd = lineStart;

        for (; _currentIndex < _currentEnd; ++_currentIndex)
        {
            // If we leave _currentDepth, its not a child of the current folder anymore.
            var state = _state[_currentIndex];
            if (state.Depth != _currentDepth)
                break;

            var lineSize = Math.Max(0, ImGui.GetStyle().IndentSpacing - 9 * ImGuiHelpers.GlobalScale);
            // Draw the child
            var (minRect, maxRect) = DrawStateStruct(state);
            if (minRect.X == 0)
                continue;

            // Draw the notch and increase the line length.
            var midPoint = (minRect.Y + maxRect.Y) / 2f - 1f;
            drawList.AddLine(new Vector2(lineStart.X, midPoint), new Vector2(lineStart.X + lineSize, midPoint), FolderLineColor,
                ImGuiHelpers.GlobalScale);
            lineEnd.Y = midPoint;
        }

        // Finally, draw the folder line.
        drawList.AddLine(lineStart, AdjustedLineEnd(lineEnd), FolderLineColor, ImGuiHelpers.GlobalScale);
    }

    // Draw a folder. Handles
    //     - drag'n drop
    //     - right-click context menus
    //     - expanding/collapsing
    private (Vector2, Vector2) DrawFolder(FileSystem<T>.Folder folder)
    {
        var       expandedState = ImGui.GetStateStorage().GetBool(ImGui.GetID(folder.Name), false);
        using var color         = ImRaii.PushColor(ImGuiCol.Text, expandedState ? ExpandedFolderColor : CollapsedFolderColor);
        var       recurse       = ImGui.TreeNodeEx(folder.Name);

        if (expandedState != recurse)
            AddOrRemoveDescendants(folder, recurse);

        color.Pop();

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            ImGui.OpenPopup(folder.Name);
        DragDropSource(folder);
        DragDropTarget(folder);
        RightClickContext(folder);

        var rect = (ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

        // If the folder is expanded, draw its children one tier deeper.
        if (recurse)
        {
            ++_currentDepth;
            ++_currentIndex;
            DrawChildren(ImGui.GetCursorScreenPos());
            ImGui.TreePop();
            --_currentIndex;
            --_currentDepth;
        }

        return rect;
    }


    // Draw the whole list.
    private bool DrawList(float width)
    {
        // Filter row is outside the child for scrolling.
        DrawFilterRow(width);

        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        using var _     = ImRaii.Child(Label, new Vector2(width, -ImGui.GetFrameHeight()), true);
        if (!_)
            return false;

        style.Pop();
        style.Push(ImGuiStyleVar.IndentSpacing, 14f * ImGuiHelpers.GlobalScale)
            .Push(ImGuiStyleVar.ItemSpacing,  new Vector2(ImGui.GetStyle().ItemSpacing.X, ImGuiHelpers.GlobalScale))
            .Push(ImGuiStyleVar.FramePadding, new Vector2(ImGuiHelpers.GlobalScale,       ImGui.GetStyle().FramePadding.Y));

        // Check if filters are dirty and recompute them before the draw iteration if necessary.
        ApplyFilters();

        ImGuiListClipperPtr clipper;
        unsafe
        {
            clipper = new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
        }

        clipper.Begin(_state.Count, ImGui.GetTextLineHeightWithSpacing());

        // Draw the clipped list.
        while (clipper.Step())
        {
            _currentIndex = clipper.DisplayStart;
            _currentEnd   = clipper.DisplayEnd;
            if (_currentIndex < _currentEnd)
            {
                if (_state[_currentIndex].Depth != 0)
                    DrawPseudoFolders();
                for (; _currentIndex < _currentEnd; ++_currentIndex)
                    DrawStateStruct(_state[_currentIndex]);
            }
        }

        // Handle all queued actions at the end of the iteration.
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
        HandleActions();
        style.Push(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        return true;
    }
}
