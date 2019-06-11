namespace SSDTLifecycleExtension.Shared.Contracts.DataAccess
{
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    public interface IFileSystemAccess
    {
        Task<string> ReadFileAsync([NotNull] string sourcePath);

        Task WriteFileAsync([NotNull] string targetPath,
                            [NotNull] string content);

        string BrowseForFile([NotNull] string extension,
                             [NotNull] string filter);

        bool CheckIfFileExists([NotNull] string filePath);

        string EnsureDirectoryExists([NotNull] string path);

        string CopyFiles([NotNull] string sourceDirectory,
                         [NotNull] string targetDirectory,
                         [NotNull] string searchPattern);

        string[] GetDirectoriesIn([NotNull] string directory);
    }
}