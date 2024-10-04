#nullable enable

using System.Text.RegularExpressions;
using Community.VisualStudio.Toolkit;
using CommunityToolkit.Diagnostics;

namespace SSDTLifecycleExtension.DataAccess;

[ExcludeFromCodeCoverage] // Test would require a Visual Studio shell.
public class VisualStudioAccess : IVisualStudioAccess
{
    private readonly Guid _outputWindowId = Guid.ParseExact(WindowGuids.OutputWindow, "B");
    private Guid? _paneGuid = null;
    private OutputWindowPane? _outputPane = null;

    public VisualStudioAccess()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        VS.Events.SolutionEvents.OnAfterCloseSolution += () => SolutionClosed?.Invoke(this, EventArgs.Empty);
    }

    private async Task<OutputWindowPane> GetOrCreateSSDTOutputPaneAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        if (_outputPane is null)
        {
            if (_paneGuid is not null)
            {
                _outputPane = await VS.Windows.GetOutputWindowPaneAsync(_paneGuid.Value);
            }

            if (_outputPane is null)
            {
                _outputPane = await VS.Windows.CreateOutputWindowPaneAsync("SSDT Lifecycle", false);
                _paneGuid = _outputPane.Guid;
            }
        }

        await _outputPane.ActivateAsync();
        await VS.Windows.ShowToolWindowAsync(_outputWindowId);
        return _outputPane;
    }

    private async Task LogToOutputPanelInternalAsync(string message)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var outputPane = await GetOrCreateSSDTOutputPaneAsync();
        await outputPane.WriteLineAsync(message);
    }

    private async Task<Project?> GetSelectedProjectAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var solutionExplorer = await VS.Windows.GetSolutionExplorerWindowAsync();
        if (solutionExplorer is null)
            return null;

        var solutionItems = await solutionExplorer.GetSelectionAsync();
        if (solutionItems is null)
            return null;

        Project? selectedProject = null;
        foreach (var solutionItem in solutionItems)
        {
            if (selectedProject is not null)
                return null; // There are multiple items selected.

            if (solutionItem.Type != SolutionItemType.Project)
                return null;

            selectedProject = (Project)solutionItem;
        }

        return selectedProject;
    }

    public event EventHandler? SolutionClosed;

    async Task<bool> IVisualStudioAccess.IsSelectedProjectOfKindAsync(string kind)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var selectedProject = await GetSelectedProjectAsync();
        if (selectedProject is null)
            return false;

        return await selectedProject.IsKindAsync(kind);
    }

    async Task<SqlProject?> IVisualStudioAccess.GetSelectedSqlProjectAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var selectedProject = await GetSelectedProjectAsync();
        if (selectedProject?.FullPath is null)
            return null;
        if (await selectedProject.IsKindAsync(Shared.Constants.SqlProjectKindGuid) == false)
            return null;

        return new SqlProject(selectedProject.Name,
                              selectedProject.FullPath,
                              selectedProject);
    }

    async Task IVisualStudioAccess.ClearSSDTLifecycleOutputAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var outputPane = await GetOrCreateSSDTOutputPaneAsync();
        await outputPane.ClearAsync();
    }

    async Task IVisualStudioAccess.ShowModalErrorAsync(string error)
    {
        Guard.IsNotNull(error);
        await VS.MessageBox.ShowErrorAsync("SSDT Lifecycle error", error);
    }

    async Task IVisualStudioAccess.BuildProjectAsync(SqlProject project)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        Guard.IsNotNull(project);

        await VS.Build.BuildProjectAsync((SolutionItem)project.SolutionItem, BuildAction.Rebuild);
    }

    async Task IVisualStudioAccess.StartLongRunningTaskIndicatorAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        await VS.StatusBar.StartAnimationAsync(StatusAnimation.Deploy);
    }

    async Task IVisualStudioAccess.StopLongRunningTaskIndicatorAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        await VS.StatusBar.EndAnimationAsync(StatusAnimation.Deploy);
    }

    async Task IVisualStudioAccess.AddConfigFileToProjectPropertiesAsync(SqlProject project,
        string targetPath)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        Guard.IsNotNull(project);
        Guard.IsNotNull(targetPath);

        var projectItem = (Project)project.SolutionItem;
        var addedFiles = await projectItem.AddExistingFilesAsync(targetPath);
        foreach (var addedFile in addedFiles)
        {
            await addedFile.TrySetAttributeAsync("BuildAction", "None");
        }
    }

    async Task IVisualStudioAccess.RemoveItemFromProjectRootAsync(SqlProject project,
        string path)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        Guard.IsNotNull(project);
        Guard.IsNotNull(path);

        var physicalFile = await PhysicalFile.FromFileAsync(path);
        if (physicalFile is null)
            return;

        var removed = await physicalFile.TryRemoveAsync();
        if (removed)
            await LogToOutputPanelInternalAsync($"Removed file {path} from project {project.FullName} ...");
        else
            await LogToOutputPanelInternalAsync($"Failed to remove file {path} from project {project.FullName} ...");
    }

    Task IVisualStudioAccess.LogToOutputPanelAsync(string message)
    {
        Guard.IsNotNull(message);
        return LogToOutputPanelInternalAsync(message);
    }
}