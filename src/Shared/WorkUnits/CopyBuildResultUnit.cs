namespace SSDTLifecycleExtension.Shared.WorkUnits;

[UsedImplicitly]
public class CopyBuildResultUnit : IWorkUnit<ScaffoldingStateModel>,
    IWorkUnit<ScriptCreationStateModel>
{
    [NotNull] private readonly IBuildService _buildService;

    public CopyBuildResultUnit([NotNull] IBuildService buildService)
    {
        _buildService = buildService ?? throw new ArgumentNullException(nameof(buildService));
    }

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
        if (stateModel == null)
            throw new ArgumentNullException(nameof(stateModel));

        return TryCopyInternal(stateModel, stateModel.Project, stateModel.Paths.Directories.NewArtifactsDirectory);
    }

    Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
                                                  CancellationToken cancellationToken)
    {
        if (stateModel == null)
            throw new ArgumentNullException(nameof(stateModel));

        return TryCopyInternal(stateModel, stateModel.Project, stateModel.Paths.Directories.NewArtifactsDirectory);
    }
}