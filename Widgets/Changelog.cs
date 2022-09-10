using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using OtterGui.Raii;

namespace OtterGui.Widgets;

public sealed class Changelog : Window
{
    public const int FreshInstallVersion = int.MaxValue;

    public const uint DefaultHeaderColor    = 0xFF60D0D0;
    public const uint DefaultHighlightColor = 0xFF6060FF;

    private readonly Func<int>                   _getLastVersion;
    private readonly Action<int>                 _setLastVersion;
    private readonly List<(string, List<Entry>)> _entries = new();

    private int _lastVersion;

    public uint HeaderColor { get; set; } = DefaultHeaderColor;

    public Changelog(string label, Func<int> getLastVersion, Action<int> setLastVersion)
        : base(label, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize,
            true)
    {
        _getLastVersion    = getLastVersion;
        _setLastVersion    = setLastVersion;
        Position           = null;
        RespectCloseHotkey = false;
        ShowCloseButton    = false;
    }

    public override void PreOpenCheck()
    {
        _lastVersion = _getLastVersion();
        if (_lastVersion == FreshInstallVersion)
        {
            IsOpen = false;
            _setLastVersion(_entries.Count);
        }
        else
        {
            IsOpen = _lastVersion < _entries.Count;
        }
    }

    public override void PreDraw()
    {
        Size = new Vector2(Math.Min(ImGui.GetMainViewport().Size.X / 2, 800), ImGui.GetMainViewport().Size.Y / 2);
        ImGuiHelpers.SetNextWindowPosRelativeMainViewport((ImGui.GetMainViewport().Size - Size.Value) / 2, ImGuiCond.Appearing);
    }

    public override void OnClose()
    {
        _setLastVersion(_entries.Count);
    }

    public override void Draw()
    {
        using (var child = ImRaii.Child("Entries", new Vector2(-1, -ImGui.GetFrameHeight() * 2)))
        {
            var i = 0;
            foreach (var (name, list) in _entries.Skip(_lastVersion).Reverse())
            {
                using var id    = ImRaii.PushId(i++);
                using var color = ImRaii.PushColor(ImGuiCol.Text, HeaderColor);
                var tree = ImGui.TreeNodeEx(name,
                    ImGuiTreeNodeFlags.NoTreePushOnOpen | (i == 1 ? ImGuiTreeNodeFlags.DefaultOpen : ImGuiTreeNodeFlags.None));
                CopyToClipboard(name, list);
                color.Pop();
                if (tree)
                    foreach (var entry in list)
                        entry.Draw();
            }
        }

        ImGui.SetCursorPosX(Size!.Value.X / 3);
        if (ImGui.Button("Understood", new Vector2(Size.Value.X / 3, 0)))
            _setLastVersion(_entries.Count);
    }

    public Changelog NextVersion(string title)
    {
        _entries.Add((title, new List<Entry>()));
        return this;
    }

    public Changelog RegisterHighlight(string text, ushort level = 0, uint color = DefaultHighlightColor)
    {
        _entries.Last().Item2.Add(new Entry(text, color, level));
        return this;
    }

    public Changelog RegisterEntry(string text, ushort level = 0)
    {
        _entries.Last().Item2.Add(new Entry(text, 0, level));
        return this;
    }

    private readonly struct Entry
    {
        public readonly string Text;
        public readonly uint   Color;
        public readonly ushort SubText;

        public Entry(string text, uint color = 0, ushort subText = 0)
        {
            Text    = text;
            Color   = color;
            SubText = subText;
        }

        public void Draw()
        {
            using var tab   = ImRaii.PushIndent(1 + SubText);
            using var color = ImRaii.PushColor(ImGuiCol.Text, Color, Color != 0);
            ImGui.Bullet();
            ImGui.PushTextWrapPos();
            ImGui.TextUnformatted(Text);
            ImGui.PopTextWrapPos();
        }

        public void Append(StringBuilder sb)
        {
            sb.Append("> ");
            if (SubText > 0)
                sb.Append('`');
            for (var i = 0; i < SubText; ++i)
                sb.Append("    ");
            if (SubText > 0)
                sb.Append('`');
            if (Color != 0)
                sb.Append("**");
            sb.Append("- ")
                .Append(Text);
            if (Color != 0)
                sb.Append("**");

            sb.Append('\n');
        }
    }

    [Conditional("DEBUG")]
    private static void CopyToClipboard(string name, List<Entry> entries)
    {
        try
        {
            if (!ImGui.IsItemClicked(ImGuiMouseButton.Right))
                return;

            var sb = new StringBuilder(1024 * 64);
            sb.Append("__**")
                .Append(name)
                .Append(" notes, Update <t:")
                .Append(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                .Append(">**__\n");

            foreach (var entry in entries)
                entry.Append(sb);

            ImGui.SetClipboardText(sb.ToString());
        }
        catch
        {
            // ignored
        }
    }
}
