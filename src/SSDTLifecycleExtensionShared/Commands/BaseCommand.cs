#nullable enable

namespace SSDTLifecycleExtension.Commands;

internal abstract class BaseCommand
{
    protected BaseCommand(OleMenuCommandService commandService,
                          ICommandAvailabilityService commandAvailabilityService,
                          int commandId,
                          Guid commandSet)
    {
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