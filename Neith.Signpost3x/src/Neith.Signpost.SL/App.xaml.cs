using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Neith.Signpost.SL
{
    public partial class App : Application
    {
        public App()
        {
            this.Startup += this.Application_Startup;
            this.UnhandledException += this.Application_UnhandledException;

            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            this.RootVisual = new MainPage();
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // アプリケーションがデバッガーの外側で実行されている場合、ChildWindow コントロール
            // によって例外が報告されます。
            if (!System.Diagnostics.Debugger.IsAttached) {
                // メモ : これにより、アプリケーションは例外がスローされた後も実行され続け、例外は
                // ハンドルされません。 
                // 実稼動アプリケーションでは、このエラー処理は、Web サイトにエラーを報告し、
                // アプリケーションを停止させるものに置換される必要があります。
                e.Handled = true;
                ChildWindow errorWin = new ErrorWindow(e.ExceptionObject);
                errorWin.Show();
            }
        }
    }
}