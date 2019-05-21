namespace SSDTLifecycleExtension
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Microsoft.VisualStudio.Shell;
    using Task = System.Threading.Tasks.Task;

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(Windows.VersionHistoryWindow))]
    [ProvideToolWindow(typeof(Windows.ConfigurationWindow))]
    [ProvideToolWindow(typeof(Windows.ScriptCreationWindow))]
    public sealed class SSDTLifecycleExtensionPackage : AsyncPackage
    {
        public const string PackageGuidString = "757ac7eb-a0da-4387-9fa2-675e78561cde";

        #region Package Members

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await Windows.VersionHistoryWindowCommand.InitializeAsync(this);
            await Windows.ConfigurationWindowCommand.InitializeAsync(this);
            await Windows.ScriptCreationWindowCommand.InitializeAsync(this);
        }

        #endregion
    }
}
