namespace SSDTLifecycleExtension.Commands
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Windows;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Shared.Contracts.Services;
    using ViewModels;

    [ExcludeFromCodeCoverage] // Test would require a Visual Studio shell.
    internal abstract class WindowBaseCommand<TWindow, TViewModel> : BaseCommand
        where TWindow : ToolWindowPane, IVisualStudioToolWindow
        where TViewModel : IViewModel
    {
        private readonly SSDTLifecycleExtensionPackage _package;
        private readonly ToolWindowInitializer _toolWindowInitializer;

        protected WindowBaseCommand(SSDTLifecycleExtensionPackage package,
                                    OleMenuCommandService commandService,
                                    ICommandAvailabilityService commandAvailabilityService,
                                    int commandId,
                                    Guid commandSet,
                                    ToolWindowInitializer toolWindowInitializer)
            : base(commandService,
                   commandAvailabilityService,
                   commandId,
                   commandSet)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            _toolWindowInitializer = toolWindowInitializer ?? throw new ArgumentNullException(nameof(toolWindowInitializer));
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

                var (success, fullProjectPath) = await _toolWindowInitializer.TryInitializeToolWindowAsync<TViewModel>(window);
                if (!success)
                    return;

                // Show the frame
                var windowFrame = (IVsWindowFrame) window.Frame;
                _package.RegisterWindowFrame(fullProjectPath, windowFrame);
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            });
        }
    }
}