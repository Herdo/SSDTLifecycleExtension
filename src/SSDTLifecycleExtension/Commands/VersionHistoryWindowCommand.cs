namespace SSDTLifecycleExtension.Commands
{
    using System;
    using System.ComponentModel.Design;
    using Windows;
    using JetBrains.Annotations;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Shared.Contracts.DataAccess;
    using Shared.Contracts.Services;
    using ViewModels;

    [UsedImplicitly]
    internal sealed class VersionHistoryWindowCommand
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public const int CommandId = 0x0902;

        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly Guid CommandSet = new Guid(SSDTLifecycleExtension.Constants.CommandSetGuid);

        private readonly SSDTLifecycleExtensionPackage _package;
        private readonly IVisualStudioAccess _visualStudioAccess;

        public VersionHistoryWindowCommand(SSDTLifecycleExtensionPackage package,
                                           OleMenuCommandService commandService,
                                           ICommandAvailabilityService commandAvailabilityService,
                                           IVisualStudioAccess visualStudioAccess)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            if (commandService == null) throw new ArgumentNullException(nameof(commandService));
            if (commandAvailabilityService == null) throw new ArgumentNullException(nameof(commandAvailabilityService));
            _visualStudioAccess = visualStudioAccess ?? throw new ArgumentNullException(nameof(visualStudioAccess));

            var menuCommandId = new CommandID(CommandSet, CommandId);
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

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static VersionHistoryWindowCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider => _package;

        public static void Initialize(VersionHistoryWindowCommand instance)
        {
            Instance = instance;
        }

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            _package.JoinableTaskFactory.RunAsync(async delegate
            {
                if (!(await _package.ShowToolWindowAsync(typeof(VersionHistoryWindow), 0, true, _package.DisposalToken) is VersionHistoryWindow window)
                    || window.Frame == null)
                {
                    throw new NotSupportedException("Cannot create tool window");
                }

                await _package.JoinableTaskFactory.SwitchToMainThreadAsync();
                // Set caption
                var project = _visualStudioAccess.GetSelectedSqlProject();
                if (project?.Name != null) window.SetCaption(project.Name);
                // Set data context
                if (window.Content is IView windowContent)
                {
                    var viewModel = _package.GetViewModel<VersionHistoryViewModel>(project);
                    windowContent.SetDataContext(viewModel);
                }
                // Show the frame
                var windowFrame = (IVsWindowFrame)window.Frame;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            });
        }
    }
}
