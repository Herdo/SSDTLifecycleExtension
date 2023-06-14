namespace SSDTLifecycleExtension.Shared.WorkUnits;

[UsedImplicitly]
public class ValidateTargetVersionUnit : IWorkUnit<ScaffoldingStateModel>,
    IWorkUnit<ScriptCreationStateModel>
{
    [NotNull] private readonly IVisualStudioAccess _visualStudioAccess;
    [NotNull] private readonly ILogger _logger;

    public ValidateTargetVersionUnit([NotNull] IVisualStudioAccess visualStudioAccess,
                                     [NotNull] ILogger logger)
    {
        _visualStudioAccess = visualStudioAccess ?? throw new ArgumentNullException(nameof(visualStudioAccess));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private async Task ValidateTargetVersionInternal<TStateModel>(TStateModel stateModel,
                                                                  Func<TStateModel, bool> isValid,
                                                                  Func<TStateModel, string> getLogMessage)
        where TStateModel : IStateModel
    {
        // Check version
        if (isValid(stateModel))
        {
            stateModel.CurrentState = StateModelState.FormattedTargetVersionValidated;
            return;
        }

        stateModel.Result = false;
        stateModel.CurrentState = StateModelState.FormattedTargetVersionValidated;
        await _logger.LogErrorAsync(getLogMessage(stateModel));
        _visualStudioAccess.ShowModalError("Please change the DAC version in the SQL project settings (see output window).");
    }

    Task IWorkUnit<ScaffoldingStateModel>.Work(ScaffoldingStateModel stateModel,
                                               CancellationToken cancellationToken)
    {
        if (stateModel == null)
            throw new ArgumentNullException(nameof(stateModel));

        return ValidateTargetVersionInternal(stateModel,
                                             sm => sm.Project.ProjectProperties.DacVersion == sm.FormattedTargetVersion,
                                             sm =>
                                                 $"DacVersion of SQL project ({sm.Project.ProjectProperties.DacVersion}) doesn't match target version ({sm.FormattedTargetVersion}).");
    }

    Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
                                                  CancellationToken cancellationToken)
    {
        if (stateModel == null)
            throw new ArgumentNullException(nameof(stateModel));

        return ValidateTargetVersionInternal(stateModel,
                                             sm => sm.CreateLatest || sm.FormattedTargetVersion > sm.PreviousVersion,
                                             sm =>
                                                 $"DacVersion of SQL project ({sm.FormattedTargetVersion}) is equal to or smaller than the previous version ({sm.PreviousVersion}).");
    }
}