namespace SSDTLifecycleExtension.DataAccess
{
    using System.Threading.Tasks;
    using Annotations;

    public interface IFileSystemAccess
    {
        Task<string> ReadFileAsync([NotNull] string sourcePath);

        Task WriteFileAsync([NotNull] string targetPath,
                            [NotNull] string content);
    }
}