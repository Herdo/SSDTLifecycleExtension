namespace SSDTLifecycleExtension.Shared.WorkUnits;

public class LoadPathsUnit(ISqlProjectService _sqlProjectService)
    : IWorkUnit<ScaffoldingStateModel>,
    IWorkUnit<ScriptCreationStateModel>
{
    private static async Task LoadPathsInternal<TStateModel>(TStateModel stateModel,
        Func<TStateModel, Task> setter,
        Func<TStateModel, PathCollection?> getter)
        where TStateModel : IStateModel
    {
        await setter(stateModel);
        if (getter(stateModel) is null)
            stateModel.Result = false;
        stateModel.CurrentState = StateModelState.PathsLoaded;
    }

    Task IWorkUnit<ScaffoldingStateModel>.Work(ScaffoldingStateModel stateModel,
        CancellationToken cancellationToken)
    {
        return LoadPathsInternal(stateModel,
            async sm => sm.Paths = await _sqlProjectService.TryLoadPathsForScaffoldingAsync(sm.Project, sm.Configuration),
            sm => sm.Paths);
    }

    Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
        CancellationToken cancellationToken)
    {
        return LoadPathsInternal(stateModel,
            async sm => sm.Paths = await _sqlProjectService.TryLoadPathsForScriptCreationAsync(stateModel.Project,
                stateModel.Configuration,
                stateModel.PreviousVersion,
                stateModel.CreateLatest),
            sm => sm.Paths);
    }
}