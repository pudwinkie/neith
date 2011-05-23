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
        private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        public LogService LogService { get; private set; }

        private void InitService()
        {
            lock (this) {
                try {
                    Log.Info("アプリケーション初期化：開始");
                    CloseService();
                    LogService = new LogService();
                }
                finally {
                    Log.Trace("アプリケーション初期化：完了");
                }
            }
        }

        private void CloseService()
        {
            lock (this) {
                if (LogService == null) return;
                try {
                    Log.Trace("アプリケーション終了処理：開始");
                    LogService.Dispose();
                    LogService = null;
                }
                finally {
                    Log.Info("アプリケーション終了処理：完了");
                }
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