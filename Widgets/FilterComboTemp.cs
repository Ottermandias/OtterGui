using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using OtterGui.Classes;

namespace OtterGui.Widgets;

/// <summary>
/// A wrapper around filterable combos that makes them work with IEnumerables without taking permanent additional memory.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class FilterComboTemp<T> : FilterComboBase<T>
{
    public T? CurrentSelection { get; private set; }

    private readonly TemporaryList<T> _items;
    private          int              _currentSelection = 0;

    protected FilterComboTemp(IEnumerable<T> items, bool keepStorage)
        : base(new TemporaryList<T>(items), keepStorage)
    {
        _items           = (TemporaryList<T>)Items;
        CurrentSelection = Items.FirstOrDefault();
    }

    protected override void Cleanup()
        => _items.ClearList();


    protected override void DrawList(float width, float itemHeight)
    {
        base.DrawList(width, itemHeight);
        if (NewSelection != null && Items.Count > NewSelection.Value)
            CurrentSelection = Items[NewSelection.Value];
    }

    public void Draw(string label, string preview, float previewWidth, float itemHeight, ImGuiComboFlags flags = ImGuiComboFlags.None)
        => Draw(label, preview, ref _currentSelection, previewWidth, itemHeight, flags);
}
