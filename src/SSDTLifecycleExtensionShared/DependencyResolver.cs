#nullable enable

namespace SSDTLifecycleExtension;

internal sealed class DependencyResolver : IDependencyResolver
{
    private readonly IUnityContainer _container;
    private readonly Dictionary<Type, Dictionary<string, object>> _viewModels;
    private bool _disposed;

    internal DependencyResolver(IVisualStudioAccess visualStudioAccess,
        ILogger logger,
        OleMenuCommandService commandService)
    {
        _container = CreateContainer(visualStudioAccess,
            logger,
            commandService);
        _viewModels = [];
    }

    private IUnityContainer CreateContainer(IVisualStudioAccess visualStudioAccess,
        ILogger logger,
        OleMenuCommandService commandService)
    {
        return new UnityContainer()

            // Register self
            .RegisterInstance(typeof(IDependencyResolver), this, new ContainerControlledLifetimeManager())

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
            .RegisterType<IXmlFormatService, XmlFormatService>()
            .RegisterType<IArtifactsService, ArtifactsService>()
            .RegisterType<IScriptModifierProviderService, ScriptModifierProviderService>()

            // Data Access
            .RegisterSingleton<IFileSystemAccess, FileSystemAccess>()
            .RegisterInstance(visualStudioAccess, new ContainerControlledLifetimeManager())
            .RegisterInstance(logger, new ContainerControlledLifetimeManager())
            .RegisterType<IDacAccess, DacAccess>()

            // Factories
            .RegisterSingleton<IScriptModifierFactory, ScriptModifierFactory>()
            .RegisterSingleton<IWorkUnitFactory, WorkUnitFactory>();
    }

    internal void RegisterPackage<TImplementation>(TImplementation package)
        where TImplementation : IAsyncPackage
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DependencyResolver));
        _container.RegisterInstance(package, new ContainerControlledLifetimeManager());
    }

    public T Get<T>()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DependencyResolver));
        return _container.Resolve<T>();
    }

    internal TViewModel GetViewModel<TViewModel>(SqlProject project)
        where TViewModel : IViewModel
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DependencyResolver));

        // Check for an existing view model registered with the project.
        if (_viewModels.TryGetValue(typeof(TViewModel), out var instances)
         && instances.TryGetValue(project.UniqueName, out var instance)
         && instance is TViewModel existingViewModel)
            return existingViewModel;

        // Create a new view model and register it with the project.
        var newViewModel = _container.Resolve<TViewModel>(new ParameterOverride("project", project));
        if (!_viewModels.TryGetValue(typeof(TViewModel), out instances))
        {
            instances = new Dictionary<string, object>();
            _viewModels.Add(typeof(TViewModel), instances);
        }
        instances[project.UniqueName] = newViewModel;

        return newViewModel;
    }

    internal void HandleSolutionClosed()
    {
        _viewModels.Clear();
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _viewModels.Clear();
        _container.Dispose();
    }
}