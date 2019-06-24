namespace SSDTLifecycleExtension.Shared.WorkUnits
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Enums;
    using Contracts.Services;
    using JetBrains.Annotations;
    using Models;

    [UsedImplicitly]
    public class LoadPathsUnit : IWorkUnit<ScaffoldingStateModel>,
                                 IWorkUnit<ScriptCreationStateModel>
    {
        [NotNull] private readonly ISqlProjectService _sqlProjectService;

        public LoadPathsUnit([NotNull] ISqlProjectService sqlProjectService)
        {
            _sqlProjectService = sqlProjectService ?? throw new ArgumentNullException(nameof(sqlProjectService));
        }

        async Task IWorkUnit<ScaffoldingStateModel>.Work(ScaffoldingStateModel stateModel,
                                                         CancellationToken cancellationToken)
        {
            stateModel.Paths = await _sqlProjectService.TryLoadPathsForScaffoldingAsync(stateModel.Project, stateModel.Configuration);
            if (stateModel.Paths == null)
                stateModel.Result = false;
            stateModel.CurrentState = StateModelState.PathsLoaded;
        }

        async Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
                                                            CancellationToken cancellationToken)
        {
            stateModel.Paths = await _sqlProjectService.TryLoadPathsForScriptCreationAsync(stateModel.Project, stateModel.Configuration, stateModel.PreviousVersion, stateModel.CreateLatest);
            if (stateModel.Paths == null)
                stateModel.Result = false;
            stateModel.CurrentState = StateModelState.PathsLoaded;
        }
    }
}