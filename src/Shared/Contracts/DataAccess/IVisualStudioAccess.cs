namespace SSDTLifecycleExtension.Shared.Contracts.DataAccess;

public interface IVisualStudioAccess
{
    /// <summary>
    ///     Event triggered when the solution is closed.
    /// </summary>
    event EventHandler SolutionClosed;

    /// <summary>
    ///     Gets the kind of the selected project.
    /// </summary>
    /// <returns>
    ///     The kind of the currently selected project, or
    ///     <b>
    ///         <see cref="Guid.Empty" />
    ///     </b>
    ///     , if none is selected.
    /// </returns>
    Guid GetSelectedProjectKind();

    /// <summary>
    ///     Gets the selected SQL project.
    /// </summary>
    /// <returns>The currently selected SQL project, or <b>null</b>, if the selected item is no SQL project.</returns>
    SqlProject? GetSelectedSqlProject();

    Task ClearSSDTLifecycleOutputAsync();

    void ShowModalError(string error);

    void BuildProject(SqlProject project);

    Task StartLongRunningTaskIndicatorAsync();

    Task StopLongRunningTaskIndicatorAsync();

    /// <summary>
    ///     Adds the item at the <paramref name="targetPath" /> to the <paramref name="project" /> properties, if it doesn't
    ///     exist there already.
    /// </summary>
    /// <param name="project">The project to add the item to.</param>
    /// <param name="targetPath">The full path of the item to add to the properties.</param>
    void AddItemToProjectProperties(SqlProject project,
        string targetPath);

    /// <summary>
    ///     Removes the <paramref name="item" /> from the <paramref name="project" />, if it exists within the project.
    /// </summary>
    /// <param name="project">The project to remove the <paramref name="item" /> from.</param>
    /// <param name="item">The file name of the file to remove on the root level.</param>
    void RemoveItemFromProjectRoot(SqlProject project,
        string item);

    Task LogToOutputPanelAsync(string message);
}