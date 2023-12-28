using Dalamud.Hooking;
using Dalamud.Plugin.Services;

namespace OtterGui.Services;

/// <summary> A utility to asynchronously create hooks, and dispose of them. </summary>
public sealed class HookManager(IGameInteropProvider _provider) : IDisposable, IService
{
    private readonly object                                    _lock  = new();
    private readonly ConcurrentDictionary<string, IDisposable> _hooks = [];
    private          Task?                                     _currentTask;
    private          bool                                      _disposed = false;

    /// <summary> Create a hook for a given address. </summary>
    public Task<Hook<T>> CreateHook<T>(string name, nint address, T detour) where T : Delegate
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HookManager));

        Task<Hook<T>> task;
        lock (_lock)
        {
            // We do not want to actually create hooks from multiple threads,
            // so chain them instead.
            if (_currentTask == null)
                task = Task.Run(() =>
                {
                    var hook = _provider.HookFromAddress(address, detour);
                    AddHook(name, hook);
                    return hook;
                });
            else
                task = _currentTask.ContinueWith(_ =>
                {
                    var hook = _provider.HookFromAddress(address, detour);
                    AddHook(name, hook);
                    return hook;
                });
            _currentTask = task;
        }
        return task;
    }

    /// <summary> Create a hook from a given signature. </summary>
    public Task<Hook<T>> CreateHook<T>(string name, string signature, T detour) where T : Delegate
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HookManager));

        Task<Hook<T>> task;
        lock (_lock)
        {
            if (_currentTask == null)
                task = Task.Run(() =>
                {
                    var hook = _provider.HookFromSignature(signature, detour);
                    AddHook(name, hook);
                    return hook;
                });
            else
                task = _currentTask.ContinueWith(_ =>
                {
                    var hook = _provider.HookFromSignature(signature, detour);
                    AddHook(name, hook);
                    return hook;
                });
            _currentTask = task;
        }
        return task;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        lock (_lock)
        {
            _currentTask?.Wait();
            _disposed = true;
            foreach(var (_, hook) in _hooks)
                hook.Dispose();
            _hooks.Clear();
            _currentTask = null;
        }
    }

    /// <summary> Add the hook and throw on failure. </summary>
    private void AddHook(string name, IDisposable hook)
    {
        if (!_hooks.TryAdd(name, hook))
            throw new Exception($"A hook with the name of {name} already exists.");
    }
}
