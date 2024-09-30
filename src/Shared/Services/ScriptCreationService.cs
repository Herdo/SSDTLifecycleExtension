namespace SSDTLifecycleExtension.Shared.Services;

public class ScriptCreationService(IWorkUnitFactory _workUnitFactory,
                                   IVisualStudioAccess _visualStudioAccess,
                                   ILogger logger)
    : AsyncExecutorBase<ScriptCreationStateModel>(logger),
    IScriptCreationService
{
    private bool _isCreating;

    private async Task<bool> CreateInternalAsync(SqlProject project,
                                                 ConfigurationModel configuration,
                                                 Version previousVersion,
                                                 bool latest,
                                                 CancellationToken cancellationToken)
    {
        var stateModel = new ScriptCreationStateModel(project, configuration, previousVersion, latest, StateModelHandleWorkInProgressChanged);
        await DoWorkAsync(stateModel, cancellationToken);
        return stateModel.Result ?? false;
    }

    protected override string GetOperationStartedMessage()
    {
        return "Initializing script creation ...";
    }

    protected override string GetOperationCompletedMessage(ScriptCreationStateModel stateModel, long elapsedMilliseconds)
    {
        return $"========== Script creation finished after {elapsedMilliseconds} milliseconds. ==========";
    }

    protected override string GetOperationFailedMessage()
    {
        return "Script creation failed.";
    }

    protected override IWorkUnit<ScriptCreationStateModel>? GetNextWorkUnitForStateModel(ScriptCreationStateModel stateModel)
    {
        return _workUnitFactory.GetNextWorkUnit(stateModel);
    }

    private async Task StateModelHandleWorkInProgressChanged(bool workInProgress)
    {
        IsCreating = workInProgress;
        if (IsCreating)
        {
            await _visualStudioAccess.StartLongRunningTaskIndicatorAsync();
            await _visualStudioAccess.ClearSSDTLifecycleOutputAsync();
        }
        else
        {
            try
            {
                await _visualStudioAccess.StopLongRunningTaskIndicatorAsync();
            }
            catch
            {
                // ignored
            }
        }
    }

    public event EventHandler? IsCreatingChanged;

    private bool IsCreating
    {
        get => _isCreating;
        set
        {
            if (value == _isCreating)
                return;
            _isCreating = value;
            IsCreatingChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    bool IScriptCreationService.IsCreating => IsCreating;

    Task<bool> IScriptCreationService.CreateAsync(SqlProject project,
        ConfigurationModel configuration,
        Version previousVersion,
        bool latest,
        CancellationToken cancellationToken)
    {
        if (IsCreating)
            throw new InvalidOperationException($"Service is already running a {nameof(IScriptCreationService.CreateAsync)} task.");

        return CreateInternalAsync(project, configuration, previousVersion, latest, cancellationToken);
    }
}