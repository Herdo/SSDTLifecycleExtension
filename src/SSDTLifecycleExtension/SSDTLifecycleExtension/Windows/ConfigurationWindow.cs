using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace SSDTLifecycleExtension.Windows
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("ee4cb0d9-81f5-408a-9867-e7c89f6b59d2")]
    public class ConfigurationWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationWindow"/> class.
        /// </summary>
        public ConfigurationWindow() : base(null)
        {
            this.Caption = "ConfigurationWindow";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new ConfigurationWindowControl();
        }
    }
}
