using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OtterGui.Log;

namespace OtterGui.Tasks;

public class AsyncTaskManager : IDisposable
{
    private record ActionWrap(Guid Id, IAction Action);

    private readonly ConcurrentDictionary<Guid, Task?> _actions = new();
    private readonly ConcurrentQueue<ActionWrap>       _queue   = new();

    protected readonly Logger                   Logger;
    protected          CancellationTokenSource? Cancellation;
    protected          Task?                    CurrentTask;

    private ActionWrap? _currentAction;

    public event Action<Guid, ActionState, Exception?>? Finished;

    protected AsyncTaskManager(Logger logger)
        => Logger = logger;

    public bool Disposed { get; private set; }

    public int QueuedCount
        => _queue.Count;

    public int DataCount
        => _actions.Count;

    public bool IsRunning
        => CurrentTask is { IsCompleted: false };

    public IAction? CurrentAction
        => _currentAction?.Action;

    public Guid CurrentId
        => _currentAction?.Id ?? Guid.Empty;

    public void Cancel(Guid id)
    {
        if (Disposed)
            return;

        if (_currentAction?.Id == id)
        {
            Task? task;
            lock (this)
            {
                task = CurrentTask;
            }
            Logger.Debug($"[{GetType().Name}] Cancellation of current task {id} triggered...");
            if (task is { IsCompleted: false })
            {
                Cancellation!.Cancel();
                task.ContinueWith(_ => Invoke(id, ActionState.Cancelled, null));
            }
        }
        else
        {
            if (_actions.TryUpdate(id, Task.FromCanceled(CancellationToken.None), null))
            {
                Logger.Debug($"[{GetType().Name}] Cancellation of not-yet-started task {id} requested.");
                Invoke(id, ActionState.Cancelled, null);
            }
        }
    }

    public ActionState GetState(Guid id, out Exception? exception, out int progress, out int maxProgress)
    {
        exception = null;
        if (!_actions.TryGetValue(id, out var task))
        {
            lock (this)
            {
                if (CurrentTask == null || _currentAction == null || _currentAction.Id != id)
                {
                    progress    = 0;
                    maxProgress = int.MaxValue;
                    return ActionState.NotQueued;
                }

                task = CurrentTask;
            }
        }

        var ret = StateFromTask(task, out exception);
        (progress, maxProgress) = ret switch
        {
            ActionState.NotStarted => (0, 1),
            ActionState.Cancelled  => (1, 1),
            ActionState.Running    => (_currentAction?.Action.Progress ?? 0, _currentAction?.Action.MaxProgress ?? 1),
            ActionState.Failed     => (1, 1),
            ActionState.Succeeded  => (1, 1),
            _                      => (0, int.MaxValue),
        };
        return ret;
    }

    public void Dispose()
    {
        Disposed = true;
        lock (this)
        {
            _actions.Clear();
            _queue.Clear();
            Cancellation?.Cancel();
            Cancellation   = null;
            CurrentTask    = null;
            _currentAction = null;
        }
    }

    protected Guid Enqueue(IAction action)
    {
        if (Disposed)
            throw new ObjectDisposedException("Task manager was exposed.");

        var guid = Guid.NewGuid();
        // Ensure uniqueness of GUID.
        while (_actions.ContainsKey(guid))
            guid = Guid.NewGuid();

        var wrap = new ActionWrap(guid, action);

        if (_queue.IsEmpty)
            lock (this)
            {
                if (CurrentTask == null || CurrentTask.IsCompleted)
                {
                    ActivateAction(wrap);
                    return _currentAction!.Id;
                }
            }

        // Ensure no duplicated queued actions.
        var oldWrap = wrap.Action.Equals(_currentAction?.Action) ? _currentAction : _queue.FirstOrDefault(w => w.Action.Equals(wrap.Action));
        if (oldWrap != null)
        {
            Logger.Debug($"[{GetType().Name}] Queued identical task, skipped in favor of {oldWrap.Id}.");
            return oldWrap.Id;
        }

        Logger.Debug($"[{GetType().Name}] Queued new task {wrap.Id}.");
        _queue.Enqueue(wrap);
        _actions.TryAdd(wrap.Id, null);

        return wrap.Id;
    }

    private void ActivateAction(ActionWrap wrap)
    {
        if (Disposed)
            throw new ObjectDisposedException("Task manager was exposed.");

        CurrentTask = _actions.AddOrUpdate(wrap.Id, _ => Activate(wrap), (_, oldTask) =>
        {
            if (oldTask == null)
                return Activate(wrap);

            Cancellation   = null;
            _currentAction = null;
            return oldTask;
        });

        if (CurrentTask == null || CurrentTask.IsCompleted)
        {
            CheckActions();
        }
        else
        {
            CurrentTask.ContinueWith(CheckActions);
            CurrentTask.ContinueWith(t => Invoke(wrap.Id, StateFromTask(t, out var e), e));
        }
    }

    private Task Activate(ActionWrap wrap)
    {
        Logger.Debug($"[{GetType().Name}] Starting task {wrap.Id}...");
        Cancellation   = new CancellationTokenSource();
        _currentAction = wrap;
        return Task.Run(() => _currentAction.Action.Execute(Cancellation.Token), Cancellation.Token);
    }

    private void Invoke(Guid id, ActionState state, Exception? ex)
    {
        Logger.Debug($"[{GetType().Name}] Finished Task {id} with state {state}{(ex == null ? "." : $". [Exception: {ex}]")}");
        Finished?.Invoke(id, state, ex);
    }

    private void CheckActions(Task? task = null)
    {
        if (Disposed || CurrentTask is { IsCompleted: false } || !_queue.TryDequeue(out var action))
            return;

        lock (this)
        {
            ActivateAction(action);
        }
    }

    private static ActionState StateFromTask(Task? task, out Exception? exception)
    {
        if (task == null)
        {
            exception = null;
            return ActionState.NotStarted;
        }

        if (task.IsCompletedSuccessfully)
        {
            exception = null;
            return ActionState.Succeeded;
        }

        if (task.IsCanceled)
        {
            exception = null;
            return ActionState.Cancelled;
        }

        if (task.IsCompleted)
        {
            exception = task.Exception;
            return ActionState.Failed;
        }

        exception = null;
        return ActionState.Running;
    }
}
