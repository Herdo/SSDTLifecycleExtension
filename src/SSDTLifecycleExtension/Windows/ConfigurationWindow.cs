namespace SSDTLifecycleExtension.Windows
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;

    [Guid("ee4cb0d9-81f5-408a-9867-e7c89f6b59d2")]
    [ExcludeFromCodeCoverage] // Test would require a UI thread.
    public class ConfigurationWindow : ToolWindowPane
    {
        public ConfigurationWindow() : base(null)
        {
            SetCaption(null);
            Content = new ConfigurationWindowControl();
        }

        internal void SetCaption(string projectName)
        {
            Caption = projectName == null
                          ? "Configuration"
                          : $"Configuration: {projectName}";
        }
    }
}
