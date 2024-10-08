﻿namespace SSDTLifecycleExtension.Shared.WorkUnits;

public class ModifyDeploymentScriptUnit(IScriptModifierProviderService _scriptModifierProviderService,
                                        IFileSystemAccess _fileSystemAccess,
                                        ILogger _logger)
    : IWorkUnit<ScriptCreationStateModel>
{
    private async Task ModifyCreatedScriptInternal(IStateModel stateModel,
        SqlProject project,
        ConfigurationModel configuration,
        PathCollection paths,
        Version previousVersion,
        bool createLatest)
    {
        var modifiers = _scriptModifierProviderService.GetScriptModifiers(configuration);
        if (!modifiers.Any())
        {
            stateModel.CurrentState = StateModelState.ModifiedDeploymentScript;
            return;
        }

        var success = await ApplyAllModifiers(project, configuration, paths, previousVersion, createLatest, modifiers);
        if (!success)
            stateModel.Result = false;
        stateModel.CurrentState = StateModelState.ModifiedDeploymentScript;
    }

    private async Task<bool> ApplyAllModifiers(SqlProject project,
        ConfigurationModel configuration,
        PathCollection paths,
        Version previousVersion,
        bool createLatest,
        IReadOnlyDictionary<ScriptModifier, IScriptModifier> modifiers)
    {
        string initialScript;
        try
        {
            initialScript = await _fileSystemAccess.ReadFileAsync(paths.DeployTargets.DeployScriptPath!);
        }
        catch (Exception e)
        {
            await _logger.LogErrorAsync(e, "Failed to read the generated script");
            return false;
        }

        var model = new ScriptModificationModel(initialScript, project, configuration, paths, previousVersion, createLatest);

        foreach (var m in modifiers.OrderBy(m => m.Key))
        {
            await _logger.LogInfoAsync($"Modifying script: {m.Key}");
            await m.Value.ModifyAsync(model);
        }

        try
        {
            await _fileSystemAccess.WriteFileAsync(paths.DeployTargets.DeployScriptPath!, model.CurrentScript);
        }
        catch (Exception e)
        {
            await _logger.LogErrorAsync(e, "Failed to write the modified script");
            return false;
        }

        return true;
    }

    Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
        CancellationToken cancellationToken)
    {
        Guard.IsNotNullOrWhiteSpace(stateModel.Paths?.DeployTargets.DeployScriptPath);

        return ModifyCreatedScriptInternal(stateModel,
            stateModel.Project,
            stateModel.Configuration,
            stateModel.Paths,
            stateModel.PreviousVersion,
            stateModel.CreateLatest);
    }
}