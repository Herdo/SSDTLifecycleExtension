namespace SSDTLifecycleExtension.DataAccess
{
    using System.Threading.Tasks;
    using Annotations;
    using EnvDTE;

    public interface IVisualStudioAccess
    {
        Project GetSelectedProject();

        Task ClearSSDTLifecycleOutputAsync();

        Task WriteLineToSSDTLifecycleOutputAsync([NotNull] string message);

        void BuildProject([NotNull] Project project);
    }
}