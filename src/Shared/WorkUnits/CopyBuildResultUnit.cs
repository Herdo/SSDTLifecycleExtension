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
    public class CopyBuildResultUnit : IWorkUnit<ScaffoldingStateModel>,
                                       IWorkUnit<ScriptCreationStateModel>
    {
        [NotNull] private readonly IBuildService _buildService;

        public CopyBuildResultUnit([NotNull] IBuildService buildService)
        {
            _buildService = buildService ?? throw new ArgumentNullException(nameof(buildService));
        }

        private async Task TryCopyInternal(IStateModel stateModel,
                                           SqlProject project,
                                           string newDacpacDirectory)
        {
            if (!await _buildService.CopyBuildResultAsync(project, newDacpacDirectory))
                stateModel.Result = false;
        }

        async Task IWorkUnit<ScaffoldingStateModel>.Work(ScaffoldingStateModel stateModel,
                                                         CancellationToken cancellationToken)
        {
            await TryCopyInternal(stateModel, stateModel.Project, stateModel.Paths.NewDacpacDirectory);
            stateModel.CurrentState = StateModelState.TriedToCopyBuildResult;
        }

        async Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
                                                            CancellationToken cancellationToken)
        {
            await TryCopyInternal(stateModel, stateModel.Project, stateModel.Paths.NewDacpacDirectory);
            stateModel.CurrentState = StateModelState.TriedToCopyBuildResult;
        }
    }
}