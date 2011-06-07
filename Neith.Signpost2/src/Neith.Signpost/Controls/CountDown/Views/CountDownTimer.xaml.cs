﻿using System;
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

namespace Neith.Signpost
{
    /// <summary>
    /// CountDownTimer.xaml の相互作用ロジック
    /// </summary>
    public partial class CountDownTimer : UserControl
    {
        public CountDownTimer()
        {
            this.InitializeComponent();
            
            // オブジェクト作成に必要なコードをこの下に挿入します。
        }

        private void viewbox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var vm = LayoutRoot.DataContext as CountDownTimerViewModel;
            if (vm == null) return;
            switch (vm.Status) {
                case CountDownTimerStatus.Run:
                    vm.Status = CountDownTimerStatus.Pause;
                    break;
                default:
                    vm.Status = CountDownTimerStatus.Run;
                    break;
            }
        }

        private void imgReset_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var vm = LayoutRoot.DataContext as CountDownTimerViewModel;
            if (vm == null) return;
            vm.Status = CountDownTimerStatus.Reset;
        }
    }
}