using System;

namespace SSDTLifecycleExtension.Services
{
    using Annotations;
    using Shared.Contracts.DataAccess;
    using Shared.Contracts.Services;

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
            if (projectKind == null)
                return;

            setVisible(projectKind == $"{{{Shared.Constants.SqlProjectKindGuid}}}");
            setEnabled(!_scriptCreationService.IsCreating);
        }
    }
}