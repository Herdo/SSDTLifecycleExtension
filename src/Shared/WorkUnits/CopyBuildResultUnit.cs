namespace SSDTLifecycleExtension.Shared.WorkUnits;

public class CopyBuildResultUnit(IBuildService _buildService)
    : IWorkUnit<ScaffoldingStateModel>,
    IWorkUnit<ScriptCreationStateModel>
{
    private async Task TryCopyInternal(IStateModel stateModel,
        SqlProject project,
        string newArtifactsDirectory)
    {
        if (!await _buildService.CopyBuildResultAsync(project, newArtifactsDirectory))
            stateModel.Result = false;
        stateModel.CurrentState = StateModelState.TriedToCopyBuildResult;
    }

    Task IWorkUnit<ScaffoldingStateModel>.Work(ScaffoldingStateModel stateModel,
        CancellationToken cancellationToken)
    {
        Guard.IsNotNullOrWhiteSpace(stateModel.Paths?.Directories.NewArtifactsDirectory);

        return TryCopyInternal(stateModel,
            stateModel.Project,
            stateModel.Paths.Directories.NewArtifactsDirectory);
    }

    Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
        CancellationToken cancellationToken)
    {
        Guard.IsNotNullOrWhiteSpace(stateModel.Paths?.Directories.NewArtifactsDirectory);

        return TryCopyInternal(stateModel,
            stateModel.Project,
            stateModel.Paths.Directories.NewArtifactsDirectory);
    }
}