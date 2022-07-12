using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Dalamud.Interface;
using Dalamud.Logging;
using ImGuiNET;
using OtterGui.Raii;
using static OtterGui.Raii.ImRaii;

namespace OtterGui.Widgets;

public class Tutorial
{
    public record struct Step(string Name, string Text, bool Enabled);

    public uint   HighlightColor { get; init; } = 0xFF20FFFF;
    public uint   BorderColor    { get; init; } = 0xD00000FF;
    public string PopupLabel     { get; init; } = "Tutorial";

    private readonly List<Step> _steps = new();

    public int EndStep
        => _steps.Count;

    public IReadOnlyList<Step> Steps
        => _steps;

    public Tutorial Register(string name, string text)
    {
        _steps.Add(new Step(name, text, true));
        return this;
    }

    public Tutorial Deprecated()
    {
        _steps.Add(new Step(string.Empty, string.Empty, false));
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public void Open(int id, int current, Action<int> setter)
    {
        if (current != id)
            return;

        OpenWhenMatch(current, setter);
    }

    private void OpenWhenMatch(int current, Action<int> setter)
    {
        var step = Steps[current];

        // Skip disabled tutorials.
        if (!step.Enabled)
        {
            setter(NextId(current));
            return;
        }

        if (ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows) || ImGui.IsWindowAppearing())
            ImGui.OpenPopup(PopupLabel);

        var windowPos = HighlightObject();
        DrawPopup(windowPos, step, NextId(current), setter);
    }

    private Vector2 HighlightObject()
    {
        var offset = ImGuiHelpers.ScaledVector2(5, 4);
        var min    = ImGui.GetItemRectMin() - offset;
        var max    = ImGui.GetItemRectMax() + offset;
        ImGui.GetForegroundDrawList().AddRect(min, max, HighlightColor, 5 * ImGuiHelpers.GlobalScale, ImDrawFlags.RoundCornersAll,
            2 * ImGuiHelpers.GlobalScale);
        return max + new Vector2(ImGuiHelpers.GlobalScale);
    }

    private void DrawPopup(Vector2 pos, Step step, int next, Action<int> setter)
    {
        using var style = DefaultStyle()
            .Push(ImGuiStyleVar.PopupBorderSize, 2 * ImGuiHelpers.GlobalScale)
            .Push(ImGuiStyleVar.PopupRounding,   5 * ImGuiHelpers.GlobalScale);
        using var color = DefaultColors()
            .Push(ImGuiCol.Border,  BorderColor)
            .Push(ImGuiCol.PopupBg, 0xFF000000);
        using var font = DefaultFont();
        ImGui.SetNextWindowPos(pos);
        ImGui.SetNextWindowSize(ImGuiHelpers.ScaledVector2(350, 0));
        using var popup = Popup(PopupLabel, ImGuiWindowFlags.AlwaysAutoResize);
        if (!popup)
            return;

        ImGui.TextUnformatted(step.Name);
        ImGui.Separator();
        ImGui.PushTextWrapPos();
        foreach (var text in step.Text.Split('\n', StringSplitOptions.TrimEntries))
        {
            if (text.Length == 0)
                ImGui.Spacing();
            else
                ImGui.TextUnformatted(text);
        }

        ImGui.PopTextWrapPos();
        ImGui.NewLine();
        var buttonText = next == EndStep ? "Finish" : "Next";
        if (ImGui.Button(buttonText))
        {
            setter(next);
            ImGui.CloseCurrentPopup();
        }

        ImGui.SameLine();
        if (ImGui.Button("Skip Tutorial"))
        {
            setter(EndStep);
            ImGui.CloseCurrentPopup();
        }

        ImGuiUtil.HoverTooltip("Skip all current tutorial entries, but show any new ones added later.");

        ImGui.SameLine();
        if (ImGui.Button("Disable Tutorial"))
        {
            setter(-1);
            ImGui.CloseCurrentPopup();
        }

        ImGuiUtil.HoverTooltip("Disable all tutorial entries.");
    }

    private int NextId(int current)
    {
        for (var i = current + 1; i < EndStep; ++i)
        {
            if (Steps[i].Enabled)
                return i;
        }

        return EndStep;
    }

    // Obtain the current ID if it is enabled, and otherwise the first enabled id after it.
    public int CurrentEnabledId(int current)
    {
        if (current < 0)
            return -1;

        for (var i = current; i < EndStep; ++i)
        {
            if (Steps[i].Enabled)
                return i;
        }

        return EndStep;
    }

    // Make sure you have as many tutorials registered as you intend to.
    public Tutorial EnsureSize(int size)
    {
        if (_steps.Count != size)
            throw new Exception("Tutorial size is incorrect.");

        return this;
    }
}
