#nullable enable

namespace SSDTLifecycleExtension.Windows;

public interface IView
{
    void SetDataContext(IViewModel viewModel);
}