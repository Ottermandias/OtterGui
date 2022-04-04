using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using ImGuiNET;
using OtterGui.Filesystem;

namespace OtterGui;

public partial class FileSystemSelector<T, TStateStorage>
{
    public void EnqueueFsAction(Action action)
        => _fsActions.Enqueue(action);

    private readonly Queue<Action> _fsActions = new();

    private void HandleActions()
    {
        while (_fsActions.TryDequeue(out var action))
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                PluginLog.Warning(e.Message);
            }
        }
    }

    private static void AddPrioritizedDelegate<TDelegate>(List<(TDelegate, int)> list, TDelegate action) where TDelegate : Delegate
    {
        var idxAction = list.FindIndex(p => p.Item1 == action);
        if (idxAction >= 0)
            list.RemoveAt(idxAction);
    }

    private static void RemovePrioritizedDelegate<TDelegate>(List<(TDelegate, int)> list, TDelegate action, int priority)
        where TDelegate : Delegate
    {
        var idxAction = list.FindIndex(p => p.Item1 == action);
        if (idxAction >= 0)
        {
            if (list[idxAction].Item2 == priority)
                return;

            list.RemoveAt(idxAction);
        }

        var idx = list.FindIndex(p => p.Item2 > priority);
        if (idx < 0)
            list.Add((action, priority));
        else
            list.Insert(idx, (action, priority));
    }

    private static void ToggleDescendants(FileSystem<T>.Folder folder, int value)
    {
        var rootPath = folder.Label();
        var id       = ImGui.GetID(rootPath);
        var storage  = ImGui.GetStateStorage();
        storage.SetInt(id, value);
        foreach (var child in folder.GetAllDescendants(SortMode.Lexicographical).OfType<FileSystem<T>.Folder>())
        {
            var path = child.Label();
            id = ImGui.GetID(path);
            storage.SetInt(id, value);
        }
    }

    private static void CopyStateStorage(ImGuiStoragePtr stateStorage, int state, FileSystem<T>.Folder folder)
    {
        if (state == 0)
            stateStorage.SetInt(ImGui.GetID(folder.Label()), 0);
        else
            foreach (var label in folder.AllLabels())
                stateStorage.SetInt(ImGui.GetID(label), state);
    }
}
