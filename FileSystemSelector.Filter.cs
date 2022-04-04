using System.Linq;
using System.Numerics;
using ImGuiNET;
using OtterGui.Filesystem;
using OtterGui.Raii;

namespace OtterGui;

public partial class FileSystemSelector<T, TStateStorage>
{
    protected string FilterValue  = string.Empty;
    private   bool   _filterDirty = true;

    protected virtual bool ChangeFilter(string filterValue)
        => true;

    protected virtual void CustomFilters(Vector2 cursorPos, float width)
    { }

    protected void SetFilterDirty()
        => _filterDirty = true;

    protected void DrawFilterRow(float width)
    {
        var       pos   = ImGui.GetCursorPos();
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero).Push(ImGuiStyleVar.FrameRounding, 0);
        ImGui.SetNextItemWidth(width);
        if (ImGui.InputTextWithHint("##Filter", "Filter...", ref FilterValue, 64) && ChangeFilter(FilterValue))
            SetFilterDirty();
        style.Pop();

        CustomFilters(pos, width);
    }

    protected virtual bool ApplyFilters(FileSystem<T>.IPath path)
        => FilterValue.Length != 0 && !path.FullName().Contains(FilterValue);

    private bool ApplyFiltersInternal(FileSystem<T>.IPath path, bool currentlyExpanded)
    {
        if (!_state.TryGetValue(path, out var state))
            state = new StateStruct()
            {
                Expanded     = ImGui.GetStateStorage().GetBool(ImGui.GetID(path.Label())),
                StateStorage = default,
                Filtered     = false,
                Visible      = currentlyExpanded,
            };

        var filtered = ApplyFilters(path);
        if (path is FileSystem<T>.Folder f)
            filtered = f.Children.Aggregate(filtered,
                (current, child) => current & ApplyFiltersInternal(child, currentlyExpanded && state.Expanded));
        state.Filtered = filtered;
        _state[path]   = state;
        if (!state.Filtered && currentlyExpanded)
        {
            if (path is FileSystem<T>.Folder)
            {
                ++_visibleFolders;
                if (state.Expanded)
                    ++_expandedFolders;
            }
            else
            {
                ++_visibleLeaves;
            }

            ++_visibleDescendants;
        }

        return filtered;
    }


    private void ApplyFilters()
    {
        if (!_filterDirty)
            return;

        _visibleDescendants = 0;
        _visibleFolders  = 0;
        _expandedFolders = 0;
        _visibleLeaves   = 0;
        foreach (var child in FileSystem.Root.Children)
            ApplyFiltersInternal(child, true);
    }
}
