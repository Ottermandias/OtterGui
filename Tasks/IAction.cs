using System;
using System.Threading;
using System.Threading.Tasks;

namespace OtterGui.Tasks;

public enum ActionState
{
    NotQueued,
    NotStarted,
    Cancelled,
    Running,
    Failed,
    Succeeded,
}

public interface IAction : IEquatable<IAction>
{
    public int Progress
        => 0;

    public int MaxProgress
        => 1;

    public void Execute(CancellationToken token);
}
