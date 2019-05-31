namespace SSDTLifecycleExtension.DataAccess
{
    using Annotations;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.VisualStudio.Shell;

    [UsedImplicitly]
    public class VisualStudioAccess : IVisualStudioAccess
    {
        private readonly DTE2 _dte2;

        public VisualStudioAccess(DTE2 dte2)
        {
            _dte2 = dte2;
        }

        Project IVisualStudioAccess.GetSelectedProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_dte2.SelectedItems.Count != 1)
                return null;

            return _dte2.SelectedItems.Item(1).Project;
        }
    }
}