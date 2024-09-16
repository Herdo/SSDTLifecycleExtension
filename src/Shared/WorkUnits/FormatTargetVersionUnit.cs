namespace SSDTLifecycleExtension.Shared.WorkUnits;

public class FormatTargetVersionUnit(IVersionService _versionService)
    : IWorkUnit<ScaffoldingStateModel>,
    IWorkUnit<ScriptCreationStateModel>
{
    Task IWorkUnit<ScaffoldingStateModel>.Work(ScaffoldingStateModel stateModel,
        CancellationToken cancellationToken)
    {
        stateModel.FormattedTargetVersion = Version.Parse(_versionService.FormatVersion(stateModel.TargetVersion, stateModel.Configuration));
        stateModel.CurrentState = StateModelState.FormattedTargetVersionLoaded;
        return Task.CompletedTask;
    }

    Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
        CancellationToken cancellationToken)
    {
        if (!stateModel.CreateLatest)
        {
            Guard.IsNotNull(stateModel.Project.ProjectProperties.DacVersion);
            stateModel.FormattedTargetVersion =
                Version.Parse(_versionService.FormatVersion(stateModel.Project.ProjectProperties.DacVersion, stateModel.Configuration));
        }
        stateModel.CurrentState = StateModelState.FormattedTargetVersionLoaded;
        return Task.CompletedTask;
    }
}