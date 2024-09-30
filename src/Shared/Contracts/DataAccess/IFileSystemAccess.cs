namespace SSDTLifecycleExtension.Shared.Contracts.DataAccess;

public interface IFileSystemAccess
{
    Task<string> ReadFileAsync(string sourcePath);

    Task WriteFileAsync(string targetPath,
        string content);

    string? BrowseForFile(string extension,
        string filter);

    bool CheckIfFileExists(string filePath);

    string? EnsureDirectoryExists(string path);

    /// <summary>
    ///     Tries to clean all files in under the <paramref name="directoryPath" />.
    /// </summary>
    /// <param name="directoryPath">The path of the directory to clean.</param>
    /// <exception cref="ArgumentException">
    ///     <paramref name="directoryPath" /> is <b>null</b>, empty or contains only white
    ///     spaces.
    /// </exception>
    void TryToCleanDirectory(string directoryPath);

    /// <summary>
    ///     Tries to clean the files under the <paramref name="directoryPath" /> that match the given
    ///     <paramref name="filter" />.
    /// </summary>
    /// <param name="directoryPath">The path of the directory to clean.</param>
    /// <param name="filter">The filter that must match for files that should be deleted.</param>
    /// <exception cref="ArgumentException">
    ///     <paramref name="directoryPath" /> is <b>null</b>, empty or contains only white
    ///     spaces.
    /// </exception>
    /// <returns>The names of the deleted files. An empty array if no files were deleted.</returns>
    string[] TryToCleanDirectory(string directoryPath,
        string filter);

    ((string Source, string Target)[] CopiedFiles, (string? File, Exception Exception)[] Errors) CopyFiles(string sourceDirectory,
        string targetDirectory,
        string searchPattern);

    string? CopyFile(string source,
        string target);

    string[] GetDirectoriesIn(string directory);

    string[] GetFilesIn(string directory,
        string filter);

    void OpenUrl(string url);
}