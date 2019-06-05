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

        void ShowModalError([NotNull] string error);

        void BuildProject([NotNull] Project project);

        Task StartLongRunningTaskIndicatorAsync();

        Task StopLongRunningTaskIndicatorAsync();
    }
}