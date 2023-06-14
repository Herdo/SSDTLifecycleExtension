namespace SSDTLifecycleExtension.Windows;

[Guid("6e00e764-f71a-438f-84fe-5fe986514012")]
[ExcludeFromCodeCoverage] // Test would require a UI thread.
public class ScriptCreationWindow : ToolWindowPane, IVisualStudioToolWindow
{
    public ScriptCreationWindow() : base(null)
    {
        SetCaption(null);
        Content = new ScriptCreationWindowControl();
    }

    public ScriptCreationWindow(string message)
        : this()
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));
    }

    public void SetCaption(string projectName)
    {
        Caption = projectName == null
            ? "Script Creation"
            : $"Script Creation: {projectName}";
    }
}