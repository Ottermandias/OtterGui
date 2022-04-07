using System;
using ImGuiNET;
using OtterGui.Filesystem;
using OtterGui.Raii;

namespace OtterGui.FileSystem.Selector;

public partial class FileSystemSelector<T, TStateStorage>
{
    public readonly string MoveLabel = string.Empty; // Gets set by setting the label itself.

    // The currently moved object.
    private FileSystem<T>.IPath? _movedPathDragDrop;

    private void DragDropSource(FileSystem<T>.IPath path)
    {
        using var _ = ImRaii.DragDropSource();
        if (!_)
            return;

        var pathName = path.FullName();
        ImGui.SetDragDropPayload(MoveLabel, IntPtr.Zero, 0);
        ImGui.Text($"Moving {pathName}...");
        _movedPathDragDrop = path;
    }

    private void DragDropTarget(FileSystem<T>.IPath path)
    {
        using var _ = ImRaii.DragDropTarget();
        if (!_)
            return;

        if (!ImGuiUtil.IsDropping(MoveLabel) || _movedPathDragDrop == null)
            return;

        var movedPath = _movedPathDragDrop;
        _fsActions.Enqueue(() =>
        {
            if (movedPath is FileSystem<T>.Folder folder)
            {
                var oldLabel = movedPath.Label();
                FileSystem.Move(folder, path is FileSystem<T>.Folder f ? f : path.Parent);
                CopyStateStorage(folder, oldLabel);
            }
            else
            {
                FileSystem.Move(movedPath, path is FileSystem<T>.Folder f ? f : path.Parent);
            }
        });
        _movedPathDragDrop = null;
    }
}
