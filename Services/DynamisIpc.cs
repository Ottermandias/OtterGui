using Dalamud.Interface;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using ImGuiNET;
using OtterGui.Log;
using OtterGui.Text;

namespace OtterGui.Services;

public class DynamisIpc : IDisposable
{
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly Logger                  _log;

    private readonly ICallGateSubscriber<uint, uint, ulong, Version, object?> _initialized;
    private readonly ICallGateSubscriber<object?>                             _disposed;

    private ICallGateSubscriber<nint, object?>?                           _inspectObject;
    private ICallGateSubscriber<nint, uint, string, uint, uint, object?>? _inspectRegion;
    private Action<nint>?                                                 _drawPointerAction;
    private ICallGateSubscriber<nint, object?>?                           _imGuiDrawPointerTooltipDetails;
    private ICallGateSubscriber<nint, (string, Type?, uint, uint)>?       _getClass;
    private ICallGateSubscriber<nint, string?, Type?, (bool, uint)>?      _isInstanceOf;
    private ICallGateSubscriber<object?>?                                 _preloadDataYaml;

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

    public void InspectObject(nint address)
        => _inspectObject?.InvokeAction(address);

    public void InspectRegion(nint address, uint size, string typeName, uint typeTemplateId, uint classKindId)
        => _inspectRegion?.InvokeAction(address, size, typeName, typeTemplateId, classKindId);

    public void DrawTooltipDetails(nint address)
        => _imGuiDrawPointerTooltipDetails?.InvokeAction(address);

    public (string Name, Type? BestManagedType, uint EstimatedSize, uint Displacement) GetClass(nint address)
        => _getClass?.InvokeFunc(address) ?? ("Unavailable", null, 0, 0);

    public (bool IsInstance, uint Displacement) IsInstanceOf(nint address, string? className, Type? classType)
        => _isInstanceOf?.InvokeFunc(address, className, classType) ?? (false, 0);

    public void DrawPointer(nint address)
    {
        if (_drawPointerAction is not null)
        {
            _drawPointerAction.Invoke(address);
        }
        else
        {
            if (address == nint.Zero)
                ImUtf8.CopyOnClickSelectable("nullptr"u8, "0x0"u8);
            else
                ImUtf8.CopyOnClickSelectable($"0x{address:X}", UiBuilder.MonoFont);
        }
    }

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

        if (minor < 3)
        {
            _log.Debug($"Could not attach to Dynamis {VersionMajor}.{VersionMinor}, only 1.3 or higher is supported.");
            return;
        }

        VersionMajor = major;
        VersionMinor = minor;
        Features     = flags;

        try
        {
            _inspectObject = _pluginInterface.GetIpcSubscriber<nint, object?>("Dynamis.InspectObject.V1");
            _inspectRegion = _pluginInterface.GetIpcSubscriber<nint, uint, string, uint, uint, object?>("Dynamis.InspectRegion.V1");
            _imGuiDrawPointerTooltipDetails = _pluginInterface.GetIpcSubscriber<nint, object?>("Dynamis.ImGuiDrawPointerTooltipDetails.V1");
            _getClass = _pluginInterface.GetIpcSubscriber<nint, (string, Type?, uint, uint)>("Dynamis.GetClass.V1");
            _isInstanceOf = _pluginInterface.GetIpcSubscriber<nint, string?, Type?, (bool, uint)>("Dynamis.IsInstanceOf.V1");
            _preloadDataYaml = _pluginInterface.GetIpcSubscriber<object?>("Dynamis.PreloadDataYaml.V1");
            _drawPointerAction = _pluginInterface.GetIpcSubscriber<Action<nint>>("Dynamis.GetImGuiDrawPointerDelegate.V1").InvokeFunc();
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
}
