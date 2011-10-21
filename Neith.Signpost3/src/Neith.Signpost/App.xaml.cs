using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Neith.Signpost.Services;

namespace Neith.Signpost
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        public static WebServer Server { get; private set; }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (Server != null) Server.Dispose();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Server = new WebServer();
        }

    }
}
