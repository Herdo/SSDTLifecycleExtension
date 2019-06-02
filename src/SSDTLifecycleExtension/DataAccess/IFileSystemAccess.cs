namespace SSDTLifecycleExtension.DataAccess
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Annotations;

    public interface IFileSystemAccess
    {
        Task<string> ReadFileAsync([NotNull] string sourcePath);

        Task WriteFileAsync([NotNull] string targetPath,
                            [NotNull] string content);

        string BrowseForFile([NotNull] string extension,
                             [NotNull] string filter);

        string[] SearchForFiles(Environment.SpecialFolder rootFolder,
                                [NotNull] string subFolder,
                                [NotNull] string searchPattern);

        bool CheckIfFileExists([NotNull] string filePath);

        string EnsureDirectoryExists([NotNull] string path);

        Task<string> StartProcessAndWaitAsync([NotNull] string fileName,
                                              [NotNull] string arguments,
                                              [CanBeNull] Func<string, Task> outputDataHandler,
                                              [CanBeNull] Func<string, Task> errorDataHandler,
                                              CancellationToken cancellationToken);

        string CopyFiles([NotNull] string sourceDirectory,
                         [NotNull] string targetDirectory,
                         [NotNull] string searchPattern);
    }
}