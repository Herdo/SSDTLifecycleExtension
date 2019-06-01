namespace SSDTLifecycleExtension.Windows
{
    using ViewModels;

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
}