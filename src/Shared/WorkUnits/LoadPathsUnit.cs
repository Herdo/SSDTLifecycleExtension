namespace SSDTLifecycleExtension.Shared.WorkUnits
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Enums;
    using Contracts.Models;
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

        private static async Task LoadPathsInternal<TStateModel>(TStateModel stateModel,
                                                                 Func<TStateModel, Task> setter,
                                                                 Func<TStateModel, PathCollection> getter)
            where TStateModel : IStateModel
        {
            await setter(stateModel);
            if (getter(stateModel) == null)
                stateModel.Result = false;
            stateModel.CurrentState = StateModelState.PathsLoaded;
        }

        Task IWorkUnit<ScaffoldingStateModel>.Work(ScaffoldingStateModel stateModel,
                                                   CancellationToken cancellationToken)
        {
            if (stateModel == null)
                throw new ArgumentNullException(nameof(stateModel));

            return LoadPathsInternal(stateModel,
                                     async sm => sm.Paths = await _sqlProjectService.TryLoadPathsForScaffoldingAsync(sm.Project, sm.Configuration),
                                     sm => sm.Paths);
        }

        Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
                                                      CancellationToken cancellationToken)
        {
            if (stateModel == null)
                throw new ArgumentNullException(nameof(stateModel));

            return LoadPathsInternal(stateModel,
                                     async sm => sm.Paths = await _sqlProjectService.TryLoadPathsForScriptCreationAsync(stateModel.Project,
                                                                                                                        stateModel.Configuration,
                                                                                                                        stateModel.PreviousVersion,
                                                                                                                        stateModel.CreateLatest),
                                     sm => sm.Paths);
        }
    }
}