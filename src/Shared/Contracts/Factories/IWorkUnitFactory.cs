namespace SSDTLifecycleExtension.Shared.Contracts.Factories;

public interface IWorkUnitFactory
{
    IWorkUnit<ScaffoldingStateModel> GetNextWorkUnit([NotNull] ScaffoldingStateModel stateModel);

    IWorkUnit<ScriptCreationStateModel> GetNextWorkUnit([NotNull] ScriptCreationStateModel stateModel);
}