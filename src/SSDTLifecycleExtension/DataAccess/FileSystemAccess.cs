namespace SSDTLifecycleExtension.DataAccess
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Annotations;

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
            using (var stream = new FileStream(targetPath, FileMode.OpenOrCreate))
                using (var writer = new StreamWriter(stream))
                    await writer.WriteAsync(content);
        }
    }
}