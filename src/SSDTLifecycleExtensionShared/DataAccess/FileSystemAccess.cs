namespace SSDTLifecycleExtension.DataAccess;

[UsedImplicitly]
[ExcludeFromCodeCoverage] // Test would require IO access.
public class FileSystemAccess : IFileSystemAccess
{
    private static async Task<string> ReadFileInternalAsync(string sourcePath)
    {
        using var stream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private static async Task WriteFileInternalAsync(string targetPath,
                                                     string content)
    {
        Stream stream = null;
        try
        {
            stream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = new StreamWriter(stream);
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
                return Array.Empty<string>();
            var children = di.EnumerateFileSystemInfos(filter).ToArray();
            if (children.Length == 0)
                return Array.Empty<string>();

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
                    return Array.Empty<string>();
                }
            }

            return deletedFiles.ToArray();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    Task<string> IFileSystemAccess.ReadFileAsync(string sourcePath)
    {
        if (sourcePath == null)
            throw new ArgumentNullException(nameof(sourcePath));

        return ReadFileInternalAsync(sourcePath);
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

    ((string Source, string Target)[] CopiedFiles, (string File, Exception Exception)[] Errors) IFileSystemAccess.CopyFiles(string sourceDirectory,
        string targetDirectory,
        string searchPattern)
    {
        if (string.IsNullOrWhiteSpace(sourceDirectory))
            throw new ArgumentException("Value cannot be null or white space.", nameof(sourceDirectory));
        if (string.IsNullOrWhiteSpace(targetDirectory))
            throw new ArgumentException("Value cannot be null or white space.", nameof(targetDirectory));
        if (string.IsNullOrWhiteSpace(searchPattern))
            throw new ArgumentException("Value cannot be null or white space.", nameof(searchPattern));

        var copiedFiles = new List<(string Source, string Target)>();
        var errors = new List<(string File, Exception Exception)>();
        string[] sourceFiles;
        try
        {
            sourceFiles = Directory.GetFiles(sourceDirectory, searchPattern, SearchOption.TopDirectoryOnly);
        }
        catch (Exception e)
        {
            errors.Add((null, e));
            return (copiedFiles.ToArray(), errors.ToArray());
        }

        foreach (var sourceFile in sourceFiles)
        {
            try
            {
                var fi = new FileInfo(sourceFile);
                var targetFilePath = Path.Combine(targetDirectory, fi.Name);
                fi.CopyTo(targetFilePath, true);
                copiedFiles.Add((sourceFile, targetFilePath));
            }
            catch (Exception e)
            {
                errors.Add((sourceFile, e));
            }
        }

        return (copiedFiles.ToArray(), errors.ToArray());
    }

    string IFileSystemAccess.CopyFile(string source,
                                      string target)
    {
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Value cannot be null or white space.", nameof(source));
        if (string.IsNullOrWhiteSpace(target))
            throw new ArgumentException("Value cannot be null or white space.", nameof(target));

        try
        {
            File.Copy(source, target, true);
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
            : Array.Empty<string>();
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
            : Array.Empty<string>();
    }

    void IFileSystemAccess.OpenUrl(string url)
    {
        var uri = new Uri(url);
        if (uri.Scheme != "http" && uri.Scheme != "https")
            return;
        System.Diagnostics.Process.Start(url);
    }
}