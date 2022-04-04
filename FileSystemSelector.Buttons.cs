using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;
using OtterGui.Raii;

namespace OtterGui;

public partial class FileSystemSelector<T, TStateStorage>
{
    public void AddButton(Action<Vector2> action, int priority)
        => RemovePrioritizedDelegate(_buttons, action, priority);

    public void RemoveButton(Action<Vector2> action)
        => AddPrioritizedDelegate(_buttons, action);


    private readonly List<(Action<Vector2>, int)> _buttons = new(1);

    private void DrawButtons(float width)
    {
        var buttonWidth = new Vector2(width / Math.Max(_buttons.Count, 1), 0);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0f)
            .Push(ImGuiStyleVar.ItemSpacing, Vector2.Zero);

        foreach (var button in _buttons)
        {
            button.Item1.Invoke(buttonWidth);
            ImGui.SameLine();
        }

        ImGui.NewLine();
    }

    private void FolderAddButton(Vector2 size)
    {
        const string newFolderName = "folderName";

        if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.FolderPlus.ToIconString(), size,
                "Create a new, empty folder. Can contain '/' to create a directory structure.", false, true))
            ImGui.OpenPopup(newFolderName);

        if (ImGuiUtil.OpenNameField(newFolderName, ref _newName) && _newName.Length > 0)
            FileSystem.FindOrCreateAllFolders(_newName);
    }

    private void InitDefaultButtons()
    {
        AddButton(FolderAddButton, 50);
    }
}
