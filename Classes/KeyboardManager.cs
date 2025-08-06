using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
using OtterGui.Services;
using OtterGui.Text;

namespace OtterGui.Classes;

public class KeyboardManager : IService, IDisposable
{
    private readonly IFramework                         _framework;
    private readonly IKeyState                          _keyState;
    private          int                                _debugCount     = 0;
    private readonly Dictionary<ModifiableHotkey, bool> _registeredKeys = [];

    public KeyboardManager(IFramework framework, IKeyState keyState)
    {
        _framework        =  framework;
        _keyState         =  keyState;
        _framework.Update += OnUpdate;
    }

    public ModifiableHotkey RegisterKey(ModifiableHotkey key)
    {
        var modifiers = key.Modifier1.IsActive() && key.Modifier2.IsActive();
        if (_registeredKeys.TryAdd(key, modifiers) && modifiers)
            ImGui.GetIO().WantTextInput = true;
        return key;
    }

    public ModifiableHotkey RegisterKey(VirtualKey key)
        => RegisterKey(new ModifiableHotkey(key));

    public ModifiableHotkey RegisterKey(VirtualKey key, ModifierHotkey modifier)
        => RegisterKey(new ModifiableHotkey(key, modifier));

    public ModifiableHotkey RegisterKey(VirtualKey key, ModifierHotkey modifier, ModifierHotkey modifier2)
        => RegisterKey(new ModifiableHotkey(key, modifier, modifier2));

    private void OnUpdate(IFramework framework)
    {
        foreach (var (key, modifiers) in _registeredKeys)
        {
            if (modifiers)
                _keyState[key.Hotkey] = false;
        }

        _registeredKeys.Clear();
    }

    public void DrawDebug()
    {
        ImUtf8.Button("Press Ctrl + V"u8);
        if (ImGui.IsItemHovered())
        {
            var key = RegisterKey(VirtualKey.V, ModifierHotkey.Control);
            if (key.Modifier1.IsActive() && ImGui.IsKeyPressed(ImGuiHelpers.VirtualKeyToImGuiKey(key.Hotkey)))
                ++_debugCount;
        }

        ImGui.SameLine();
        ImUtf8.Text($"Pressed {_debugCount} times.");

        ImGui.Separator();
        var active   = ImGui.GetColorU32(ImGuiCol.Button);
        var inactive = ImGui.GetColorU32(ImGuiCol.FrameBg);
        ImUtf8.TextFramed("Control"u8, ModifierHotkey.Control.IsActive() ? active : inactive);
        ImUtf8.SameLineInner();
        ImUtf8.TextFramed("Shift"u8, ModifierHotkey.Shift.IsActive() ? active : inactive);
        ImUtf8.SameLineInner();
        ImUtf8.TextFramed("Alt"u8, ModifierHotkey.Alt.IsActive() ? active : inactive);
        ImUtf8.SameLineInner();
        ImUtf8.TextFramed("Want Text Input"u8, ImGui.GetIO().WantTextInput ? active : inactive);
        ImUtf8.SameLineInner();
        ImUtf8.TextFramed("Capture Keyboard"u8, ImGui.GetIO().WantCaptureKeyboard ? active : inactive);

        ImGui.Separator();
        using (var table = ImUtf8.Table("table"u8, 3))
        {
            if (!table)
                return;

            foreach (var (key, modifiers) in _registeredKeys)
            {
                ImUtf8.DrawTableColumn($"{key}");
                var pressed = ImGui.IsKeyDown(ImGuiHelpers.VirtualKeyToImGuiKey(key.Hotkey));
                ImUtf8.DrawTableColumn(pressed ? "Pressed"u8 : "No Input"u8);
                ImUtf8.DrawTableColumn(modifiers ? "Modifiers Active"u8 : "Modifiers Inactive"u8);
            }
        }

        ImGui.Separator();

        using var tree = ImUtf8.TreeNode("Full Keystate"u8);
        if (!tree)
            return;

        using var table2 = ImUtf8.Table("table2"u8, 4, ImGuiTableFlags.SizingFixedFit);
        if (!table2)
            return;

        ImUtf8.TableSetupColumn("Fancy Name"u8);
        ImUtf8.TableSetupColumn("ImGuiKey"u8);
        ImUtf8.TableSetupColumn("Game State"u8);
        ImUtf8.TableSetupColumn("ImGui State"u8);

        ImGui.TableHeadersRow();
        foreach (var key in _keyState.GetValidVirtualKeys())
        {
            var imguiKey = ImGuiHelpers.VirtualKeyToImGuiKey(key);
            ImUtf8.DrawTableColumn(key.GetFancyName());
            ImUtf8.DrawTableColumn($"{imguiKey}");
            ImUtf8.DrawTableColumn(_keyState[key] ? "Active"u8 : "Inactive"u8);
            ImUtf8.DrawTableColumn(ImGui.IsKeyDown(imguiKey) ? "Active"u8 : "Inactive"u8);
        }
    }

    public void Dispose()
        => _framework.Update -= OnUpdate;
}
