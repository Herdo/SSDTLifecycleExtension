namespace SSDTLifecycleExtension.Windows;

[Guid("9d99a573-92d5-4bec-af64-88eb26ce12f7")]
[ExcludeFromCodeCoverage] // Test would require a UI thread.
public class VersionHistoryWindow : ToolWindowPane, IVisualStudioToolWindow
{
    public VersionHistoryWindow() : base(null)
    {
        SetCaption(null);
        Content = new VersionHistoryWindowControl();
    }

    public VersionHistoryWindow(string message)
        : this()
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));
    }

    public void SetCaption(string projectName)
    {
        Caption = projectName == null
            ? "Version History"
            : $"Version History: {projectName}";
    }
}