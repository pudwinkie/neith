using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Neith.Logger;

namespace Neith.Signpost
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private LogCtrl logCtrl = null;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            logCtrl = new LogCtrl();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (logCtrl != null) logCtrl.Dispose();
        }
    }
}
