namespace SSDTLifecycleExtension.Shared.WorkUnits;

public class WorkUnitFactory(IDependencyResolver _dependencyResolver)
    : IWorkUnitFactory
{
    IWorkUnit<ScaffoldingStateModel>? IWorkUnitFactory.GetNextWorkUnit(ScaffoldingStateModel stateModel)
    {
        return stateModel.CurrentState switch
        {
            StateModelState.Initialized => _dependencyResolver.Get<LoadSqlProjectPropertiesUnit>(),
            StateModelState.SqlProjectPropertiesLoaded => _dependencyResolver.Get<FormatTargetVersionUnit>(),
            StateModelState.FormattedTargetVersionLoaded => _dependencyResolver.Get<ValidateTargetVersionUnit>(),
            StateModelState.FormattedTargetVersionValidated => _dependencyResolver.Get<LoadPathsUnit>(),
            StateModelState.PathsLoaded => _dependencyResolver.Get<BuildProjectUnit>(),
            StateModelState.TriedToBuildProject => _dependencyResolver.Get<CleanNewArtifactsDirectoryUnit>(),
            StateModelState.TriedToCleanArtifactsDirectory => _dependencyResolver.Get<CopyBuildResultUnit>(),
            StateModelState.TriedToCopyBuildResult => _dependencyResolver.Get<CopyDacpacToSharedDacpacRepositoryUnit>(),
            StateModelState.TriedToCopyDacpacToSharedDacpacRepository => null,
            _ => throw new ArgumentOutOfRangeException(nameof(stateModel) + '.' + nameof(IStateModel.CurrentState)),
        };
    }

    IWorkUnit<ScriptCreationStateModel>? IWorkUnitFactory.GetNextWorkUnit(ScriptCreationStateModel stateModel)
    {
        return stateModel.CurrentState switch
        {
            StateModelState.Initialized => _dependencyResolver.Get<LoadSqlProjectPropertiesUnit>(),
            StateModelState.SqlProjectPropertiesLoaded => _dependencyResolver.Get<FormatTargetVersionUnit>(),
            StateModelState.FormattedTargetVersionLoaded => _dependencyResolver.Get<ValidateTargetVersionUnit>(),
            StateModelState.FormattedTargetVersionValidated => _dependencyResolver.Get<LoadPathsUnit>(),
            StateModelState.PathsLoaded => _dependencyResolver.Get<VerifyPathsUnit>(),
            StateModelState.PathsVerified => _dependencyResolver.Get<BuildProjectUnit>(),
            StateModelState.TriedToBuildProject => _dependencyResolver.Get<CleanNewArtifactsDirectoryUnit>(),
            StateModelState.TriedToCleanArtifactsDirectory => _dependencyResolver.Get<CopyBuildResultUnit>(),
            StateModelState.TriedToCopyBuildResult => _dependencyResolver.Get<CopyDacpacToSharedDacpacRepositoryUnit>(),
            StateModelState.TriedToCopyDacpacToSharedDacpacRepository => _dependencyResolver.Get<CreateDeploymentFilesUnit>(),
            StateModelState.TriedToCreateDeploymentFiles => _dependencyResolver.Get<ModifyDeploymentScriptUnit>(),
            StateModelState.ModifiedDeploymentScript => stateModel.CreateLatest
                ? null
                : _dependencyResolver.Get<DeleteRefactorLogUnit>(),
            StateModelState.DeletedRefactorLog => stateModel.CreateLatest
                ? null
                : _dependencyResolver.Get<CleanLatestArtifactsDirectoryUnit>(),
            StateModelState.DeletedLatestArtifacts => null,
            _ => throw new ArgumentOutOfRangeException(nameof(stateModel) + '.' + nameof(IStateModel.CurrentState)),
        };
    }
}