using Dalamud.Interface;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using OtterGui.Log;
using OtterGui.Text;

namespace OtterGui.Services;

public class DynamisIpc : IDisposable, IService
{
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly Logger                  _log;

    private readonly ICallGateSubscriber<uint, uint, ulong, Version, object?> _initialized;
    private readonly ICallGateSubscriber<object?>                             _disposed;

    private ICallGateSubscriber<nint, string?, object?>?                           _inspectObject;
    private ICallGateSubscriber<nint, uint, string, uint, uint, string?, object?>? _inspectRegion;
    private Action<nint, Func<string?>?, string?, ulong, Vector2>?                 _drawPointerAction;
    private ICallGateSubscriber<nint, object?>?                                    _imGuiDrawPointerTooltipDetails;
    private ICallGateSubscriber<nint, Func<string?>?, object?>?                    _imGuiOpenPointerContextMenu;
    private ICallGateSubscriber<nint, (string, Type?, uint, uint)>?                _getClass;
    private ICallGateSubscriber<nint, string?, Type?, (bool, uint)>?               _isInstanceOf;
    private ICallGateSubscriber<object?>?                                          _preloadDataYaml;

    public bool IsSubscribed
        => VersionMajor > 0;

    public ulong Features { get; private set; }

    public uint VersionMajor { get; private set; }

    public uint VersionMinor { get; private set; }

    public Exception? Error { get; private set; }

    public DynamisIpc(IDalamudPluginInterface pi, Logger log)
    {
        _pluginInterface = pi;
        _log             = log;

        try
        {
            _initialized = _pluginInterface.GetIpcSubscriber<uint, uint, ulong, Version, object?>("Dynamis.ApiInitialized");
            _initialized.Subscribe(OnInitialized);
            _disposed = _pluginInterface.GetIpcSubscriber<object?>("Dynamis.ApiDisposing");
            _disposed.Subscribe(OnDisposed);
            UpdateVersion();
            Error = null;
        }
        catch (Exception ex)
        {
            _initialized = null!;
            _disposed    = null!;
            Error        = ex;
            _log.Error($"Error subscribing to Dynamis IPC Events:\n{ex}");
        }
    }


    public void Dispose()
    {
        Error        = null;
        VersionMajor = 0;
        OnDisposed();
        _initialized.Unsubscribe(OnInitialized);
        _disposed.Unsubscribe(OnDisposed);
    }

    public void InspectObject(nint address, string? name = null)
        => _inspectObject?.InvokeAction(address, name);

    public void InspectRegion(nint address, uint size, string typeName, uint typeTemplateId, uint classKindId, string? name = null)
        => _inspectRegion?.InvokeAction(address, size, typeName, typeTemplateId, classKindId, name);

    public void DrawTooltipDetails(nint address)
        => _imGuiDrawPointerTooltipDetails?.InvokeAction(address);

    public void OpenContextMenu(nint address, Func<string?>? name = null)
        => _imGuiOpenPointerContextMenu?.InvokeAction(address, name);

    public (string Name, Type? BestManagedType, uint EstimatedSize, uint Displacement) GetClass(nint address)
        => _getClass?.InvokeFunc(address) ?? ("Unavailable", null, 0, 0);

    public (bool IsInstance, uint Displacement) IsInstanceOf(nint address, string? className, Type? classType)
        => _isInstanceOf?.InvokeFunc(address, className, classType) ?? (false, 0);

