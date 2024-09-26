namespace SSDTLifecycleExtension.Shared.Models;

public abstract class BaseStateModel : BaseModel, IStateModel
{
    protected BaseStateModel(Func<bool, Task> handleWorkInProgressChanged)
    {
        HandleWorkInProgressChanged = handleWorkInProgressChanged;
        CurrentState = StateModelState.Initialized;
    }

    public Func<bool, Task> HandleWorkInProgressChanged { get; }

    public StateModelState CurrentState { get; set; }

    public bool? Result { get; set; }
}