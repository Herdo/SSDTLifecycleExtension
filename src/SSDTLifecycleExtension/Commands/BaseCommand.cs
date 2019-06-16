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

                if (!(sender is OleMenuCommand command))

                    return;

                commandAvailabilityService.HandleCommandAvailability(b => command.Visible = b, b => command.Enabled = b);

            };

            commandService.AddCommand(menuItem);
        }

        protected abstract void Execute(object sender,
                                        EventArgs e);
    }
}