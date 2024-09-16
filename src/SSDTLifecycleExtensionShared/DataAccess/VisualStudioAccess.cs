#nullable enable

namespace SSDTLifecycleExtension.DataAccess;

[ExcludeFromCodeCoverage] // Test would require a Visual Studio shell.
public class VisualStudioAccess : IVisualStudioAccess
{
    private readonly DTE2 _dte2;
    private readonly AsyncPackage _package;
    private Guid _paneGuid;

    public VisualStudioAccess(DTE2 dte2,
        AsyncPackage package)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        _dte2 = dte2;
        _dte2.Events.SolutionEvents.AfterClosing += () => SolutionClosed?.Invoke(this, EventArgs.Empty);
        _package = package;
        _paneGuid = new Guid(Constants.CreationProgressPaneGuid);
    }

    private async System.Threading.Tasks.Task<IVsOutputWindowPane> GetOrCreateSSDTOutputPaneAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        if (!(await _package.GetServiceAsync(typeof(SVsOutputWindow)) is IVsOutputWindow outputWindow))
            throw new InvalidOperationException($"Cannot get {nameof(IVsOutputWindow)}.");

        var getPaneResult = outputWindow.GetPane(ref _paneGuid, out var outputPane);
        if (getPaneResult == Microsoft.VisualStudio.VSConstants.S_OK)
        {
            outputPane.Activate();
            return outputPane;
        }

        var createPaneResult = outputWindow.CreatePane(ref _paneGuid, "SSDT Lifecycle", 1, 1);
        if (createPaneResult != Microsoft.VisualStudio.VSConstants.S_OK)
            throw new InvalidOperationException("Failed to get or create SSDT Lifecycle output pane.");
        getPaneResult = outputWindow.GetPane(ref _paneGuid, out outputPane);
        if (getPaneResult == Microsoft.VisualStudio.VSConstants.S_OK)
        {
            outputPane.Activate();
            return outputPane;
        }

        throw new InvalidOperationException("Failed to get or create SSDT Lifecycle output pane.");
    }

    private async Task LogToOutputPanelInternalAsync(string message)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var outputPane = await GetOrCreateSSDTOutputPaneAsync();
        outputPane.OutputStringThreadSafe(message);
        outputPane.OutputStringThreadSafe(Environment.NewLine);
    }

    public event EventHandler? SolutionClosed;

    Guid IVisualStudioAccess.GetSelectedProjectKind()
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        return _dte2.SelectedItems.Count == 1
            ? Guid.Parse(_dte2.SelectedItems.Item(1).Project.Kind)
            : Guid.Empty;
    }

    SqlProject? IVisualStudioAccess.GetSelectedSqlProject()
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (_dte2.SelectedItems.Count != 1)
            return null;

        var selectedProject = _dte2.SelectedItems.Item(1).Project;
        var selectedProjectKindGuid = Guid.Parse(selectedProject.Kind);
        var sqlProjectKindGuid = Guid.Parse(Shared.Constants.SqlProjectKindGuid);
        if (selectedProjectKindGuid != sqlProjectKindGuid)
            return null;

        return new SqlProject(selectedProject.Name,
                              selectedProject.FullName,
                              selectedProject.UniqueName);
    }

    async Task IVisualStudioAccess.ClearSSDTLifecycleOutputAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var outputPane = await GetOrCreateSSDTOutputPaneAsync();
        outputPane.Clear();
    }

    void IVisualStudioAccess.ShowModalError(string error)
    {
        if (error == null)
            throw new ArgumentNullException(nameof(error));
        MessageBox.Show(error, "SSDT Lifecycle error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
    }

    void IVisualStudioAccess.BuildProject(SqlProject project)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (project == null)
            throw new ArgumentNullException(nameof(project));

        var sb = _dte2.Solution.SolutionBuild;
        sb.BuildProject("Release",
                        project.UniqueName,
                        true);
    }

    async Task IVisualStudioAccess.StartLongRunningTaskIndicatorAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        if (!(await _package.GetServiceAsync(typeof(SVsStatusbar)) is IVsStatusbar statusBar))
            throw new InvalidOperationException($"Cannot get {nameof(IVsStatusbar)}.");

        // Use the standard Visual Studio icon for building.
        object icon = (short)Microsoft.VisualStudio.Shell.Interop.Constants.SBAI_Deploy;

        // Display the icon in the Animation region.
        statusBar.Animation(1, ref icon);
    }

    async Task IVisualStudioAccess.StopLongRunningTaskIndicatorAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        if (!(await _package.GetServiceAsync(typeof(SVsStatusbar)) is IVsStatusbar statusBar))
            throw new InvalidOperationException($"Cannot get {nameof(IVsStatusbar)}.");

        // Use the standard Visual Studio icon for building.
        object icon = (short)Microsoft.VisualStudio.Shell.Interop.Constants.SBAI_Deploy;

        // Stop the animation.
        statusBar.Animation(0, ref icon);
    }

    void IVisualStudioAccess.AddItemToProjectProperties(SqlProject project,
                                                        string targetPath)
    {
        if (project == null)
            throw new ArgumentNullException(nameof(project));
        if (targetPath == null)
            throw new ArgumentNullException(nameof(targetPath));

        ThreadHelper.ThrowIfNotOnUIThread();
        var p = _dte2.Solution.Projects.OfType<Project>().SingleOrDefault(m =>
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return m.UniqueName == project.UniqueName;
        });

        var properties = p?.ProjectItems.OfType<ProjectItem>().SingleOrDefault(m =>
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return m.Name == "Properties";
        });
        if (properties == null)
            return;

        var fileName = Path.GetFileName(targetPath);
        if (properties.ProjectItems.OfType<ProjectItem>().All(m =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return m.Name != fileName;
            })) properties.ProjectItems.AddFromFile(targetPath);
    }

    void IVisualStudioAccess.RemoveItemFromProjectRoot(SqlProject project,
                                                       string item)
    {
        if (project == null)
            throw new ArgumentNullException(nameof(project));
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        ThreadHelper.ThrowIfNotOnUIThread();
        var p = _dte2.Solution.Projects.OfType<Project>().SingleOrDefault(m =>
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return m.UniqueName == project.UniqueName;
        });

        var matchingItem = p?.ProjectItems.OfType<ProjectItem>().SingleOrDefault(m =>
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return m.Name == item;
        });

        matchingItem?.Remove();
    }

    Task IVisualStudioAccess.LogToOutputPanelAsync(string message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        return LogToOutputPanelInternalAsync(message);
    }
}