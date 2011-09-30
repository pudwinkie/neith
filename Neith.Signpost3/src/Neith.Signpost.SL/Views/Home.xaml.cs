﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading.Tasks;
using Neith.Signpost.Services;

namespace Neith.Signpost
{
    public partial class Home : Page
    {
        private ISignpostChannel ch;

        public Home()
        {
            InitializeComponent();
        }

        // ユーザーがこのページに移動したときに実行されます。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void ContentText_Loaded(object sender, RoutedEventArgs e)
        {
            var ctrl = sender as TextBox;
            TaskEx.Run(() => AsyncJob(ctrl));
        }

        private async void AsyncJob(TextBox ctrl)
        {
            try {
                if (ch == null) ch = await Channels.CreateSignpostChannelAsync();
                var time = await ch.GetServerTimeAsync();
                ctrl.Text = string.Format("server time={0}", time);
            }
            catch (Exception ex) {
                var win = new ErrorWindow(ex);
                win.Show();
            }
        }


    }
}