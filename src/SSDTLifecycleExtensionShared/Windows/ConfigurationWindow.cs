#nullable enable

namespace SSDTLifecycleExtension.Windows;

[Guid("ee4cb0d9-81f5-408a-9867-e7c89f6b59d2")]
[ExcludeFromCodeCoverage] // Test would require a UI thread.
public class ConfigurationWindow : ToolWindowPane, IVisualStudioToolWindow
{
    public ConfigurationWindow() : base(null)
    {
        SetCaption(null);
        Content = new ConfigurationWindowControl();
    }

    public ConfigurationWindow(object context) : this()
    {
    }

    public void SetCaption(string? projectName)
    {
        Caption = projectName == null
            ? "Configuration"
            : $"Configuration: {projectName}";
    }
}