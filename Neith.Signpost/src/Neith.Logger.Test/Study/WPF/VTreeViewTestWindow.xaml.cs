using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Neith.Logger.Model;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace Neith.Logger.Test.Study.WPF
{
    /// <summary>
    /// VTreeViewTestWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class VTreeViewTestWindow : Window
    {
        #region 依存関係プロパティ

        /// <summary>Logコレクション</summary>
        public static readonly DependencyProperty NeithLogsProperty;

        static VTreeViewTestWindow()
        {
            try {
                NeithLogsProperty = DependencyProperty.Register("NeithLogs", typeof(NeithLogCollection), typeof(VTreeViewTestWindow),
                    new FrameworkPropertyMetadata(new NeithLogCollection()));
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>Logコレクション</summary>
        public NeithLogCollection NeithLogs
        {
            get { return (NeithLogCollection)GetValue(NeithLogsProperty); }
            private set { SetValue(NeithLogsProperty, value); }
        }


        #endregion

        public VTreeViewTestWindow()
        {
            InitializeComponent();
        }

        private Blue.Windows.StickyWindow _stickyWindow;

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            _stickyWindow = new Blue.Windows.StickyWindow(this);
            _stickyWindow.StickToScreen = true;
            _stickyWindow.StickToOther = true;
            _stickyWindow.StickOnResize = true;
            _stickyWindow.StickOnMove = true;

            var qItem = Enumerable.Range(0, 100)
                .Select(i =>
                {
                    var item = NeithLog.Create();
                    item.Host = "Host";
                    item.Application = "あぷりけ～しょんなのよ";
                    item.Message = string.Format("COUNT={0:0000}", i);
                    return item;
                });
            NeithLogs= new NeithLogCollection(qItem);
        }
    }
}
