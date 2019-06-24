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
    public class LoadSqlProjectPropertiesUnit : IWorkUnit<ScaffoldingStateModel>,
                                                IWorkUnit<ScriptCreationStateModel>
    {
        [NotNull] private readonly ISqlProjectService _sqlProjectService;

        public LoadSqlProjectPropertiesUnit([NotNull] ISqlProjectService sqlProjectService)
        {
            _sqlProjectService = sqlProjectService ?? throw new ArgumentNullException(nameof(sqlProjectService));
        }

        private async Task TryLoadSqlProjectPropertiesInternal(IStateModel stateModel,
                                                               SqlProject project)
        {
            var loaded = await _sqlProjectService.TryLoadSqlProjectPropertiesAsync(project);
            if (!loaded)
                stateModel.Result = false;
        }

        async Task IWorkUnit<ScaffoldingStateModel>.Work(ScaffoldingStateModel stateModel,
                                                         CancellationToken cancellationToken)
        {
            await TryLoadSqlProjectPropertiesInternal(stateModel, stateModel.Project);
            stateModel.CurrentState = StateModelState.SqlProjectPropertiesLoaded;
        }

        async Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
                                                            CancellationToken cancellationToken)
        {
            await TryLoadSqlProjectPropertiesInternal(stateModel, stateModel.Project);
            stateModel.CurrentState = StateModelState.SqlProjectPropertiesLoaded;
        }
    }
}