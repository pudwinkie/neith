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

namespace Neith.Signpost
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Environment.Is64BitProcess) this.MenuItem2.Header = "64bit Process";
            else this.MenuItem2.Header = "32bit Process";
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible) MenuItem1.Header = "画面を隠す";
            else MenuItem1.Header = "画面を表示";
        }

        private void MenuItem1_Click(object sender, RoutedEventArgs e)
        {
            if (!IsVisible) {
                // 表示する
                WindowState = System.Windows.WindowState.Normal;
                Visibility = System.Windows.Visibility.Visible;
            }
            else {
                // 隠す
                Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void MenuItem2_Click(object sender, RoutedEventArgs e)
        {
            var logWin = new LogWindow();
            logWin.Show();
        }

    }
}
