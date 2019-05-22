namespace SSDTLifecycleExtension
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Unity;
    using Task = System.Threading.Tasks.Task;

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(Windows.VersionHistoryWindow))]
    [ProvideToolWindow(typeof(Windows.ConfigurationWindow))]
    [ProvideToolWindow(typeof(Windows.ScriptCreationWindow))]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class SSDTLifecycleExtensionPackage : AsyncPackage
    {
        public const string PackageGuidString = "757ac7eb-a0da-4387-9fa2-675e78561cde";

        private IUnityContainer _container;
        private DTE2 _dte2;

        private static IUnityContainer BuildUnityContainer()
        {
            var container = new UnityContainer();
            return container;
        }

        #region Base Overrides

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            if (!(await GetServiceAsync(typeof(DTE)) is DTE2 dte2))
                return;
            _dte2 = dte2;

            // Initialize Unity Container
            _container = BuildUnityContainer();

            // Initialize commands
            await VersionHistoryWindowCommand.InitializeAsync(this);
            await ConfigurationWindowCommand.InitializeAsync(this);
            await ScriptCreationWindowCommand.InitializeAsync(this);
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

        #endregion

        internal void MenuItemOnBeforeQueryStatus(object sender,
                                                  EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!(sender is OleMenuCommand command))
                return;
            
            if (_dte2.SelectedItems.Count != 1)
                return;

            var project = _dte2.SelectedItems.Item(1).Project;
            if (project == null)
                return;

            command.Visible = project.Kind == "{00d1a9c2-b5f0-4af3-8072-f6c62b433612}"; // *.sqlproj
        }
    }
}
