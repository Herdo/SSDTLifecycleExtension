﻿namespace SSDTLifecycleExtension.Shared.WorkUnits;

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
        if (string.IsNullOrWhiteSpace(paths.DeploySources.PublishProfilePath))
        {
            stateModel.Result = false;
            stateModel.CurrentState = StateModelState.PathsVerified;
            await _logger.LogErrorAsync("Failed to find publish profile. "
                                      + $"The {nameof(ConfigurationModel.PublishProfilePath)} is set to \"{ConfigurationModel.UseSinglePublishProfileSpecialKeyword}\", but there's either none, or more than one publish profile in the directory. "
                                      + $"Please read the documentation at {_logger.DocumentationBaseUrl}publish-profile-path for more details.");
            return;
        }

        if (_fileSystemAccess.CheckIfFileExists(paths.DeploySources.PublishProfilePath))
        {
            stateModel.CurrentState = StateModelState.PathsVerified;
            return;
        }

        stateModel.Result = false;
        stateModel.CurrentState = StateModelState.PathsVerified;
        await _logger.LogErrorAsync($"Failed to find publish profile at \"{paths.DeploySources.PublishProfilePath}\". "
                                  + $"Please read the documentation at {_logger.DocumentationBaseUrl}publish-profile-path for more details.");
    }

    Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
                                                  CancellationToken cancellationToken)
    {
        if (stateModel == null)
            throw new ArgumentNullException(nameof(stateModel));

        return VerifyPathsInternal(stateModel, stateModel.Paths);
    }
}