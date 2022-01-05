namespace SSDTLifecycleExtension.Commands
{
    using System;
    using System.ComponentModel.Design;
    using Microsoft.VisualStudio.Shell;
    using Shared.Contracts.Services;

    internal abstract class BaseCommand
    {
        protected BaseCommand(OleMenuCommandService commandService,
                              ICommandAvailabilityService commandAvailabilityService,
                              int commandId,
                              Guid commandSet)
        {
            if (commandService == null) throw new ArgumentNullException(nameof(commandService));
            if (commandAvailabilityService == null) throw new ArgumentNullException(nameof(commandAvailabilityService));

            var menuCommandId = new CommandID(commandSet, commandId);
            var menuItem = new OleMenuCommand(Execute, menuCommandId);
            menuItem.BeforeQueryStatus += (sender,
                                           args) =>
            {
                commandAvailabilityService.HandleCommandAvailability(b => menuItem.Visible = b,
                                                                     b => menuItem.Enabled = b);
            };

            commandService.AddCommand(menuItem);
        }

        protected abstract void Execute(object sender,
                                        EventArgs e);
    }
}