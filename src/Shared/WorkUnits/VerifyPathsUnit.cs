namespace SSDTLifecycleExtension.Shared.WorkUnits;

public class VerifyPathsUnit(IFileSystemAccess _fileSystemAccess,
                             ILogger _logger)
    : IWorkUnit<ScriptCreationStateModel>
{
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

        if (_fileSystemAccess.CheckIfFileExists(paths.DeploySources.PublishProfilePath!))
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
        Guard.IsNotNull(stateModel.Paths);

        return VerifyPathsInternal(stateModel,
            stateModel.Paths);
    }
}