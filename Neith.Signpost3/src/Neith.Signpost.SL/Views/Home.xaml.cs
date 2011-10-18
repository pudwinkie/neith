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
using System.Windows.Navigation;
using System.Threading.Tasks;
using Neith.Signpost.Services;

namespace Neith.Signpost
{
    public partial class Home : Page
    {
        public Home()
        {
            InitializeComponent();
        }

        private void BeginInvoke(Action act)
        {
            Dispatcher.BeginInvoke(act);
        }


        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void tbTest_Loaded(object sender, RoutedEventArgs e)
        {
            var ctrl = sender as TextBox;
            TaskEx.Run(() => AsyncJob(ctrl));
        }

        private async void AsyncJob(TextBox ctrl)
        {
            try {
                var ch = await Channels.GetSignpostChannelAsync();
                var time = await ch.GetServerTimeAsync();
                BeginInvoke(() => ctrl.Text = string.Format("server time={0}", time));
            }
            catch (Exception ex) {
                BeginInvoke(() =>
                {
                    var win = new ErrorWindow(ex);
                    win.Show();
                });
            }
        }


    }
}
