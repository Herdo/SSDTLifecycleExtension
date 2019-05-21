using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace SSDTLifecycleExtension.Windows
{
    [Guid("6e00e764-f71a-438f-84fe-5fe986514012")]
    public class ScriptCreationWindow : ToolWindowPane
    {
        public ScriptCreationWindow() : base(null)
        {
            this.Caption = "ScriptCreationWindow";

            this.Content = new ScriptCreationWindowControl();
        }
    }
}
