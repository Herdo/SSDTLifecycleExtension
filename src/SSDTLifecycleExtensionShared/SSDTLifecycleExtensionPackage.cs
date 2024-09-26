﻿#nullable enable

namespace SSDTLifecycleExtension;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[Guid(PackageGuidString)]
[ProvideMenuResource("Menus.ctmenu", 1)]
[ProvideToolWindow(typeof(ScriptCreationWindow), Transient = true, Window = EnvDTE.Constants.vsWindowKindMainWindow, Style = VsDockStyle.Tabbed, MultiInstances = false,
                   Height = 900, Width = 900, DockedHeight = 900, DockedWidth = 900)]
[ProvideToolWindow(typeof(VersionHistoryWindow), Transient = true, Window = EnvDTE.Constants.vsWindowKindMainWindow, Style = VsDockStyle.Tabbed, MultiInstances = false)]
[ProvideToolWindow(typeof(ConfigurationWindow), Transient = true, Window = EnvDTE.Constants.vsWindowKindMainWindow, Style = VsDockStyle.Tabbed, MultiInstances = false,
                   Height = 900, Width = 900, DockedHeight = 900, DockedWidth = 900)]
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
[ExcludeFromCodeCoverage] // Test would require a Visual Studio shell.
public sealed class SSDTLifecycleExtensionPackage : AsyncPackage, IAsyncPackage
{
    // ReSharper disable once MemberCanBePrivate.Global
    public const string SqlProjectContextGuid = "b5759c1b-ffdd-48bd-ae82-61317eeb3a75";

    // ReSharper disable once MemberCanBePrivate.Global
    public const string PackageGuidString = "757ac7eb-a0da-4387-9fa2-675e78561cde";

    private readonly Dictionary<string, List<IVsWindowFrame>> _openedWindowFrames;

    private DependencyResolver? _dependencyResolver;
    private DTE2? _dte2;

    public SSDTLifecycleExtensionPackage()
    {
        _openedWindowFrames = new Dictionary<string, List<IVsWindowFrame>>();
    }

    private async Task<DependencyResolver> GetDependencyResolverAsync()
    {
        if (!(await GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService commandService))
            throw new InvalidOperationException($"Cannot initialize {nameof(SSDTLifecycleExtensionPackage)} without the {nameof(OleMenuCommandService)}.");
        if (_dte2 is null)
            throw new InvalidOperationException("Missing DTE2 to initialize dependency resolver.");

        var visualStudioAccess = new VisualStudioAccess(_dte2, this);
        var visualStudioLogger = new VisualStudioLogger(visualStudioAccess, Constants.DocumentationBaseUrl);
        return new DependencyResolver(visualStudioAccess, visualStudioLogger, commandService);
    }

    private void AttachToDte2Events()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (_dte2 is null)
            return;
        _dte2.Events.SolutionEvents.AfterClosing += SolutionEvents_AfterClosing;
        _dte2.Events.SolutionEvents.ProjectRemoved += SolutionEvents_ProjectRemoved;
        _dte2.Events.SolutionEvents.ProjectRenamed += SolutionEvents_ProjectRenamed;
    }

    private void SolutionEvents_AfterClosing()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        CloseOpenFrames(null);
    }

    private void SolutionEvents_ProjectRemoved(Project project)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        CloseOpenFrames(project.FullName);
    }

    private void SolutionEvents_ProjectRenamed(Project project, string oldName)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        CloseOpenFrames(oldName);
    }

    private void CloseOpenFrames(string? filter)
    {
        var keyToRemove = new List<string>();
        foreach (var windowFrames in _openedWindowFrames.Where(m => filter is null || m.Key == filter))
        {
            keyToRemove.Add(windowFrames.Key);
            foreach (var windowFrame in windowFrames.Value)
            {
                try
                {
                    ThreadHelper.ThrowIfNotOnUIThread();
                    windowFrame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);
                }
                catch
                {
                    // Even when closing a window fails, we try to close the other windows as well.
                    // When closing fails after the solution is closed, there's no reason to notify the user.
                    // In that case, we're just leaving behind an unclean UI state.
                }
            }
        }

        foreach (var key in keyToRemove)
            _openedWindowFrames.Remove(key);
    }

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
        _dependencyResolver.RegisterPackage(this);

        // Initialize DTE event handlers
        AttachToDte2Events();

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

    /// <summary>
    /// Adds the <paramref name="windowFrame"/> to an internal list. When the solution is closed, all registered window frames will be closed.
    /// </summary>
    /// <param name="fullProjectPath">The full path of the project the <paramref name="windowFrame"/> was opened for.</param>
    /// <param name="windowFrame">The opened <see cref="IVsWindowFrame"/>.</param>
    internal void RegisterWindowFrame(string fullProjectPath,
        IVsWindowFrame windowFrame)
    {
        if (!_openedWindowFrames.TryGetValue(fullProjectPath, out var windowFrames))
        {
            windowFrames = [];
            _openedWindowFrames[fullProjectPath] = windowFrames;
        }

        if (!windowFrames.Contains(windowFrame))
            windowFrames.Add(windowFrame);
    }
}