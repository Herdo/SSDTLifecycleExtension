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
            stateModel.CurrentState = StateModelState.SqlProjectPropertiesLoaded;
        }

        Task IWorkUnit<ScaffoldingStateModel>.Work(ScaffoldingStateModel stateModel,
                                                   CancellationToken cancellationToken)
        {
            if (stateModel == null)
                throw new ArgumentNullException(nameof(stateModel));

            return TryLoadSqlProjectPropertiesInternal(stateModel, stateModel.Project);
        }

        Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
                                                      CancellationToken cancellationToken)
        {
            if (stateModel == null)
                throw new ArgumentNullException(nameof(stateModel));

            return TryLoadSqlProjectPropertiesInternal(stateModel, stateModel.Project);
        }
    }
}