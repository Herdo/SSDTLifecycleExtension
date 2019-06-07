namespace SSDTLifecycleExtension.Shared.Services
{
    using System;
    using Contracts.DataAccess;
    using Contracts.Services;
    using Shared;

    [UsedImplicitly]
    public class CommandAvailabilityService : ICommandAvailabilityService
    {
        private readonly IVisualStudioAccess _visualStudioAccess;
        private readonly IScriptCreationService _scriptCreationService;

        public CommandAvailabilityService(IVisualStudioAccess visualStudioAccess,
                                          IScriptCreationService scriptCreationService)
        {
            _visualStudioAccess = visualStudioAccess;
            _scriptCreationService = scriptCreationService;
        }

        void ICommandAvailabilityService.HandleCommandAvailability(Action<bool> setVisible,
                                                                   Action<bool> setEnabled)
        {
            var projectKind = _visualStudioAccess.GetSelectedProjectKind();
            if (projectKind == Guid.Empty)
                return;

            setVisible(projectKind == Guid.Parse(Shared.Constants.SqlProjectKindGuid));
            setEnabled(!_scriptCreationService.IsCreating);
        }
    }
}