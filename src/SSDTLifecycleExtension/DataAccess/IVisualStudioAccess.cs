namespace SSDTLifecycleExtension.DataAccess
{
    using EnvDTE;

    public interface IVisualStudioAccess
    {
        Project GetSelectedProject();
    }
}