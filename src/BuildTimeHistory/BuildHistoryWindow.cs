using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace BuildTimeHistory
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
    [Guid("6d248884-4d83-4697-860c-06cc61411a1f")]
    public class BuildHistoryWindow : ToolWindowPane
    {
        public BuildHistoryWindow() : base(null)
        {
            this.Caption = "Build History";

            this.Content = new BuildHistoryWindowControl();
        }
    }
}
