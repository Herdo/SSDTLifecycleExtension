namespace SSDTLifecycleExtension.Shared.Services;

public class CommandAvailabilityService(IVisualStudioAccess _visualStudioAccess,
                                        IScaffoldingService _scaffoldingService,
                                        IScriptCreationService _scriptCreationService)
    : ICommandAvailabilityService
{
    void ICommandAvailabilityService.HandleCommandAvailability(Action<bool> setVisible,
        Action<bool> setEnabled)
    {
        var isSqlProject = _visualStudioAccess.IsSelectedProjectOfKindAsync(Constants.SqlProjectKindGuid).Result;
        var visible = isSqlProject;
        var enabled = visible && !_scaffoldingService.IsScaffolding && !_scriptCreationService.IsCreating;

        setVisible(visible);
        setEnabled(enabled);
    }
}