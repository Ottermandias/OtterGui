using Dalamud.Interface.Utility;
using ImGuiNET;
using OtterGui.Raii;
using OtterGui.Text;
using OtterGui.Text.Widget;

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
        return ImUtf8.Checkbox(Names[idx], ref ret);
    }

    public override bool DrawFilter()
    {
        using var id    = ImUtf8.PushId(FilterLabel);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0);
        ImGui.SetNextItemWidth(-Table.ArrowWidth * ImGuiHelpers.GlobalScale);
        var       all   = FilterValue.HasFlag(AllFlags);
        using var color = ImRaii.PushColor(ImGuiCol.FrameBg, 0x803030A0, !all);
        using var combo = ImUtf8.Combo(""u8, Label, ComboFlags);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            SetValue(AllFlags, true);
            return true;
        }

        if (!all)
            ImUtf8.HoverTooltip("Right-click to clear filters."u8);

        if (!combo)
            return false;

        color.Pop();

        var ret = false;
        if (ImUtf8.Checkbox("Enable All"u8, ref all))
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

public abstract class TriStateColumnFlags<T, TItem> : Column<TItem> where T : struct, Enum
{
    public    T               AllFlags   = default;
    protected ImGuiComboFlags ComboFlags = ImGuiComboFlags.NoArrowButton;

    protected abstract IReadOnlyList<(T On, T Off)> Values { get; }

    protected abstract string[] Names { get; }

    public virtual T FilterValue
        => default;

    protected virtual void SetValue((T On, T Off) value, bool? enable)
    { }

    protected virtual void SetValue(T flags, bool value)
    { }

    protected virtual bool DrawCheckbox(int idx, out bool? ret)
    {
        var (on, off) = Values[idx];
        bool? current = FilterValue.HasFlag(on)
            ? FilterValue.HasFlag(off)
                ? null
                : true
            : false;

        return TriStateCheckbox.Instance.Draw(Names[idx], current, out ret);
    }

    public override bool DrawFilter()
    {
        using var id    = ImUtf8.PushId(FilterLabel);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0);
        ImGui.SetNextItemWidth(-Table.ArrowWidth * ImGuiHelpers.GlobalScale);
        var       all   = FilterValue.HasFlag(AllFlags);
        using var color = ImRaii.PushColor(ImGuiCol.FrameBg, 0x803030A0, !all);
        using var combo = ImUtf8.Combo(""u8, Label, ComboFlags);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            SetValue(AllFlags, true);
            return true;
        }

        if (!all && ImGui.IsItemHovered())
            ImUtf8.HoverTooltip("Right-click to clear filters."u8);

        if (!combo)
            return false;

        color.Pop();

        var ret = false;
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
