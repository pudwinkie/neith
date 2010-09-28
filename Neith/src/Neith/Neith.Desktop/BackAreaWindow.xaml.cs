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
using System.Windows.Shapes;
using Neith.Interop;

namespace Neith.Desktop
{
    /// <summary>
    /// BackAreaWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class BackAreaWindow : Window
    {
        public BackAreaWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            var area = this.GetScreenRect();
            Top = area.Top;
            Height = area.Height;
            Width = 320;
            Left = (double)area.Width - Width;
        }

    }
}
