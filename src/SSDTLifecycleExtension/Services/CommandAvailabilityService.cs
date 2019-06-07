using System;

namespace SSDTLifecycleExtension.Services
{
    using Annotations;
    using DataAccess;
    using Microsoft.VisualStudio.Shell;

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

        void ICommandAvailabilityService.HandleCommandAvailability(object sender,
                                                                   EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!(sender is OleMenuCommand command))
                return;

            var projectKind = _visualStudioAccess.GetSelectedProjectKind();
            if (projectKind == null)
                return;

            command.Visible = projectKind == $"{{{Constants.SqlProjectKindGuid}}}";
            command.Enabled = !_scriptCreationService.IsCreating;
        }
    }
}