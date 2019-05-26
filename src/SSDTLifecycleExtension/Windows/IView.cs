namespace SSDTLifecycleExtension.Windows
{
    using ViewModels;

    public interface IView
    {
        void SetDataContext(IViewModel viewModel);
    }
}