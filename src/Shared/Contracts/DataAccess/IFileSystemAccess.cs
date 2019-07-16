namespace SSDTLifecycleExtension.Shared.Contracts.DataAccess
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    public interface IFileSystemAccess
    {
        Task<string> ReadFileAsync([NotNull] string sourcePath);

        /// <summary>
        /// Reads the stream from the <paramref name="sourcePath"/> and passes it to the <paramref name="streamConsumer"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result the <paramref name="streamConsumer"/> will provide.</typeparam>
        /// <param name="sourcePath">The path to the file to read.</param>
        /// <param name="streamConsumer">The consumer who will transform the stream to the result.</param>
        /// <exception cref="ArgumentNullException"><paramref name="sourcePath"/> or <paramref name="streamConsumer"/> are <b>null</b>.</exception>
        /// <returns>The <typeparamref name="TResult"/>, when the stream consumer was called, otherwise null.</returns>
        /// <remarks>The result should be disposed when no longer needed.</remarks>
        [NotNull]
        SecureStreamResult<TResult> ReadFromStream<TResult>([NotNull] string sourcePath,
                                                            [NotNull] Func<Stream, TResult> streamConsumer)
            where TResult : class;

        Task WriteFileAsync([NotNull] string targetPath,
                            [NotNull] string content);

        string BrowseForFile([NotNull] string extension,
                             [NotNull] string filter);

        bool CheckIfFileExists([NotNull] string filePath);

        string EnsureDirectoryExists([NotNull] string path);

        /// <summary>
        /// Tries to clean all files in under the <paramref name="directoryPath"/>.
        /// </summary>
        /// <param name="directoryPath">The path of the directory to clean.</param>
        /// <exception cref="ArgumentException"><paramref name="directoryPath"/> is <b>null</b>, empty or contains only white spaces.</exception>
        void TryToCleanDirectory([NotNull] string directoryPath);

        /// <summary>
        /// Tries to clean the files under the <paramref name="directoryPath"/> that match the given <paramref name="filter"/>.
        /// </summary>
        /// <param name="directoryPath">The path of the directory to clean.</param>
        /// <param name="filter">The filter that must match for files that should be deleted.</param>
        /// <exception cref="ArgumentNullException"><paramref name="filter"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="directoryPath"/> is <b>null</b>, empty or contains only white spaces.</exception>
        /// <returns>The names of the deleted files. An empty array if no files were deleted.</returns>
        string[] TryToCleanDirectory([NotNull] string directoryPath,
                                     [NotNull] string filter);

        string CopyFiles([NotNull] string sourceDirectory,
                         [NotNull] string targetDirectory,
                         [NotNull] string searchPattern);

        string CopyFile([NotNull] string source,
                        [NotNull] string target);

        string[] GetDirectoriesIn([NotNull] string directory);

        string[] GetFilesIn([NotNull] string directory,
                            [NotNull] string filter);

        void OpenUrl(string url);
    }
}