namespace SSDTLifecycleExtension.Shared.Contracts.Factories
{
    using JetBrains.Annotations;
    using Shared.Models;

    public interface IWorkUnitFactory
    {
        IWorkUnit<ScaffoldingStateModel> GetNextWorkUnit([NotNull] ScaffoldingStateModel stateModel);

        IWorkUnit<ScriptCreationStateModel> GetNextWorkUnit([NotNull] ScriptCreationStateModel stateModel);
    }
}