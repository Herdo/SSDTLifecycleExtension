namespace SSDTLifecycleExtension.Shared.Contracts.Factories;

public interface IWorkUnitFactory
{
    IWorkUnit<ScaffoldingStateModel>? GetNextWorkUnit(ScaffoldingStateModel stateModel);

    IWorkUnit<ScriptCreationStateModel>? GetNextWorkUnit(ScriptCreationStateModel stateModel);
}