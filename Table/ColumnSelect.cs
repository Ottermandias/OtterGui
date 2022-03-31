using System;
using System.Collections.Generic;
using Dalamud.Interface;
using ImGuiNET;

namespace OtterGui.Table;

public class ColumnSelect<T, TItem> : Column<TItem> where T : struct, Enum, IEquatable<T>
{
    public ColumnSelect(T initialValue)
        => FilterValue = initialValue;

    protected virtual IReadOnlyList<T> Values
        => Enum.GetValues<T>();

    protected virtual string[] Names
        => Enum.GetNames<T>();

    protected virtual void SetValue(T value)
        => FilterValue = value;

    public    T   FilterValue;
    protected int Idx = -1;

    public override bool DrawFilter()
    {
        var       tmp   = Idx;
        using var id    = Raii.ImRaii.PushId(FilterLabel);
        using var style = Raii.ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0);
        ImGui.SetNextItemWidth(-Table.ArrowWidth * ImGuiHelpers.GlobalScale);
        if (!ImGui.BeginCombo(string.Empty, Idx < 0 ? Label : Names[Idx]))
            return false;

        using var end = Raii.ImRaii.DeferredEnd(ImGui.EndCombo);
        var       ret = false;
        for (var i = 0; i < Names.Length; ++i)
        {
            if (FilterValue.Equals(Values[i]))
                Idx = i;
            if (!ImGui.Selectable(Names[i], Idx == i) || Idx == i)
                continue;

            Idx = i;
            SetValue(Values[i]);
            ret = true;
        }

        return ret;
    }
}