    public void DrawPointer(nint address, Func<string?>? name = null, string? customText = null, DrawPointerFlags flags = DrawPointerFlags.None,
        ImGuiSelectableFlags selectableFlags = ImGuiSelectableFlags.None, Vector2 size = default)
    {
        if (_drawPointerAction is not null)
        {
            _drawPointerAction.Invoke(address, name, customText, unchecked((uint)selectableFlags | ((ulong)flags << 32)), size);
        }
        else
        {
            using (ImRaii.PushFont(UiBuilder.MonoFont,
                       customText is null ? address != nint.Zero : flags.HasFlag(DrawPointerFlags.MonoFont)))
            {
                using var style = ImRaii.PushStyle(ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f,
                    customText is null ? address == nint.Zero : flags.HasFlag(DrawPointerFlags.Semitransparent));
                style.Push(ImGuiStyleVar.SelectableTextAlign, new Vector2(1.0f, 0.5f),
                    size != default || flags.HasFlag(DrawPointerFlags.RightAligned));
                if (ImUtf8.Selectable(customText ?? (address == nint.Zero ? "nullptr" : $"0x{address:X}"),
                        flags.HasFlag(DrawPointerFlags.Selected), selectableFlags, size))
                {
                    try
                    {
                        ImUtf8.SetClipboardText($"0x{address:X}");
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            ImUtf8.HoverTooltip("Click to copy to clipboard."u8);
        }
    }

    public unsafe void DrawPointer(void* address, Func<string?>? name = null, string? customText = null,
        DrawPointerFlags flags = DrawPointerFlags.None, ImGuiSelectableFlags selectableFlags = ImGuiSelectableFlags.None,
        Vector2 size = default)
        => DrawPointer((nint)address, name, customText, flags, selectableFlags, size);

    public void DrawDebugInfo()
    {
        using var table = ImUtf8.Table("##Dynamis"u8, 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg);
        if (!table)
            return;

        ImUtf8.DrawTableColumn("Available"u8);
        ImUtf8.DrawTableColumn($"{IsSubscribed}");
        if (IsSubscribed)
        {
            ImUtf8.DrawTableColumn("Version"u8);
            ImUtf8.DrawTableColumn($"{VersionMajor}.{VersionMinor}");
            ImUtf8.DrawTableColumn("Features"u8);
            ImUtf8.DrawTableColumn($"{Features:X4}");
            ImUtf8.DrawTableColumn("Detach"u8);
            ImGui.TableNextColumn();
            if (ImUtf8.SmallButton("Try##Detach"u8))
                OnDisposed();

            ImUtf8.DrawTableColumn("Reattach"u8);
            ImGui.TableNextColumn();
            if (ImUtf8.SmallButton("Try##Reattach"u8))
                UpdateVersion();
        }
        else
        {
            ImUtf8.DrawTableColumn("Error"u8);
            ImUtf8.DrawTableColumn($"{Error?.Message}");
            ImUtf8.DrawTableColumn("Attach"u8);
            ImGui.TableNextColumn();
            if (ImUtf8.SmallButton("Try##Attach"u8))
                UpdateVersion();
        }
    }

    private void OnInitialized(uint major, uint minor, ulong flags, Version _)
    {
        OnDisposed();
        if (major is not 1)
        {
            _log.Debug($"Could not attach to Dynamis {VersionMajor}.{VersionMinor}, only 1.X is supported.");
            return;
        }

        if (minor < 6)
        {
            _log.Debug($"Could not attach to Dynamis {VersionMajor}.{VersionMinor}, only 1.6 or higher is supported.");
            return;
        }

        VersionMajor = major;
        VersionMinor = minor;
        Features     = flags;

        try
        {
            _inspectObject = _pluginInterface.GetIpcSubscriber<nint, string?, object?>("Dynamis.InspectObject.V2");
            _inspectRegion = _pluginInterface.GetIpcSubscriber<nint, uint, string, uint, uint, string?, object?>("Dynamis.InspectRegion.V2");
            _imGuiDrawPointerTooltipDetails = _pluginInterface.GetIpcSubscriber<nint, object?>("Dynamis.ImGuiDrawPointerTooltipDetails.V1");
            _imGuiOpenPointerContextMenu = _pluginInterface.GetIpcSubscriber<nint, Func<string?>?, object?>("Dynamis.ImGuiOpenPointerContextMenu.V1");
            _getClass = _pluginInterface.GetIpcSubscriber<nint, (string, Type?, uint, uint)>("Dynamis.GetClass.V1");
            _isInstanceOf = _pluginInterface.GetIpcSubscriber<nint, string?, Type?, (bool, uint)>("Dynamis.IsInstanceOf.V1");
            _preloadDataYaml = _pluginInterface.GetIpcSubscriber<object?>("Dynamis.PreloadDataYaml.V1");
            _drawPointerAction = _pluginInterface
                .GetIpcSubscriber<Action<nint, Func<string?>?, string?, ulong, Vector2>>("Dynamis.GetImGuiDrawPointerDelegate.V3").InvokeFunc();
            _log.Verbose("Preloading Dynamis data.yml...");
            _preloadDataYaml.InvokeAction();
            _log.Debug($"Attached to Dynamis {VersionMajor}.{VersionMinor}.");
        }
        catch (Exception ex)
        {
            Error = ex;
            _log.Error($"Error subscribing to Dynamis IPC:\n{ex}");
            OnDisposed();
        }
    }

    private void OnDisposed()
    {
        if (VersionMajor > 0)
            _log.Debug($"Detaching from Dynamis {VersionMajor}.{VersionMinor}.");
        Error        = null;
        VersionMajor = 0;
        VersionMinor = 0;
        Features     = 0;

        _inspectObject                  = null;
        _inspectRegion                  = null;
        _getClass                       = null;
        _isInstanceOf                   = null;
        _preloadDataYaml                = null;
        _drawPointerAction              = null;
        _imGuiDrawPointerTooltipDetails = null;
        _imGuiOpenPointerContextMenu    = null;
    }

    private void UpdateVersion()
    {
        try
        {
            if (_pluginInterface.GetIpcSubscriber<(uint Major, uint Minor, ulong Flags)>("Dynamis.GetApiVersion") is { } subscriber)
            {
                var (major, minor, flags) = subscriber.InvokeFunc();
                OnInitialized(major, minor, flags, null!);
            }
            else
            {
                OnDisposed();
            }
        }
        catch (Exception ex)
        {
            Error = ex;
            _log.Verbose($"Error subscribing to Dynamis IPC:\n{ex}");
            OnDisposed();
        }
    }

    // See https://github.com/Exter-N/Dynamis/blob/main/Dynamis/UI/ImGuiComponents.cs
    [Flags]
    public enum DrawPointerFlags : uint
    {
        None = 0,

        /// <summary>
        /// Draws the ImGui selectable as selected.
        /// </summary>
        Selected = 1,

        /// <summary>
        /// Draws the supplied custom text in a monospace font.
        /// Applied to the default text if the pointer is not null.
        /// </summary>
        MonoFont = 2,

        /// <summary>
        /// Draws the supplied custom text with halved opacity.
        /// Applied to the default text if the pointer is null.
        /// </summary>
        Semitransparent = 4,

        /// <summary>
        /// Aligns the text to the right horizontally and centers it vertically.
        /// Always applied when passed an explicit size.
        /// </summary>
        RightAligned = 8,
    }
}
