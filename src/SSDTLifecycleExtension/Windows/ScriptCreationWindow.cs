namespace SSDTLifecycleExtension.Windows
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;

    [Guid("6e00e764-f71a-438f-84fe-5fe986514012")]
    [ExcludeFromCodeCoverage] // Test would require a UI thread.
    public class ScriptCreationWindow : ToolWindowPane
    {
        public ScriptCreationWindow() : base(null)
        {
            SetCaption(null);
            Content = new ScriptCreationWindowControl();
        }

        internal void SetCaption(string projectName)
        {
            Caption = projectName == null
                          ? "Script Creation"
                          : $"Script Creation: {projectName}";
        }
    }
}
