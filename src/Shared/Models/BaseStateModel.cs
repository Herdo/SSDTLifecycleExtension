namespace SSDTLifecycleExtension.Shared.Models;

public abstract class BaseStateModel : BaseModel, IStateModel
{
    protected BaseStateModel([NotNull] Func<bool, Task> handleWorkInProgressChanged)
    {
        HandleWorkInProgressChanged = handleWorkInProgressChanged ?? throw new ArgumentNullException(nameof(handleWorkInProgressChanged));
        CurrentState = StateModelState.Initialized;
    }

    public Func<bool, Task> HandleWorkInProgressChanged { get; }

    public StateModelState CurrentState { get; set; }

    public bool? Result { get; set; }
}