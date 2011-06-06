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
using System.ComponentModel;

namespace Neith.Signpost
{
    /// <summary>
    /// EorzeaClock.xaml の相互作用ロジック
    /// </summary>
    public partial class EorzeaClock : UserControl
    {
        public EorzeaClock()
        {
            this.InitializeComponent();

            // オブジェクト作成に必要なコードをこの下に挿入します。
            var vm = LayoutRoot.DataContext as EorzeaClockViewModel;
            vm.IsTimerUpdate = !DesignerProperties.GetIsInDesignMode(this);
        }
    }
}