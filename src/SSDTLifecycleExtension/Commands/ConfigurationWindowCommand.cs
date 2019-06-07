namespace SSDTLifecycleExtension.Commands
{
    using System;
    using System.ComponentModel.Design;
    using Windows;
    using Annotations;
    using DataAccess;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Services;
    using Shared.Contracts;
    using ViewModels;

    [UsedImplicitly]
    internal sealed class ConfigurationWindowCommand
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public const int CommandId = 0x0903;

        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly Guid CommandSet = new Guid(SSDTLifecycleExtension.Constants.CommandSetGuid);

        private readonly SSDTLifecycleExtensionPackage _package;
        private readonly IVisualStudioAccess _visualStudioAccess;

        public ConfigurationWindowCommand(SSDTLifecycleExtensionPackage package,
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
            menuItem.BeforeQueryStatus += commandAvailabilityService.HandleCommandAvailability;
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ConfigurationWindowCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider => _package;

        public static void Initialize(ConfigurationWindowCommand instance)
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
                if (!(await _package.ShowToolWindowAsync(typeof(ConfigurationWindow), 0, true, _package.DisposalToken) is ConfigurationWindow window)
                    || window.Frame == null)
                {
                    throw new NotSupportedException("Cannot create tool window");
                }

                await _package.JoinableTaskFactory.SwitchToMainThreadAsync();
                // Set caption
                var project = _visualStudioAccess.GetSelectedSqlProject();
                if (project != null) window.SetCaption(project.Name);
                // Set data context
                if (window.Content is IView windowContent)
                {
                    var viewModel = _package.GetViewModel<ConfigurationViewModel>(project);
                    await viewModel.InitializeAsync();
                    windowContent.SetDataContext(viewModel);
                }
                // Show the frame
                var windowFrame = (IVsWindowFrame)window.Frame;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            });
        }
    }
}
