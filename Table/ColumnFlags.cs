using Dalamud.Interface.Utility;
using ImGuiNET;
using OtterGui.Raii;

namespace OtterGui.Table;

public class ColumnFlags<T, TItem> : Column<TItem> where T : struct, Enum
{
    public    T               AllFlags   = default;
    protected ImGuiComboFlags ComboFlags = ImGuiComboFlags.NoArrowButton;

    protected virtual IReadOnlyList<T> Values
        => Enum.GetValues<T>();

    protected virtual string[] Names
        => Enum.GetNames<T>();

    public virtual T FilterValue
        => default;

    protected virtual void SetValue(T value, bool enable)
    { }

    protected virtual bool DrawCheckbox(int idx, out bool ret)
    {
        ret = FilterValue.HasFlag(Values[idx]);
        return ImGui.Checkbox(Names[idx], ref ret);
    }

    public override bool DrawFilter()
    {
        using var id    = ImRaii.PushId(FilterLabel);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0);
        ImGui.SetNextItemWidth(-Table.ArrowWidth * ImGuiHelpers.GlobalScale);
        var       all   = FilterValue.HasFlag(AllFlags);
        using var color = ImRaii.PushColor(ImGuiCol.FrameBg, 0x803030A0, !all);
        using var combo = ImRaii.Combo(string.Empty, Label, ComboFlags);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            SetValue(AllFlags, true);
            return true;
        }

        if (!all && ImGui.IsItemHovered())
            ImGui.SetTooltip("Right-click to clear filters.");

        if (!combo)
            return false;

        color.Pop();

        var ret = false;
        if (ImGui.Checkbox("Enable All", ref all))
        {
            SetValue(AllFlags, all);
            ret = true;
        }

        using var indent = ImRaii.PushIndent(10f);
        for (var i = 0; i < Names.Length; ++i)
        {
            if (!DrawCheckbox(i, out var tmp))
                continue;

            SetValue(Values[i], tmp);
            ret = true;
        }

        return ret;
    }
}
