namespace SSDTLifecycleExtension.Shared.WorkUnits;

public class LoadSqlProjectPropertiesUnit(ISqlProjectService _sqlProjectService)
    : IWorkUnit<ScaffoldingStateModel>,
    IWorkUnit<ScriptCreationStateModel>
{
    private async Task TryLoadSqlProjectPropertiesInternal(IStateModel stateModel,
        SqlProject project)
    {
        var loaded = await _sqlProjectService.TryLoadSqlProjectPropertiesAsync(project);
        if (!loaded)
            stateModel.Result = false;
        stateModel.CurrentState = StateModelState.SqlProjectPropertiesLoaded;
    }

    Task IWorkUnit<ScaffoldingStateModel>.Work(ScaffoldingStateModel stateModel,
        CancellationToken cancellationToken)
    {
        return TryLoadSqlProjectPropertiesInternal(stateModel, stateModel.Project);
    }

    Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
        CancellationToken cancellationToken)
    {
        return TryLoadSqlProjectPropertiesInternal(stateModel, stateModel.Project);
    }
}