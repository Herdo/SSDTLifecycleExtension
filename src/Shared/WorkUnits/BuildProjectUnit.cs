namespace SSDTLifecycleExtension.Shared.WorkUnits;

[UsedImplicitly]
public class BuildProjectUnit : IWorkUnit<ScaffoldingStateModel>,
    IWorkUnit<ScriptCreationStateModel>
{
    [NotNull] private readonly IBuildService _buildService;

    public BuildProjectUnit([NotNull] IBuildService buildService)
    {
        _buildService = buildService ?? throw new ArgumentNullException(nameof(buildService));
    }

    private async Task TryBuildInternal(IStateModel stateModel,
                                        SqlProject project,
                                        bool doBuild)
    {
        if (doBuild && !await _buildService.BuildProjectAsync(project))
            stateModel.Result = false;
        stateModel.CurrentState = StateModelState.TriedToBuildProject;
    }

    Task IWorkUnit<ScaffoldingStateModel>.Work(ScaffoldingStateModel stateModel,
                                               CancellationToken cancellationToken)
    {
        if (stateModel == null)
            throw new ArgumentNullException(nameof(stateModel));

        return TryBuildInternal(stateModel, stateModel.Project, true);
    }

    Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
                                                  CancellationToken cancellationToken)
    {
        if (stateModel == null)
            throw new ArgumentNullException(nameof(stateModel));

        return TryBuildInternal(stateModel, stateModel.Project, stateModel.Configuration.BuildBeforeScriptCreation);
    }
}