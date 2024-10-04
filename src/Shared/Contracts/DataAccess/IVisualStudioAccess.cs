namespace SSDTLifecycleExtension.Shared.Contracts.DataAccess;

public interface IVisualStudioAccess
{
    /// <summary>
    ///     Event triggered when the solution is closed.
    /// </summary>
    event EventHandler SolutionClosed;

    /// <summary>
    ///     Checks if the selected project is of the specified <paramref name="kind" />.
    /// </summary>
    /// <param name="kind">The kind to check for.</param>
    /// <returns>
    ///     <b>True</b>, if the selected project is of the specified <paramref name="kind" />, otherwise <b>false</b>.
    /// </returns>
    Task<bool> IsSelectedProjectOfKindAsync(string kind);

    /// <summary>
    ///     Gets the selected SQL project.
    /// </summary>
    /// <returns>The currently selected SQL project, or <b>null</b>, if the selected item is no SQL project.</returns>
    Task<SqlProject?> GetSelectedSqlProjectAsync();

    Task ClearSSDTLifecycleOutputAsync();

    Task ShowModalErrorAsync(string error);

    Task BuildProjectAsync(SqlProject project);

    Task StartLongRunningTaskIndicatorAsync();

    Task StopLongRunningTaskIndicatorAsync();

    /// <summary>
    ///     Adds the item at the <paramref name="targetPath" /> to the <paramref name="project" /> properties, if it doesn't
    ///     exist there already.
    /// </summary>
    /// <param name="project">The project to add the item to.</param>
    /// <param name="targetPath">The full path of the item to add to the properties.</param>
    Task AddConfigFileToProjectPropertiesAsync(SqlProject project,
        string targetPath);

    /// <summary>
    ///     Removes the <paramref name="path" /> from the <paramref name="project" />, if it exists within the project.
    /// </summary>
    /// <param name="project">The project to remove the <paramref name="path" /> from.</param>
    /// <param name="path">The file path of the file to remove.</param>
    Task RemoveItemFromProjectRootAsync(SqlProject project,
        string path);

    Task LogToOutputPanelAsync(string message);
}