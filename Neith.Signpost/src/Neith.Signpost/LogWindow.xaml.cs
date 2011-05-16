using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Neith.Logger.Model;

namespace Neith.Signpost
{
    /// <summary>
    /// LogWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class LogWindow : Window
    {
        #region 依存関係プロパティ

        /// <summary>Logコレクション</summary>
        public static readonly DependencyProperty NeithLogsProperty;

        static LogWindow()
        {
            try {
                NeithLogsProperty = DependencyProperty.Register("NeithLogs", typeof(NeithLogCollection), typeof(LogWindow),
                    new FrameworkPropertyMetadata(new NeithLogCollection()));
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>Logコレクション</summary>
        public NeithLogCollection NeithLogs { get { return (NeithLogCollection)GetValue(NeithLogsProperty); } }


        #endregion
        #region その他のフィールド
        private Blue.Windows.StickyWindow _stickyWindow;


        #endregion


        public LogWindow()
        {
            this.InitializeComponent();
            
            // オブジェクト作成に必要なコードをこの下に挿入します。
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _stickyWindow = new Blue.Windows.StickyWindow(this);
            _stickyWindow.StickToScreen = true;
            _stickyWindow.StickToOther = true;
            _stickyWindow.StickOnResize = true;
            _stickyWindow.StickOnMove = true;

        }
    }
}