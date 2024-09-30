namespace SSDTLifecycleExtension.Shared.Services;

public class ScaffoldingService(IWorkUnitFactory _workUnitFactory,
                                IVisualStudioAccess _visualStudioAccess,
                                ILogger logger)
    : AsyncExecutorBase<ScaffoldingStateModel>(logger),
    IScaffoldingService
{
    private bool _isScaffolding;

    private async Task<bool> ScaffoldInternalAsync(SqlProject project,
                                                   ConfigurationModel configuration,
                                                   Version targetVersion,
                                                   CancellationToken cancellationToken)
    {
        var stateModel = new ScaffoldingStateModel(project, configuration, targetVersion, StateModelHandleWorkInProgressChanged);
        await DoWorkAsync(stateModel, cancellationToken);
        return stateModel.Result ?? false;
    }

    protected override string GetOperationStartedMessage()
    {
        return "Initializing scaffolding ...";
    }

    protected override string GetOperationCompletedMessage(ScaffoldingStateModel stateModel, long elapsedMilliseconds)
    {
        return $"========== Scaffolding version {stateModel.FormattedTargetVersion} finished after {elapsedMilliseconds} milliseconds. ==========";
    }

    protected override string GetOperationFailedMessage()
    {
        return "DACPAC scaffolding failed.";
    }

    protected override IWorkUnit<ScaffoldingStateModel>? GetNextWorkUnitForStateModel(ScaffoldingStateModel stateModel)
    {
        return _workUnitFactory.GetNextWorkUnit(stateModel);
    }

    private async Task StateModelHandleWorkInProgressChanged(bool workInProgress)
    {
        IsScaffolding = workInProgress;
        if (IsScaffolding)
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

    public event EventHandler? IsScaffoldingChanged;

    private bool IsScaffolding
    {
        get => _isScaffolding;
        set
        {
            if (value == _isScaffolding)
                return;
            _isScaffolding = value;
            IsScaffoldingChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    bool IScaffoldingService.IsScaffolding => IsScaffolding;

    Task<bool> IScaffoldingService.ScaffoldAsync(SqlProject project,
        ConfigurationModel configuration,
        Version targetVersion,
        CancellationToken cancellationToken)
    {
        if (IsScaffolding)
            throw new InvalidOperationException($"Service is already running a {nameof(IScriptCreationService.CreateAsync)} task.");

        return ScaffoldInternalAsync(project, configuration, targetVersion, cancellationToken);
    }
}