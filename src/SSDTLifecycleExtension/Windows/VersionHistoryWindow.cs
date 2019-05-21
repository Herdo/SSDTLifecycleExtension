using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace SSDTLifecycleExtension.Windows
{
    [Guid("9d99a573-92d5-4bec-af64-88eb26ce12f7")]
    public class VersionHistoryWindow : ToolWindowPane
    {
        public VersionHistoryWindow() : base(null)
        {
            this.Caption = "VersionHistoryWindow";

            this.Content = new VersionHistoryWindowControl();
        }
    }
}
