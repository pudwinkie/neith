using System;
using System.Text;
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
                var sv = SignpostContext.Create();
                sv.GetServerTime(rc =>
                {
                    if (rc.IsComplete) ctrl.Text = string.Format("サーバ時刻={0}", rc.Value);
                    if (rc.HasError) {
                        WriteError(ctrl, rc.Error);
                        rc.MarkErrorAsHandled();
                    }
                }, null);
            }
            catch (Exception ex) {
                WriteError(ctrl, ex);
            }
        }

        private static void WriteError(TextBlock ctrl, Exception ex)
        {
            var buf = new StringBuilder();
            buf.AppendLine("---* 例外 *---");
            buf.AppendLine(ex.ToString());
            buf.AppendLine();
            buf.AppendLine("---* 設定情報 *---");
            buf.AppendLine("SERVICE URI:" + SignpostContext.ServiceUrl);
            ctrl.Text = buf.ToString();
        }
    }
}