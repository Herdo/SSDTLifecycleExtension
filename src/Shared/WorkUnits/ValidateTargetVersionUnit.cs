﻿namespace SSDTLifecycleExtension.Shared.WorkUnits;

public class ValidateTargetVersionUnit(IVisualStudioAccess _visualStudioAccess,
                                       ILogger _logger)
    : IWorkUnit<ScaffoldingStateModel>,
    IWorkUnit<ScriptCreationStateModel>
{
    private async Task ValidateTargetVersionInternal<TStateModel>(TStateModel stateModel,
            Func<TStateModel, bool> isValid,
            Func<TStateModel, string> getLogMessage)
        where TStateModel : IStateModel
    {
        // Check version
        if (isValid(stateModel))
        {
            stateModel.CurrentState = StateModelState.FormattedTargetVersionValidated;
            return;
        }

        stateModel.Result = false;
        stateModel.CurrentState = StateModelState.FormattedTargetVersionValidated;
        await _logger.LogErrorAsync(getLogMessage(stateModel));
        await _visualStudioAccess.ShowModalErrorAsync("Please change the DAC version in the SQL project settings (see output window).");
    }

    Task IWorkUnit<ScaffoldingStateModel>.Work(ScaffoldingStateModel stateModel,
        CancellationToken cancellationToken)
    {
        return ValidateTargetVersionInternal(stateModel,
            sm => sm.Project.ProjectProperties.DacVersion == sm.FormattedTargetVersion,
            sm => $"DacVersion of SQL project ({sm.Project.ProjectProperties.DacVersion}) doesn't match target version ({sm.FormattedTargetVersion}).");
    }

    Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
        CancellationToken cancellationToken)
    {
        return ValidateTargetVersionInternal(stateModel,
            sm => sm.CreateLatest || sm.FormattedTargetVersion > sm.PreviousVersion,
            sm => $"DacVersion of SQL project ({sm.FormattedTargetVersion}) is equal to or smaller than the previous version ({sm.PreviousVersion}).");
    }
}