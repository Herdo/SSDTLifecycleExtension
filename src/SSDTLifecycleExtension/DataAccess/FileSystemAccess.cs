namespace SSDTLifecycleExtension.DataAccess
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Annotations;
    using Microsoft.VisualStudio.Threading;
    using Microsoft.Win32;

    [UsedImplicitly]
    public class FileSystemAccess : IFileSystemAccess
    {
        async Task<string> IFileSystemAccess.ReadFileAsync(string sourcePath)
        {
            if (sourcePath == null)
                throw new ArgumentNullException(nameof(sourcePath));

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

        async Task IFileSystemAccess.WriteFileAsync(string targetPath,
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
            using (var stream = new FileStream(targetPath, FileMode.Create))
                using (var writer = new StreamWriter(stream))
                    await writer.WriteAsync(content);
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

        (string[] Result, string Error) IFileSystemAccess.SearchForFiles(Environment.SpecialFolder rootFolder,
                                                                         string subFolder,
                                                                         string searchPattern)
        {
            if (string.IsNullOrWhiteSpace(subFolder))
                throw new ArgumentException("Value cannot be null or white space.", nameof(subFolder));
            if (string.IsNullOrWhiteSpace(searchPattern))
                throw new ArgumentException("Value cannot be null or white space.", nameof(searchPattern));

            try
            {
                var sd = Environment.GetFolderPath(rootFolder);
                var searchRoot = Path.Combine(sd, subFolder);
                return (Directory.GetFiles(searchRoot, searchPattern, SearchOption.AllDirectories), null);
            }
            catch (Exception e)
            {
                return (null, e.Message);
            }
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

        async Task<string> IFileSystemAccess.StartProcessAndWaitAsync(string fileName,
                                                                      string arguments,
                                                                      Func<string, Task> outputDataHandler,
                                                                      Func<string, Task> errorDataHandler,
                                                                      CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Value cannot be null or white space.", nameof(fileName));
            if (string.IsNullOrWhiteSpace(arguments))
                throw new ArgumentException("Value cannot be null or white space.", nameof(arguments));

            var psi = new ProcessStartInfo(fileName, arguments)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            var p = new Process
            {
                StartInfo = psi
            };
            if (outputDataHandler != null)
                p.OutputDataReceived += async (sender,
                                               args) => await outputDataHandler.Invoke(args.Data);
            if (errorDataHandler != null)
                p.ErrorDataReceived += async (sender,
                                              args) => await errorDataHandler.Invoke(args.Data);
            try
            {
                p.Start();
                if (outputDataHandler != null)
                    p.BeginOutputReadLine();
                if (errorDataHandler != null)
                    p.BeginErrorReadLine();
                await p.WaitForExitAsync(cancellationToken);
                return null;
            }
            catch (Exception e)
            {
                return e.Message;
            }
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
            return Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly);
        }
    }
}