namespace SSDTLifecycleExtension.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.Win32;
    using Shared.Contracts;
    using Shared.Contracts.DataAccess;

    [UsedImplicitly]
    [ExcludeFromCodeCoverage] // Test would require IO access.
    public class FileSystemAccess : IFileSystemAccess
    {
        private static async Task<string> ReadFileInternalAsync(string sourcePath)
        {
            try
            {
                using (var stream = new FileStream(sourcePath, FileMode.Open))
                    using (var reader = new StreamReader(stream))
                        return await reader.ReadToEndAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task WriteFileInternalAsync(string targetPath,
                                                         string content)
        {
            Stream stream = null;
            try
            {
                stream = new FileStream(targetPath, FileMode.Create);
                using (var writer = new StreamWriter(stream))
                    await writer.WriteAsync(content);
            }
            finally
            {
                stream?.Dispose();
            }
        }

        private static string[] TryToCleanDirectoryInternal(string directoryPath,
                                                            string filter)
        {
            try
            {
                var di = new DirectoryInfo(directoryPath);
                if (!di.Exists)
                    return new string[0];
                var children = di.EnumerateFileSystemInfos(filter).ToArray();
                if (children.Length == 0)
                    return new string[0];

                var deletedFiles = new List<string>();
                foreach (var child in children)
                {
                    // Inner try-catch to delete as much as possible
                    try
                    {
                        child.Delete();
                        deletedFiles.Add(child.Name);
                    }
                    catch
                    {
                        return new string[0];
                    }
                }

                return deletedFiles.ToArray();
            }
            catch
            {
                return new string[0];
            }
        }

        Task<string> IFileSystemAccess.ReadFileAsync(string sourcePath)
        {
            if (sourcePath == null)
                throw new ArgumentNullException(nameof(sourcePath));

            return ReadFileInternalAsync(sourcePath);
        }

        SecureStreamResult<TResult> IFileSystemAccess.ReadFromStream<TResult>(string sourcePath,
                                                                              Func<Stream, TResult> streamConsumer)
        {
            if (sourcePath == null)
                throw new ArgumentNullException(nameof(sourcePath));
            if (streamConsumer == null)
                throw new ArgumentNullException(nameof(streamConsumer));

            Stream stream = null;
            try
            {
                stream = new FileStream(sourcePath, FileMode.OpenOrCreate, FileAccess.Read);
            }
            catch (Exception e)
            {
                stream?.Dispose();
                return new SecureStreamResult<TResult>(null, null, e);
            }

            return new SecureStreamResult<TResult>(stream, streamConsumer(stream), null);
        }

        Task IFileSystemAccess.WriteFileAsync(string targetPath,
                                              string content)
        {
            if (targetPath == null)
                throw new ArgumentNullException(nameof(targetPath));
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            // Ensure the directory exists.
            var directory = Path.GetDirectoryName(targetPath);
            if (directory == null)
                throw new InvalidOperationException($"Cannot determine directory of '{nameof(targetPath)}'.");
            var di = new DirectoryInfo(directory);
            if (!di.Exists) di.Create();

            // Write the file.
            return WriteFileInternalAsync(targetPath, content);
        }

        string IFileSystemAccess.BrowseForFile(string extension,
                                               string filter)
        {
            if (extension == null)
                throw new ArgumentNullException(nameof(extension));
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            var ofd = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = extension,
                Multiselect = false,
                ValidateNames = true,
                Filter = filter
            };
            var result = ofd.ShowDialog();
            return result == true ? ofd.FileName : null;
        }

        bool IFileSystemAccess.CheckIfFileExists(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Value cannot be null or white space.", nameof(filePath));

            return File.Exists(filePath);
        }

        string IFileSystemAccess.EnsureDirectoryExists(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Value cannot be null or white space.", nameof(path));

            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return null;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        void IFileSystemAccess.TryToCleanDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentException("Value cannot be null or white space.", nameof(directoryPath));

            TryToCleanDirectoryInternal(directoryPath, "*");
        }

        string[] IFileSystemAccess.TryToCleanDirectory(string directoryPath,
                                                       string filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentException("Value cannot be null or white space.", nameof(directoryPath));

            return TryToCleanDirectoryInternal(directoryPath, filter);
        }

        string IFileSystemAccess.CopyFiles(string sourceDirectory,
                                           string targetDirectory,
                                           string searchPattern)
        {
            if (string.IsNullOrWhiteSpace(sourceDirectory))
                throw new ArgumentException("Value cannot be null or white space.", nameof(sourceDirectory));
            if (string.IsNullOrWhiteSpace(targetDirectory))
                throw new ArgumentException("Value cannot be null or white space.", nameof(targetDirectory));
            if (string.IsNullOrWhiteSpace(searchPattern))
                throw new ArgumentException("Value cannot be null or white space.", nameof(searchPattern));

            try
            {
                var sourceFiles = Directory.GetFiles(sourceDirectory, searchPattern, SearchOption.TopDirectoryOnly);
                foreach (var sourceFile in sourceFiles)
                {
                    var fi = new FileInfo(sourceFile);
                    fi.CopyTo(Path.Combine(targetDirectory, fi.Name), true);
                }

                return null;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        string[] IFileSystemAccess.GetDirectoriesIn(string directory)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            return Directory.Exists(directory)
                       ? Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly)
                       : new string[0];
        }

        string[] IFileSystemAccess.GetFilesIn(string directory,
                                              string filter)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            return Directory.Exists(directory)
                       ? Directory.GetFiles(directory, filter, SearchOption.TopDirectoryOnly)
                       : new string[0];
        }

        void IFileSystemAccess.OpenUrl(string url)
        {
            var uri = new Uri(url);
            if (uri.Scheme != "http" && uri.Scheme != "https")
                return;
            Process.Start(url);
        }
    }
}