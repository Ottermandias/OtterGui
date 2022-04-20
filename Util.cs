using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using ImGuiNET;
using OtterGui.Raii;

namespace OtterGui;

public static partial class ImGuiUtil
{
    // Go to the next column, then enter text.
    public static void TextNextColumn(string text)
    {
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(text);
    }

    // Draw a single piece of text in the given color.
    public static void TextColored(uint color, string text)
    {
        using var _ = ImRaii.PushColor(ImGuiCol.Text, color);
        ImGui.TextUnformatted(text);
    }

    // Create a selectable that copies its text to clipboard when clicked.
    // Also adds a tooltip on hover.
    public static void CopyOnClickSelectable(string text)
    {
        if (ImGui.Selectable(text))
            ImGui.SetClipboardText(text);

        HoverTooltip("Click to copy to clipboard.");
    }

    // Draw a single FontAwesomeIcon.
    public static void PrintIcon(FontAwesomeIcon icon)
    {
        using var font = ImRaii.PushFont(UiBuilder.IconFont);
        ImGui.TextUnformatted(icon.ToIconString());
    }

    // Draw a help marker, followed by a label.
    public static void LabeledHelpMarker(string label, string tooltip)
    {
        ImGuiComponents.HelpMarker(tooltip);
        ImGui.SameLine();
        ImGui.Text(label);
        HoverTooltip(tooltip);
    }

    // Drag between min and max with the given speed and format.
    // Has width of width.
    // Returns true if the item was edited but is not active anymore.
    public static bool DragFloat(string label, ref float value, float width, float speed, float min, float max, string format)
    {
        ImGui.SetNextItemWidth(width);
        if (ImGui.DragFloat(label, ref value, speed, min, max, format))
            value = Math.Clamp(value, min, max);

        return ImGui.IsItemDeactivatedAfterEdit();
    }

    // Drag between min and max with the given speed and format.
    // Has width of width.
    // Returns true if the item was edited but is not active anymore.
    public static bool DragInt(string label, ref int value, float width, float speed, int min, int max, string format)
    {
        ImGui.SetNextItemWidth(width);
        if (ImGui.DragInt(label, ref value, speed, min, max, format))
            value = Math.Clamp(value, min, max);

        return ImGui.IsItemDeactivatedAfterEdit();
    }

    public static bool DrawDisabledButton(string label, Vector2 size, string description, bool disabled, bool icon = false)
    {
        using var alpha = ImRaii.PushStyle(ImGuiStyleVar.Alpha, 0.5f, disabled);
        using var font  = ImRaii.PushFont(UiBuilder.IconFont, icon);
        var       ret   = ImGui.Button(label, size);
        alpha.Pop();
        font.Pop();
        HoverTooltip(description);
        return ret && !disabled;
    }

    public static void DrawTextButton(string text, Vector2 size, uint buttonColor)
    {
        using var color = ImRaii.PushColor(ImGuiCol.Button, buttonColor)
            .Push(ImGuiCol.ButtonActive,  buttonColor)
            .Push(ImGuiCol.ButtonHovered, buttonColor);
        ImGui.Button(text, size);
    }

    public static void DrawTextButton(string text, Vector2 size, uint buttonColor, uint textColor)
    {
        using var color = ImRaii.PushColor(ImGuiCol.Button, buttonColor)
            .Push(ImGuiCol.ButtonActive,  buttonColor)
            .Push(ImGuiCol.ButtonHovered, buttonColor)
            .Push(ImGuiCol.Text,          textColor);
        ImGui.Button(text, size);
    }

    public static void HoverTooltip(string tooltip)
    {
        if (tooltip.Length > 0 && ImGui.IsItemHovered())
            ImGui.SetTooltip(tooltip);
    }

    public static bool Checkbox(string label, string description, bool current, Action<bool> setter)
    {
        var tmp    = current;
        var result = ImGui.Checkbox(label, ref tmp);
        HoverTooltip(description);
        if (!result || tmp == current)
            return false;

        setter(tmp);
        return true;
    }

    public static void DrawTableColumn(string text)
    {
        ImGui.TableNextColumn();
        ImGui.Text(text);
    }

    public static bool DrawEditButtonText(int id, string current, out string newText, ref bool edit, Vector2 buttonSize, float inputWidth,
        uint maxLength = 256)
    {
        newText = current;
        var       tmpEdit = edit;
        using var style   = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, ImGui.GetStyle().ItemSpacing / 2);
        using var _       = ImRaii.PushId(id);
        if (DrawDisabledButton(FontAwesomeIcon.Edit.ToIconString(), buttonSize, "Rename", edit, true))
            edit = true;
        ImGui.SameLine();
        style.Pop();
        if (!edit)
        {
            DrawTextButton(current, Vector2.Zero, ImGui.GetColorU32(ImGuiCol.FrameBg));
            return false;
        }

        ImGui.SetNextItemWidth(inputWidth);
        if (edit != tmpEdit)
        {
            ImGui.SetKeyboardFocusHere();
            ImGui.SetItemDefaultFocus();
        }

        if (ImGui.InputText("##rename", ref newText, maxLength, ImGuiInputTextFlags.EnterReturnsTrue))
            return true;

        if (edit == tmpEdit && !ImGui.IsItemActive())
            edit = false;
        return false;
    }

    public static void HoverIcon(ImGuiScene.TextureWrap icon, Vector2 iconSize)
    {
        var size = new Vector2(icon.Width, icon.Height);
        ImGui.Image(icon.ImGuiHandle, iconSize);
        if (iconSize.X > size.X || iconSize.Y > size.Y || !ImGui.IsItemHovered())
            return;

        ImGui.BeginTooltip();
        ImGui.Image(icon.ImGuiHandle, size);
        ImGui.EndTooltip();
    }


    public static void RightAlign(string text, float offset = 0)
    {
        offset = ImGui.GetContentRegionAvail().X - offset - ImGui.CalcTextSize(text).X;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
        ImGui.Text(text);
    }

    public static void RightJustify(string text, uint color)
    {
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() - ImGui.CalcTextSize(text).X);
        TextColored(color, text);
    }

    public static void Center(string text)
    {
        var offset = (ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(text).X) / 2;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
        ImGui.Text(text);
    }

    public static bool OpenNameField(string popupName, ref string newName)
    {
        using var popup = ImRaii.Popup(popupName);
        if (!popup)
            return false;

        if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.Escape)))
            ImGui.CloseCurrentPopup();

        ImGui.SetNextItemWidth(300 * ImGuiHelpers.GlobalScale);
        var enterPressed = ImGui.InputTextWithHint("##newName", "Enter New Name...", ref newName, 64, ImGuiInputTextFlags.EnterReturnsTrue);
        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere();

        if (!enterPressed)
            return false;

        ImGui.CloseCurrentPopup();
        return true;
    }

    public static unsafe bool IsDropping(string name)
        => ImGui.AcceptDragDropPayload(name).NativePtr != null;
}
