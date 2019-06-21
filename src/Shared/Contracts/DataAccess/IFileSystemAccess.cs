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
        [NotNull]
        SecureResult<TResult> ReadFromStream<TResult>([NotNull] string sourcePath,
                                                      [NotNull] Func<Stream, TResult> streamConsumer)
            where TResult : class;

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