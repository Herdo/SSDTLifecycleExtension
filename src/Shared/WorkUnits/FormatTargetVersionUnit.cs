﻿namespace SSDTLifecycleExtension.Shared.WorkUnits;

[UsedImplicitly]
public class FormatTargetVersionUnit : IWorkUnit<ScaffoldingStateModel>,
    IWorkUnit<ScriptCreationStateModel>
{
    [NotNull] private readonly IVersionService _versionService;

    public FormatTargetVersionUnit([NotNull] IVersionService versionService)
    {
        _versionService = versionService ?? throw new ArgumentNullException(nameof(versionService));
    }

    Task IWorkUnit<ScaffoldingStateModel>.Work(ScaffoldingStateModel stateModel,
                                               CancellationToken cancellationToken)
    {
        if (stateModel == null)
            throw new ArgumentNullException(nameof(stateModel));

        stateModel.FormattedTargetVersion = Version.Parse(_versionService.FormatVersion(stateModel.TargetVersion, stateModel.Configuration));
        stateModel.CurrentState = StateModelState.FormattedTargetVersionLoaded;
        return Task.CompletedTask;
    }

    Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
                                                  CancellationToken cancellationToken)
    {
        if (stateModel == null)
            throw new ArgumentNullException(nameof(stateModel));

        if (!stateModel.CreateLatest)
            stateModel.FormattedTargetVersion =
                Version.Parse(_versionService.FormatVersion(stateModel.Project.ProjectProperties.DacVersion, stateModel.Configuration));
        stateModel.CurrentState = StateModelState.FormattedTargetVersionLoaded;
        return Task.CompletedTask;
    }
}