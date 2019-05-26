namespace SSDTLifecycleExtension.Windows
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using ViewModels;

    public partial class ConfigurationWindowControl : IView
    {
        public ConfigurationWindowControl()
        {
            InitializeComponent();
        }

        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", ToString()),
                "ConfigurationWindow");
        }

        void IView.SetDataContext(IViewModel viewModel)
        {
            DataContext = viewModel;
        }
    }
}