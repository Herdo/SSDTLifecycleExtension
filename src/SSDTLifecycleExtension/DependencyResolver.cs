namespace SSDTLifecycleExtension
{
    using System;
    using DataAccess;
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

    internal sealed class DependencyResolver : IDisposable
    {
        private readonly IUnityContainer _container;

        internal DependencyResolver(VisualStudioAccess visualStudioAccess,
                                    OleMenuCommandService commandService)
        {
            _container = new UnityContainer()

                         // Visual Studio dependencies
                        .RegisterInstance(this, new ContainerControlledLifetimeManager()) // The package
                        .RegisterInstance(commandService, new ContainerControlledLifetimeManager()) // The command service

                         // ViewModels
                        .RegisterType<ScriptCreationViewModel>()
                        .RegisterType<VersionHistoryViewModel>()
                        .RegisterType<ConfigurationViewModel>()

                         // Services with state / events
                        .RegisterSingleton<IConfigurationService, ConfigurationService>()
                        .RegisterSingleton<IScaffoldingService, ScaffoldingService>()
                        .RegisterSingleton<IScriptCreationService, ScriptCreationService>()

                         // Stateless services
                        .RegisterType<ICommandAvailabilityService, CommandAvailabilityService>()
                        .RegisterType<IBuildService, BuildService>()
                        .RegisterType<IVersionService, VersionService>()
                        .RegisterType<ISqlProjectService, SqlProjectService>()

                         // Data Access
                        .RegisterSingleton<IFileSystemAccess, FileSystemAccess>()
                        .RegisterInstance<IVisualStudioAccess>(visualStudioAccess, new ContainerControlledLifetimeManager())
                        .RegisterInstance<ILogger>(visualStudioAccess, new ContainerControlledLifetimeManager())
                        .RegisterType<IDacAccess, DacAccess>()

                         // Factories
                        .RegisterSingleton<IScriptModifierFactory, ScriptModifierFactory>();
        }

        internal T Get<T>() => _container.Resolve<T>();

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

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}