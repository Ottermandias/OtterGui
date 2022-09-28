using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;
using OtterGui.Classes;
using OtterGui.Raii;

namespace OtterGui.Widgets;

public abstract class FilteredCombo<T>
{
    private readonly string              _label;
    private readonly float               _width;
    private readonly IReadOnlyList<T>    _items;
    private readonly List<(string, int)> _available;

    private float Width
        => _width * ImGuiHelpers.GlobalScale;

    private LowerString _filter        = string.Empty;
    private int         _lastSelection = -1;
    private bool        _filterDirty   = true;

    protected LowerString Filter
        => _filter;

    public FilteredCombo(string label, float unscaledWidth, IReadOnlyList<T> items)
    {
        _label     = label;
        _width     = unscaledWidth;
        _items     = items;
        _available = new List<(string, int)>(_items.Count);
    }

    // Can be called to manually reset the filter,
    // if it is dependent on things other than the entered string.
    public void ResetFilter()
        => _filterDirty = true;

    public void Draw(int currentSelected)
    {
        UpdateFilter();

        // Set preview value if it is available.
        var preview = currentSelected >= 0 && currentSelected < _items.Count ? ToString(_items[currentSelected]) : string.Empty;

        ImGui.SetNextItemWidth(Width);
        using var combo = ImRaii.Combo(_label, preview, ImGuiComboFlags.PopupAlignLeft);
        if (!combo)
            return;

        // Width of the popup window and text input field.
        var width = ImGui.GetWindowWidth() - ImGui.GetStyle().WindowPadding.X;

        var setScroll = false;
        ImGui.SetNextItemWidth(width);
        // If the popup is opening, set the last selection to the currently selected object, if any,
        // scroll to it, and set keyboard focus to the filter field.
        if (ImGui.IsWindowAppearing())
        {
            _lastSelection = _available.IndexOf(p => p.Item2 == currentSelected);
            setScroll      = true;
            ImGui.SetKeyboardFocusHere();
        }

        // Draw the text input.
        LowerString.InputWithHint("##filter", "Filter...", ref _filter);

        // Enable keyboard navigation for going up and down,
        // jumping if reaching the end. This also scrolls to the element.
        if (_available.Count > 0)
        {
            if (ImGui.IsKeyPressed(ImGuiKey.DownArrow))
                (_lastSelection, setScroll) = ((_lastSelection + 1) % _available.Count, true);
            else if (ImGui.IsKeyPressed(ImGuiKey.UpArrow))
                (_lastSelection, setScroll) = ((_lastSelection - 1 + _available.Count) % _available.Count, true);
        }

        var end = false; // If the popup should be closed.

        // A child for the items, so that the filter remains visible.
        // Height is based on default combo height minus the filter input.
        using (var _ = ImRaii.Child("ChildL",
                   new Vector2(width, ImGui.GetTextLineHeightWithSpacing() * 8 - ImGui.GetFrameHeight() - ImGui.GetStyle().WindowPadding.Y)))
        {
            // Draw all available objects with their name.
            foreach (var ((name, globalIdx), localIdx) in _available.WithIndex())
            {
                using var id = ImRaii.PushId(globalIdx);
                if (ImGui.Selectable(name, _lastSelection == localIdx))
                {
                    Select(globalIdx);
                    end = true;
                }

                // Actually set scroll if necessary.
                if (_lastSelection == localIdx && setScroll)
                    ImGui.SetScrollHereY();
            }
        }

        // Escape closes the popup without selection
        end |= ImGui.IsKeyPressed(ImGuiKey.Escape);

        // Enter selects the current selection if any, or the first available item.
        if (ImGui.IsKeyPressed(ImGuiKey.Enter))
        {
            if (_lastSelection >= 0)
                Select(_available[_lastSelection].Item2);
            else if (_available.Count > 0)
                Select(_available[0].Item2);

            end = true;
        }

        // Close the popup and reset state.
        if (end)
        {
            _filter        = LowerString.Empty;
            _lastSelection = -1;
            ImGui.CloseCurrentPopup();
        }
    }

    // Be stateful and update the filter whenever it gets dirty.
    // This is when the string is changed or on manual calls.
    private void UpdateFilter()
    {
        if (!_filterDirty)
            return;


        // Keep the selected key if possible.
        var lastSelection = _lastSelection == -1 ? -1 : _available[_lastSelection].Item2;
        _lastSelection = -1;

        _available.Clear();
        foreach (var (obj, idx) in _items.WithIndex().Where(p => IsVisible(p.Item1)))
        {
            if (lastSelection == idx)
                _lastSelection = _available.Count;
            _available.Add((ToString(obj), idx));
        }
    }

    protected abstract bool   IsVisible(T obj);
    protected abstract void   Select(int globalIdx);
    protected abstract string ToString(T obj);
}
