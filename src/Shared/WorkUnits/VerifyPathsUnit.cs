namespace SSDTLifecycleExtension.Shared.WorkUnits
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.DataAccess;
    using Contracts.Enums;
    using Contracts.Models;
    using JetBrains.Annotations;
    using Models;

    [UsedImplicitly]
    public class VerifyPathsUnit : IWorkUnit<ScriptCreationStateModel>
    {
        [NotNull] private readonly IFileSystemAccess _fileSystemAccess;
        [NotNull] private readonly ILogger _logger;

        public VerifyPathsUnit([NotNull] IFileSystemAccess fileSystemAccess,
                               [NotNull] ILogger logger)
        {
            _fileSystemAccess = fileSystemAccess ?? throw new ArgumentNullException(nameof(fileSystemAccess));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task VerifyPathsInternal(IStateModel stateModel,
                                               PathCollection paths)
        {
            await _logger.LogInfoAsync("Verifying paths ...");
            if (!string.IsNullOrWhiteSpace(paths.DeploySources.PublishProfilePath) && _fileSystemAccess.CheckIfFileExists(paths.DeploySources.PublishProfilePath))
            {
                stateModel.CurrentState = StateModelState.PathsVerified;
                return;
            }

            stateModel.Result = false;
            stateModel.CurrentState = StateModelState.PathsVerified;
            await _logger.LogErrorAsync("Failed to find publish profile.");
        }

        Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
                                                      CancellationToken cancellationToken)
        {
            if (stateModel == null)
                throw new ArgumentNullException(nameof(stateModel));

            return VerifyPathsInternal(stateModel, stateModel.Paths);
        }
    }
}