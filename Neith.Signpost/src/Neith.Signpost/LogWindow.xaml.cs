using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                // ポップアップ入力モード
                NeithLogsProperty = DependencyProperty.Register("NeithLogs", typeof(bool), typeof(ObservableCollection<NeithLog>),
                    new FrameworkPropertyMetadata(new ObservableCollection<NeithLogVM>()));
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>Logコレクション</summary>
        public ObservableCollection<NeithLogVM> NeithLogs { get { return (ObservableCollection<NeithLogVM>)GetValue(NeithLogsProperty); } }


        #endregion

        public LogWindow()
        {
            this.InitializeComponent();
            
            // オブジェクト作成に必要なコードをこの下に挿入します。
        }
    }
}