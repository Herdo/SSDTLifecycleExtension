namespace SSDTLifecycleExtension.Shared.WorkUnits
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.DataAccess;
    using Contracts.Enums;
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

        async Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
                                                            CancellationToken cancellationToken)
        {
            await _logger.LogAsync("Verifying paths ...");
            if (_fileSystemAccess.CheckIfFileExists(stateModel.Paths.PublishProfilePath))
            {
                stateModel.CurrentState = StateModelState.PathsVerified;
                return;
            }

            stateModel.Result = false;
            stateModel.CurrentState = StateModelState.PathsVerified;
            await _logger.LogAsync("ERROR: Failed to find publish profile.");
        }
    }
}