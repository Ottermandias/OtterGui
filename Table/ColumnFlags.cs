using System;
using System.Collections.Generic;
using Dalamud.Interface;
using ImGuiNET;

namespace OtterGui.Table;

public class ColumnFlags<T, TItem> : Column<TItem> where T : struct, Enum
{
    public T AllFlags = default;

    protected virtual IReadOnlyList<T> Values
        => Enum.GetValues<T>();

    protected virtual string[] Names
        => Enum.GetNames<T>();

    public virtual T FilterValue
        => default;

    protected virtual void SetValue(T value, bool enable)
    { }

    public override bool DrawFilter()
    {
        using var id    = Raii.ImRaii.PushId(FilterLabel);
        using var style = Raii.ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0);
        ImGui.SetNextItemWidth(-Table.ArrowWidth * ImGuiHelpers.GlobalScale);
        var       all   = FilterValue.HasFlag(AllFlags);
        using var color = Raii.ImRaii.PushColor(ImGuiCol.FrameBg, 0x803030A0, !all);
        if (!ImGui.BeginCombo(string.Empty, Label, ImGuiComboFlags.NoArrowButton))
            return false;

        color.Pop();

        using var end = Raii.ImRaii.DeferredEnd(ImGui.EndCombo);

        var ret = false;
        if (ImGui.Checkbox("Enable All", ref all))
        {
            SetValue(AllFlags, all);
            ret = true;
        }

        using var indent = Raii.ImRaii.PushIndent(10f);
        for (var i = 0; i < Names.Length; ++i)
        {
            var tmp = FilterValue.HasFlag(Values[i]);
            if (!ImGui.Checkbox(Names[i], ref tmp))
                continue;

            SetValue(Values[i], tmp);
            ret = true;
        }

        return ret;
    }
}
