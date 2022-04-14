using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;

namespace OtterGui.Raii;

public static partial class ImRaii
{
    public static EndStack DeferredEnd(Action a, bool condition = true)
        => new EndStack().Push(a, condition);

    public static EndStack NewGroup()
    {
        ImGui.BeginGroup();
        return DeferredEnd(ImGui.EndGroup);
    }

    public static EndStack NewTooltip()
    {
        ImGui.BeginTooltip();
        return DeferredEnd(ImGui.EndTooltip);
    }

    public sealed class EndStack : IDisposable
    {
        private readonly Stack<Action> _cleanActions = new();

        public EndStack Push(Action a, bool condition = true)
        {
            if (condition)
                _cleanActions.Push(a);

            return this;
        }


        public EndStack Pop(int num = 1)
        {
            while (num-- > 0 && _cleanActions.TryPop(out var action))
                action.Invoke();

            return this;
        }

        public void Dispose()
            => Pop(_cleanActions.Count);
    }

    public static IEndObject Child(string strId)
        => new EndUnconditionally(ImGui.EndChild, ImGui.BeginChild(strId));

    public static IEndObject Child(string strId, Vector2 size)
        => new EndUnconditionally(ImGui.EndChild, ImGui.BeginChild(strId, size));

    public static IEndObject Child(string strId, Vector2 size, bool border)
        => new EndUnconditionally(ImGui.EndChild, ImGui.BeginChild(strId, size, border));

    public static IEndObject Child(string strId, Vector2 size, bool border, ImGuiWindowFlags flags)
        => new EndUnconditionally(ImGui.EndChild, ImGui.BeginChild(strId, size, border, flags));

    public static IEndObject DragDropTarget()
        => new EndConditionally(ImGui.EndDragDropTarget, ImGui.BeginDragDropTarget());

    public static IEndObject DragDropSource()
        => new EndConditionally(ImGui.EndDragDropSource, ImGui.BeginDragDropSource());

    public static IEndObject DragDropSource(ImGuiDragDropFlags flags)
        => new EndConditionally(ImGui.EndDragDropSource, ImGui.BeginDragDropSource(flags));

    public static IEndObject Popup(string id)
        => new EndConditionally(ImGui.EndPopup, ImGui.BeginPopup(id));

    public static IEndObject Popup(string id, ImGuiWindowFlags flags)
        => new EndConditionally(ImGui.EndPopup, ImGui.BeginPopup(id, flags));

    public static IEndObject Combo(string label, string previewValue)
        => new EndConditionally(ImGui.EndCombo, ImGui.BeginCombo(label, previewValue));

    public static IEndObject Combo(string label, string previewValue, ImGuiComboFlags flags)
        => new EndConditionally(ImGui.EndCombo, ImGui.BeginCombo(label, previewValue, flags));

    public static IEndObject Group()
    {
        ImGui.BeginGroup();
        return new EndUnconditionally(ImGui.EndGroup, true);
    }

    public static IEndObject Tooltip()
    {
        ImGui.BeginTooltip();
        return new EndUnconditionally(ImGui.EndTooltip, true);
    }

    public static IEndObject ListBox(string label)
        => new EndConditionally(ImGui.EndListBox, ImGui.BeginListBox(label));

    public static IEndObject ListBox(string label, Vector2 size)
        => new EndConditionally(ImGui.EndListBox, ImGui.BeginListBox(label, size));

    public static IEndObject Table(string table, int numColumns)
        => new EndConditionally(ImGui.EndTable, ImGui.BeginTable(table, numColumns));

    public static IEndObject Table(string table, int numColumns, ImGuiTableFlags flags)
        => new EndConditionally(ImGui.EndTable, ImGui.BeginTable(table, numColumns, flags));

    public static IEndObject Table(string table, int numColumns, ImGuiTableFlags flags, Vector2 outerSize)
        => new EndConditionally(ImGui.EndTable, ImGui.BeginTable(table, numColumns, flags, outerSize));

    public static IEndObject Table(string table, int numColumns, ImGuiTableFlags flags, Vector2 outerSize, float innerWidth)
        => new EndConditionally(ImGui.EndTable, ImGui.BeginTable(table, numColumns, flags, outerSize, innerWidth));

    public static IEndObject TabBar(string label)
        => new EndConditionally(ImGui.EndTabBar, ImGui.BeginTabBar(label));

    public static IEndObject TabBar(string label, ImGuiTabBarFlags flags)
        => new EndConditionally(ImGui.EndTabBar, ImGui.BeginTabBar(label, flags));

    public static IEndObject TabItem(string label)
        => new EndConditionally(ImGui.EndTabItem, ImGui.BeginTabItem(label));

    public static IEndObject TabItem(string label, ref bool open)
        => new EndConditionally(ImGui.EndTabItem, ImGui.BeginTabItem(label, ref open));

    public static IEndObject TabItem(string label, ref bool open, ImGuiTabItemFlags flags)
        => new EndConditionally(ImGui.EndTabItem, ImGui.BeginTabItem(label, ref open, flags));

    public static IEndObject TreeNode(string label)
        => new EndConditionally(ImGui.TreePop, ImGui.TreeNodeEx(label));

    public static IEndObject TreeNode(string label, ImGuiTreeNodeFlags flags)
        => new EndConditionally(ImGui.TreePop, ImGui.TreeNodeEx(label, flags));

    public interface IEndObject : IDisposable
    {
        public bool Success { get; }

        public static bool operator true(IEndObject i)
            => i.Success;

        public static bool operator false(IEndObject i)
            => !i.Success;

        public static bool operator !(IEndObject i)
            => !i.Success;
    }

    private struct EndUnconditionally : IEndObject
    {
        private Action EndAction { get; }
        public  bool   Success   { get; }
        public  bool   Disposed  { get; private set; }

        public EndUnconditionally(Action endAction, bool success)
        {
            EndAction = endAction;
            Success   = success;
            Disposed  = false;
        }

        public void Dispose()
        {
            if (Disposed)
                return;

            EndAction();
            Disposed = true;
        }
    }

    private struct EndConditionally : IEndObject
    {
        private Action EndAction { get; }
        public  bool   Success   { get; }
        public  bool   Disposed  { get; private set; }

        public EndConditionally(Action endAction, bool success)
        {
            EndAction = endAction;
            Success   = success;
            Disposed  = false;
        }

        public void Dispose()
        {
            if (Disposed)
                return;

            if (Success)
                EndAction();
            Disposed = true;
        }
    }
}
