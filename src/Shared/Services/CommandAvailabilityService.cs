namespace SSDTLifecycleExtension.Shared.Services;

[UsedImplicitly]
public class CommandAvailabilityService : ICommandAvailabilityService
{
    private readonly IVisualStudioAccess _visualStudioAccess;
    private readonly IScaffoldingService _scaffoldingService;
    private readonly IScriptCreationService _scriptCreationService;

    public CommandAvailabilityService(IVisualStudioAccess visualStudioAccess,
                                      IScaffoldingService scaffoldingService,
                                      IScriptCreationService scriptCreationService)
    {
        _visualStudioAccess = visualStudioAccess ?? throw new ArgumentNullException(nameof(visualStudioAccess));
        _scaffoldingService = scaffoldingService ?? throw new ArgumentNullException(nameof(scaffoldingService));
        _scriptCreationService = scriptCreationService ?? throw new ArgumentNullException(nameof(scriptCreationService));
    }

    void ICommandAvailabilityService.HandleCommandAvailability(Action<bool> setVisible,
                                                               Action<bool> setEnabled)
    {
        if (setVisible == null)
            throw new ArgumentNullException(nameof(setVisible));
        if (setEnabled == null)
            throw new ArgumentNullException(nameof(setEnabled));

        var projectKind = _visualStudioAccess.GetSelectedProjectKind();
        if (projectKind == Guid.Empty)
            return;

        var visible = projectKind == Guid.Parse(Constants.SqlProjectKindGuid);
        var enabled = visible && !_scaffoldingService.IsScaffolding && !_scriptCreationService.IsCreating;

        setVisible(visible);
        setEnabled(enabled);
    }
}