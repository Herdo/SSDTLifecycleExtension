namespace SSDTLifecycleExtension.Windows
{
    using ViewModels;

    public partial class ConfigurationWindowControl : IView
    {
        public ConfigurationWindowControl()
        {
            InitializeComponent();
        }

        void IView.SetDataContext(IViewModel viewModel)
        {
            DataContext = viewModel;
        }
    }
}