namespace SSDTLifecycleExtension
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Microsoft.VisualStudio.Shell;
    using Task = System.Threading.Tasks.Task;

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(SSDTLifecycleExtensionPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(SSDTLifecycleExtension.Windows.VersionHistoryWindow))]
    [ProvideToolWindow(typeof(SSDTLifecycleExtension.Windows.ConfigurationWindow))]
    [ProvideToolWindow(typeof(SSDTLifecycleExtension.Windows.ScriptCreationWindow))]
    public sealed class SSDTLifecycleExtensionPackage : AsyncPackage
    {
        public const string PackageGuidString = "757ac7eb-a0da-4387-9fa2-675e78561cde";

        #region Package Members

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await SSDTLifecycleExtension.Windows.VersionHistoryWindowCommand.InitializeAsync(this);
            await SSDTLifecycleExtension.Windows.ConfigurationWindowCommand.InitializeAsync(this);
            await SSDTLifecycleExtension.Windows.ScriptCreationWindowCommand.InitializeAsync(this);
        }

        #endregion
    }
}
