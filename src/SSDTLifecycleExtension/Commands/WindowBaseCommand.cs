namespace SSDTLifecycleExtension.Commands
{
    using System;
    using Windows;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Shared.Contracts.DataAccess;
    using Shared.Contracts.Services;
    using ViewModels;

    internal abstract class WindowBaseCommand<TWindow, TViewModel> : BaseCommand
        where TWindow : ToolWindowPane, IVisualStudioToolWindow
        where TViewModel : IViewModel
    {
        private readonly SSDTLifecycleExtensionPackage _package;
        private readonly IVisualStudioAccess _visualStudioAccess;

        protected WindowBaseCommand(SSDTLifecycleExtensionPackage package,
                                    OleMenuCommandService commandService,
                                    ICommandAvailabilityService commandAvailabilityService,
                                    IVisualStudioAccess visualStudioAccess,
                                    int commandId,
                                    Guid commandSet)
            : base(commandService,
                   commandAvailabilityService,
                   commandId,
                   commandSet)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            _visualStudioAccess = visualStudioAccess ?? throw new ArgumentNullException(nameof(visualStudioAccess));
        }

        protected override void Execute(object sender,
                                        EventArgs e)
        {
            _package.JoinableTaskFactory.RunAsync(async delegate
            {
                if (!(await _package.ShowToolWindowAsync(typeof(TWindow), 0, true, _package.DisposalToken) is TWindow window)
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
                    var viewModel = _package.GetViewModel<TViewModel>(project);
                    windowContent.SetDataContext(viewModel);
                }
                // Show the frame
                var windowFrame = (IVsWindowFrame)window.Frame;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            });
        }
    }
}