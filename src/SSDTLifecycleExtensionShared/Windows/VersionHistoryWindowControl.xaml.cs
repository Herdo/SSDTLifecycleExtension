#nullable enable

namespace SSDTLifecycleExtension.Windows;

[ExcludeFromCodeCoverage] // Test would require a UI thread.
public partial class VersionHistoryWindowControl : IView
{
    public VersionHistoryWindowControl()
    {
        InitializeComponent();
    }

    void IView.SetDataContext(IViewModel viewModel)
    {
        DataContext = viewModel;
    }
}