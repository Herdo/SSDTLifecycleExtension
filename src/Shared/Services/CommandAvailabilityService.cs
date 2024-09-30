namespace SSDTLifecycleExtension.Shared.Services;

public class CommandAvailabilityService(IVisualStudioAccess _visualStudioAccess,
                                        IScaffoldingService _scaffoldingService,
                                        IScriptCreationService _scriptCreationService)
    : ICommandAvailabilityService
{
    void ICommandAvailabilityService.HandleCommandAvailability(Action<bool> setVisible,
        Action<bool> setEnabled)
    {
        var projectKind = _visualStudioAccess.GetSelectedProjectKind();
        if (projectKind == Guid.Empty)
            return;

        var visible = projectKind == Guid.Parse(Constants.SqlProjectKindGuid);
        var enabled = visible && !_scaffoldingService.IsScaffolding && !_scriptCreationService.IsCreating;

        setVisible(visible);
        setEnabled(enabled);
    }
}