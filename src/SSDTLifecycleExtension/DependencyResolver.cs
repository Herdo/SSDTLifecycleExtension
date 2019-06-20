namespace SSDTLifecycleExtension
{
    using System;
    using Windows;
    using DataAccess;
    using JetBrains.Annotations;
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
        private bool _disposed;

        internal DependencyResolver([NotNull] IVisualStudioAccess visualStudioAccess,
                                    [NotNull] ILogger logger,
                                    [NotNull] OleMenuCommandService commandService)
        {
            if (visualStudioAccess == null)
                throw new ArgumentNullException(nameof(visualStudioAccess));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (commandService == null)
                throw new ArgumentNullException(nameof(commandService));

            _container = CreateContainer(visualStudioAccess,
                                         logger,
                                         commandService);
        }

        private static IUnityContainer CreateContainer(IVisualStudioAccess visualStudioAccess,
                                                       ILogger logger,
                                                       OleMenuCommandService commandService)
        {
            return new UnityContainer()

                   // Visual Studio dependencies
                  .RegisterInstance(commandService, new ContainerControlledLifetimeManager()) // The command service

                   // Tool window initialization
                  .RegisterSingleton<ToolWindowInitializer>()

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
                  .RegisterInstance(visualStudioAccess, new ContainerControlledLifetimeManager())
                  .RegisterInstance(logger, new ContainerControlledLifetimeManager())
                  .RegisterType<IDacAccess, DacAccess>()

                   // Factories
                  .RegisterSingleton<IScriptModifierFactory, ScriptModifierFactory>();
        }

        internal void RegisterPackage<TImplementation>([NotNull] TImplementation package)
            where TImplementation : IAsyncPackage
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DependencyResolver));
            if (package == null)
                throw new ArgumentNullException(nameof(package));
            _container.RegisterInstance(package, new ContainerControlledLifetimeManager());
        }

        internal T Get<T>()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DependencyResolver));
            return _container.Resolve<T>();
        }

        internal TViewModel GetViewModel<TViewModel>([NotNull] SqlProject project) where TViewModel : IViewModel
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DependencyResolver));
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
            if (_disposed)
                return;
            _disposed = true;
            _container.Dispose();
        }
    }
}