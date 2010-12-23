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
        public LogService LogService { get; private set; }

        private void InitService()
        {
            lock (this) {
                CloseService();
                LogService = new LogService();
            }
        }

        private void CloseService()
        {
            lock (this) {
                if (LogService == null) return;
                LogService.Dispose();
                LogService = null;
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            InitService();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            CloseService();
        }

        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            CloseService();
        }


    }
}