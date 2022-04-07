using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using ImGuiNET;
using OtterGui.Filesystem;

namespace OtterGui.FileSystem.Selector;

public partial class FileSystemSelector<T, TStateStorage>
{
    // Some actions should not be done during due to changed collections
    // or dependency on ImGui IDs.
    protected void EnqueueFsAction(Action action)
        => _fsActions.Enqueue(action);

    private readonly Queue<Action> _fsActions = new();

    // Execute all collected actions in the queue, called after creating the selector,
    // but before starting the draw iteration.
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

    // Used for buttons and context menu entries.
    private static void AddPrioritizedDelegate<TDelegate>(List<(TDelegate, int)> list, TDelegate action) where TDelegate : Delegate
    {
        var idxAction = list.FindIndex(p => p.Item1 == action);
        if (idxAction >= 0)
            list.RemoveAt(idxAction);
    }

    // Used for buttons and context menu entries.
    private void RemovePrioritizedDelegate<TDelegate>(List<(TDelegate, int)> list, TDelegate action, int priority)
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

    // Set the expansion state of a specific folder and all its descendant folders to the given value.
    // Can only be executed from the main selector window due to ID computation, so use this only in Enqueued actions.
    // Handles ImGui-state as well as cache-state.
    private void ToggleDescendants(FileSystem<T>.Folder folder, int stateIdx, int value)
    {
        var rootPath = folder.Label();
        var id       = ImGui.GetID(rootPath);
        var storage  = ImGui.GetStateStorage();
        storage.SetInt(id, value);

        RemoveDescendants(stateIdx);
        foreach (var child in folder.GetAllDescendants(SortMode.Lexicographical).OfType<FileSystem<T>.Folder>())
        {
            var path = child.Label();
            id = ImGui.GetID(path);
            storage.SetInt(id, value);
        }

        if (value != 0)
            AddDescendants(folder, stateIdx);
    }

    // Set the state of all parents of a given folder to open,
    // and the folder itself and all its descendants to their previous state before renaming.
    // Can only be executed from the main selector window due to ID computation, so use this only in Enqueued actions.
    // Handles only ImGui-state, since it is used for moves and renames which trigger a filter-recomputation anyway.
    private static void CopyStateStorage(FileSystem<T>.Folder folder, string oldLabel)
    {
        var stateStorage = ImGui.GetStateStorage();
        var mainLabel    = folder.Label();
        // TODO: make more efficient.
        foreach (var label in folder.AllLabels())
            stateStorage.SetInt(ImGui.GetID(label), 1);
        if (stateStorage.GetInt(ImGui.GetID(oldLabel), 0) == 0)
            stateStorage.SetInt(ImGui.GetID(mainLabel), 0);
        else
            foreach (var desc in folder.GetAllDescendants(SortMode.Lexicographical).OfType<FileSystem<T>.Folder>())
            {
                var label = desc.Label();
                var old   = label.Replace(mainLabel, oldLabel);
                var state = stateStorage.GetInt(ImGui.GetID(old), 0);
                stateStorage.SetInt(ImGui.GetID(label), state);
            }
    }
}
