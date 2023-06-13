namespace SSDTLifecycleExtension.Windows;

[ExcludeFromCodeCoverage] // Test would require a UI thread.
public partial class ScriptCreationWindowControl : IView
{
    public ScriptCreationWindowControl()
    {
        InitializeComponent();
    }

    void IView.SetDataContext(IViewModel viewModel)
    {
        DataContext = viewModel;
    }
}