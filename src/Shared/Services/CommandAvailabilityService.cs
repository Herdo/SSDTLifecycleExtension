namespace SSDTLifecycleExtension.Shared.Services
{
    using System;
    using Contracts.DataAccess;
    using Contracts.Services;
    using JetBrains.Annotations;

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
            _visualStudioAccess = visualStudioAccess;
            _scaffoldingService = scaffoldingService;
            _scriptCreationService = scriptCreationService;
        }

        void ICommandAvailabilityService.HandleCommandAvailability(Action<bool> setVisible,
                                                                   Action<bool> setEnabled)
        {
            var projectKind = _visualStudioAccess.GetSelectedProjectKind();
            if (projectKind == Guid.Empty)
                return;

            setVisible(projectKind == Guid.Parse(Shared.Constants.SqlProjectKindGuid));
            setEnabled(!_scaffoldingService.IsScaffolding && !_scriptCreationService.IsCreating);
        }
    }
}