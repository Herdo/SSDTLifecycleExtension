namespace SSDTLifecycleExtension.Shared.WorkUnits;

public class BuildProjectUnit(IBuildService _buildService)
    : IWorkUnit<ScaffoldingStateModel>,
    IWorkUnit<ScriptCreationStateModel>
{
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
        return TryBuildInternal(stateModel, stateModel.Project, true);
    }

    Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
        CancellationToken cancellationToken)
    {
        return TryBuildInternal(stateModel, stateModel.Project, stateModel.Configuration.BuildBeforeScriptCreation);
    }
}