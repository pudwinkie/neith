using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Neith.Desktop
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private Window backAreaWindow;

        /// <summary>
        /// アプリケーション起動処理を呼び出します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppStartup(object sender, StartupEventArgs e)
        {
            backAreaWindow = new BackAreaWindow();
            backAreaWindow.Show();
        }

        /// <summary>
        /// アプリケーションの終了処理を呼び出します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppExit(object sender, ExitEventArgs e)
        {

        }

        /// <summary>
        /// OS終了イベントを処理します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppSessionEnding(object sender, SessionEndingCancelEventArgs e)
        {

        }
    }
}
