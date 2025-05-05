using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using OtterGui.Log;
using OtterGui.Text;

namespace OtterGui.Services;

public class DynamisIpc : IDisposable
{
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly Logger                  _log;

    private readonly ICallGateSubscriber<uint, uint, ulong, Version, object?> _initialized;
    private readonly ICallGateSubscriber<object?>                             _disposed;

    private uint  _currentVersionMajor;
    private uint  _currentVersionMinor;
    private ulong _featureFlags;

    private ICallGateSubscriber<nint, object?>?                           _inspectObject;
    private ICallGateSubscriber<nint, uint, string, uint, uint, object?>? _inspectRegion;
    private ICallGateSubscriber<nint, object?>?                           _imGuiDrawPointer;
    private Action<nint>?                                                 _drawPointerAction;
    private ICallGateSubscriber<nint, object?>?                           _imGuiDrawPointerTooltipDetails;
    private ICallGateSubscriber<nint, (string, Type?, uint, uint)>?       _getClass;
    private ICallGateSubscriber<nint, string?, Type?, (bool, uint)>?      _isInstanceOf;
    private ICallGateSubscriber<object?>?                                 _preloadDataYaml;

    public bool IsSubscribed
        => _currentVersionMajor > 0;

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
        }
        catch (Exception ex)
        {
            _initialized = null!;
            _disposed    = null!;
            _log.Error($"Error subscribing to Dynamis IPC Events:\n{ex}");
        }
    }


    public void Dispose()
    {
        _currentVersionMajor = 0;
        OnDisposed();
        _initialized.Unsubscribe(OnInitialized);
        _disposed.Unsubscribe(OnDisposed);
    }

    public void OnDisposed()
    {
        if (_currentVersionMajor > 0)
            _log.Debug($"Detaching from Dynamis {_currentVersionMajor}.{_currentVersionMinor}.");
        _currentVersionMajor = 0;
        _currentVersionMinor = 0;
        _featureFlags        = 0;

        _inspectObject    = null;
        _inspectRegion    = null;
        _imGuiDrawPointer = null;
        _getClass         = null;
        _isInstanceOf     = null;
        _preloadDataYaml  = null;
    }

    public void InspectObject(nint address)
        => _inspectObject?.InvokeAction(address);

    public void InspectRegion(nint address, uint unk, string unk2, uint unk3, uint unk4)
        => _inspectRegion?.InvokeAction(address, unk, unk2, unk3, unk4);

    public void DrawTooltipDetails(nint address)
        => _imGuiDrawPointerTooltipDetails?.InvokeAction(address);

    public (string Name, Type? Type, uint Unk, uint Unk2) GetClass(nint address)
        => _getClass?.InvokeFunc(address) ?? ("Unavailable", null, 0, 0);

    public (bool IsInstance, uint Unk) IsInstanceOf(nint address, string? className, Type? classType)
        => _isInstanceOf?.InvokeFunc(address, className, classType) ?? (false, 0);

    public void PreloadDataYaml()
        => _preloadDataYaml?.InvokeAction();

    public void DrawPointer(nint address)
    {
        if (_drawPointerAction is not null)
        {
            _drawPointerAction.Invoke(address);
        }
        else
        {
            if (address == nint.Zero)
            {
                ImUtf8.CopyOnClickSelectable("nullptr"u8, "0x0"u8);
            }
            else
            {
                using var font = ImRaii.PushFont(UiBuilder.MonoFont);
                ImUtf8.CopyOnClickSelectable($"0x{address:X}");
            }
        }
    }

    private void OnInitialized(uint major, uint minor, ulong flags, Version _)
    {
        OnDisposed();
        if (_currentVersionMajor is not 1)
        {
            _log.Debug($"Could not attach to Dynamis {_currentVersionMajor}.{_currentVersionMinor}, only 1.X is supported.");
            return;
        }

        if (_currentVersionMinor < 3)
        {
            _log.Debug($"Could not attach to Dynamis {_currentVersionMajor}.{_currentVersionMinor}, only 1.3 or higher is supported.");
            return;
        }

        _currentVersionMajor = major;
        _currentVersionMinor = minor;
        _featureFlags        = flags;

        try
        {
            _inspectObject = _pluginInterface.GetIpcSubscriber<nint, object?>("Dynamis.InspectObject.V1");
            _inspectRegion = _pluginInterface.GetIpcSubscriber<nint, uint, string, uint, uint, object?>("Dynamis.InspectRegion.V1");
            _imGuiDrawPointer = _pluginInterface.GetIpcSubscriber<nint, object?>("Dynamis.ImGuiDrawPointer.V1");
            _imGuiDrawPointerTooltipDetails = _pluginInterface.GetIpcSubscriber<nint, object?>("Dynamis.ImGuiDrawPointerTooltipDetails.V1");
            _getClass = _pluginInterface.GetIpcSubscriber<nint, (string, Type?, uint, uint)>("Dynamis.GetClass.V1");
            _isInstanceOf = _pluginInterface.GetIpcSubscriber<nint, string?, Type?, (bool, uint)>("Dynamis.IsInstanceOf.V1");
            _preloadDataYaml = _pluginInterface.GetIpcSubscriber<object?>("Dynamis.PreloadDataYaml.V1");
            _drawPointerAction = _pluginInterface.GetIpcSubscriber<Action<nint>>("Dynamis.GetImGuiDrawPointerDelegate.V1").InvokeFunc();
            _log.Debug($"Attached to Dynamis {_currentVersionMajor}.{_currentVersionMinor}.");
        }
        catch (Exception ex)
        {
            _log.Error($"Error subscribing to Dynamis IPC:\n{ex}");
            OnDisposed();
        }
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
            _log.Verbose($"Error subscribing to Dynamis IPC:\n{ex}");
            OnDisposed();
        }
    }
}
