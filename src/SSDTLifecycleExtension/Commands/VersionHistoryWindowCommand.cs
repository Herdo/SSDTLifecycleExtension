﻿namespace SSDTLifecycleExtension.Commands
{
    using System;
    using System.ComponentModel.Design;
    using Windows;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using ViewModels;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class VersionHistoryWindowCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0902;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = SSDTLifecycleExtensionPackageCommandSet.CommandSetGuid;

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly SSDTLifecycleExtensionPackage _package;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionHistoryWindowCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private VersionHistoryWindowCommand(SSDTLifecycleExtensionPackage package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandId = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(Execute, menuCommandId);
            menuItem.BeforeQueryStatus += _package.MenuItemOnBeforeQueryStatus;
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

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(SSDTLifecycleExtensionPackage package)
        {
            // Switch to the main thread - the call to AddCommand in VersionHistoryWindowCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new VersionHistoryWindowCommand(package, commandService);
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
                var project = _package.GetSelectedProject();
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
