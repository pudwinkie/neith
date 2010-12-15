using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FF14LogViewer
{
internal static class Program
{
    // Methods
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new LogViewerForm());
    }
}

}
