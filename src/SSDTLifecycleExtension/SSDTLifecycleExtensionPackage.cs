namespace SSDTLifecycleExtension
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Shell;
    using Unity;
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

        private IUnityContainer _container;

        private static IUnityContainer BuildUnityContainer()
        {
            var container = new UnityContainer();
            return container;
        }

        #region Package Members

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // Initialize Unity Container
            _container = BuildUnityContainer();

            // Initialize commands
            await Windows.VersionHistoryWindowCommand.InitializeAsync(this);
            await Windows.ConfigurationWindowCommand.InitializeAsync(this);
            await Windows.ScriptCreationWindowCommand.InitializeAsync(this);
        }

        protected override object GetService(Type serviceType)
        {
            if (_container?.IsRegistered(serviceType) ?? false)
                return _container.Resolve(serviceType);

            return base.GetService(serviceType);
        }

        protected override async Task<object> InitializeToolWindowAsync(Type toolWindowType,
                                                                  int id,
                                                                  CancellationToken cancellationToken)
        {
            return await GetServiceAsync(toolWindowType);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            try
            {
                _container?.Dispose();
            }
            catch { }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}
