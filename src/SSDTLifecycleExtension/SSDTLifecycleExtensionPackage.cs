namespace SSDTLifecycleExtension
{
    using System;
    using System.ComponentModel.Design;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows;
    using Commands;
    using DataAccess;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Task = System.Threading.Tasks.Task;

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(ScriptCreationWindow), Transient = true, Style = VsDockStyle.Tabbed, MultiInstances = false)]
    [ProvideToolWindow(typeof(VersionHistoryWindow), Transient = true, Style = VsDockStyle.Tabbed, MultiInstances = false)]
    [ProvideToolWindow(typeof(ConfigurationWindow), Transient = true, Style = VsDockStyle.Tabbed, MultiInstances = false)]
    [ProvideAutoLoad(SqlProjectContextGuid, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideUIContextRule(SqlProjectContextGuid,
        name: "SqlProject auto load",
        expression: "(SingleProject | MultipleProjects) & SolutionReady & SqlProject",
        termNames: new[]
        {
            "SingleProject",
            "MultipleProjects",
            "SolutionReady",
            "SqlProject"
        },
        termValues: new[]
        {
            VSConstants.UICONTEXT.SolutionHasSingleProject_string,
            VSConstants.UICONTEXT.SolutionHasMultipleProjects_string,
            VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string,
            "ActiveProjectFlavor:" + Shared.Constants.SqlProjectKindGuid
        })]
    public sealed class SSDTLifecycleExtensionPackage : AsyncPackage
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public const string SqlProjectContextGuid = "b5759c1b-ffdd-48bd-ae82-61317eeb3a75";

        // ReSharper disable once MemberCanBePrivate.Global
        public const string PackageGuidString = "757ac7eb-a0da-4387-9fa2-675e78561cde";

        private DependencyResolver _dependencyResolver;
        private DTE2 _dte2;

        private async Task<DependencyResolver> GetDependencyResolverAsync()
        {
            if (!(await GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService commandService))
                throw new InvalidOperationException($"Cannot initialize {nameof(SSDTLifecycleExtensionPackage)} without the {nameof(OleMenuCommandService)}.");

            var visualStudioAccess = new VisualStudioAccess(_dte2, this);
            return new DependencyResolver(visualStudioAccess, commandService);
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

            // Initialize DependencyResolver
            _dependencyResolver = await GetDependencyResolverAsync();

            // Initialize commands
            ScriptCreationWindowCommand.Initialize(_dependencyResolver.Get<ScriptCreationWindowCommand>());
            VersionHistoryWindowCommand.Initialize(_dependencyResolver.Get<VersionHistoryWindowCommand>());
            ConfigurationWindowCommand.Initialize(_dependencyResolver.Get<ConfigurationWindowCommand>());
        }

        public override IVsAsyncToolWindowFactory GetAsyncToolWindowFactory(Guid toolWindowType)
        {
            if (toolWindowType == typeof(ScriptCreationWindow).GUID
                || toolWindowType == typeof(VersionHistoryWindow).GUID
                || toolWindowType == typeof(ConfigurationWindow).GUID)
                return this;

            ThreadHelper.ThrowIfNotOnUIThread();
            return base.GetAsyncToolWindowFactory(toolWindowType);
        }

        protected override string GetToolWindowTitle(Type toolWindowType,
                                                     int id)
        {
            return ReferenceEquals(typeof(SSDTLifecycleExtensionPackage).Assembly, toolWindowType.Assembly)
                       ? "Loading SSDT Lifecycle window ..."
                       : base.GetToolWindowTitle(toolWindowType, id);
        }

        protected override Task<object> InitializeToolWindowAsync(Type toolWindowType,
                                                                  int id,
                                                                  CancellationToken cancellationToken)
        {
            // If there's any common async one-time initialization for tool windows to perform, perform them here.
            return Task.FromResult((object)string.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                _dependencyResolver?.Dispose();
            }
            catch
            {
                // ignored - when the extension gets disposed, there's no logger left to log the exception.
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        #endregion
    }
}
