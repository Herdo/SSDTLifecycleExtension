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

        public CommandAvailabilityService(IVisualStudioAccess visualStudioAccess)
        {
            _visualStudioAccess = visualStudioAccess;
        }

        void ICommandAvailabilityService.HandleCommandAvailability(object sender,
                                                                   EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!(sender is OleMenuCommand command))
                return;

            var project = _visualStudioAccess.GetSelectedProject();
            if (project == null)
                return;

            command.Visible = project.Kind == $"{{{Constants.SqlProjectKindGuid}}}";
        }
    }
}