namespace SSDTLifecycleExtension
{
    using System;
    using System.ComponentModel.Design;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using DataAccess;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Shared.Contracts;
    using Shared.Contracts.DataAccess;
    using Shared.Contracts.Factories;
    using Shared.Contracts.Services;
    using Shared.ScriptModifiers;
    using Shared.Services;
    using Unity;
    using Unity.Lifetime;
    using Unity.Resolution;
    using ViewModels;
    using Task = System.Threading.Tasks.Task;

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(Windows.ScriptCreationWindow), Transient = true, Style = VsDockStyle.Tabbed, MultiInstances = false)]
    [ProvideToolWindow(typeof(Windows.VersionHistoryWindow), Transient = true, Style = VsDockStyle.Tabbed, MultiInstances = false)]
    [ProvideToolWindow(typeof(Windows.ConfigurationWindow), Transient = true, Style = VsDockStyle.Tabbed, MultiInstances = false)]
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

        public const string SqlProjectContextGuid = "b5759c1b-ffdd-48bd-ae82-61317eeb3a75";

        public const string PackageGuidString = "757ac7eb-a0da-4387-9fa2-675e78561cde";

        private IUnityContainer _container;
        private DTE2 _dte2;

        private async Task<IUnityContainer> BuildUnityContainerAsync(CancellationToken cancellationToken)
        {
            if (!(await GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService commandService))
                throw new InvalidOperationException($"Cannot initialize {nameof(SSDTLifecycleExtensionPackage)} without the {nameof(OleMenuCommandService)}.");

            var container = new UnityContainer()

                // Visual Studio dependencies
               .RegisterInstance(this, new ContainerControlledLifetimeManager()) // The package
               .RegisterInstance(commandService, new ContainerControlledLifetimeManager()) // The command service
               
                // ViewModels
               .RegisterType<ScriptCreationViewModel>()
               .RegisterType<VersionHistoryViewModel>()
               .RegisterType<ConfigurationViewModel>()
                
                // Services
               .RegisterSingleton<IConfigurationService, ConfigurationService>()
               .RegisterSingleton<ICommandAvailabilityService, CommandAvailabilityService>()
               .RegisterSingleton<IScriptCreationService, ScriptCreationService>()
               .RegisterSingleton<IVersionService, VersionService>()
               .RegisterSingleton<ISqlProjectService, SqlProjectService>()
                
                // Data Access
               .RegisterSingleton<IFileSystemAccess, FileSystemAccess>()
               .RegisterInstance<IVisualStudioAccess>(new VisualStudioAccess(_dte2, this), new ContainerControlledLifetimeManager())
                
                // Factories
               .RegisterSingleton<IScriptModifierFactory, ScriptModifierFactory>();

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
            _container = await BuildUnityContainerAsync(cancellationToken);

            // Initialize commands
            ScriptCreationWindowCommand.Initialize(_container.Resolve<ScriptCreationWindowCommand>());
            VersionHistoryWindowCommand.Initialize(_container.Resolve<VersionHistoryWindowCommand>());
            ConfigurationWindowCommand.Initialize(_container.Resolve<ConfigurationWindowCommand>());
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

        internal TViewModel GetViewModel<TViewModel>(SqlProject project) where TViewModel : IViewModel
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            // Check for an existing view model registered with the project.
            if (_container.IsRegistered(typeof(TViewModel), project.UniqueName))
                return _container.Resolve<TViewModel>(project.UniqueName);

            // Create a new view model and register it with the project.
            var newViewModel = _container.Resolve<TViewModel>(new ParameterOverride("project", project));
            _container.RegisterInstance(typeof(TViewModel), project.UniqueName, newViewModel, new ContainerControlledLifetimeManager());
            return newViewModel;
        }
    }
}
