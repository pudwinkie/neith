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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Neith.Signpost.Services;

namespace Neith.Signpost.SL
{
    public partial class Home : Page
    {
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
            var ctrl = sender as TextBlock;
            try {
                var sv = new SignpostContext();
                sv.GetServerTime(rc =>
                {
                    if (rc.IsComplete) ctrl.Text = string.Format("サーバ時刻={0}", rc.Value);
                    if (rc.HasError) {
                        ctrl.Text = rc.Error.ToString();
                        rc.MarkErrorAsHandled();
                    }
                }, null);
            }
            catch (Exception ex) {
                ctrl.Text = ex.ToString();
            }
        }
    }
}