using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.DomainServices.Client;
using System.Text;
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
        private readonly Lazy<SignpostContext> sv
            = new Lazy<SignpostContext>(() => new SignpostContext());
        public SignpostContext SV { get { return sv.Value; } }

        public WebDomainClient<SignpostContext.ISignpostServiceContract> CL
        {
            get
            {
                return SV.DomainClient as WebDomainClient<SignpostContext.ISignpostServiceContract>;
            }
        }


        public Home()
        {
            InitializeComponent();
        }

        // ユーザーがこのページに移動したときに実行されます。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

#if false
        private void ContentText_Loaded(object sender, RoutedEventArgs e)
        {
            var ctrl = sender as TextBox;
            try {
                {
                    var buf = new StringBuilder();
                    buf.AppendLine("---* 呼び出し中... *---");
                    ctrl.Text = buf.ToString();
                }
                SV.GetServerTime(rc =>
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
#endif

        private void ContentText_Loaded(object sender, RoutedEventArgs e)
        {
            var ctrl = sender as TextBox;
            try {
                {
                    var buf = new StringBuilder();
                    buf.AppendLine("---* 呼び出し中... *---");
                    ctrl.Text = buf.ToString();
                }
                SV.GetBool(rc =>
                {
                    if (rc.IsComplete) ctrl.Text = string.Format("Result={0}", rc.Value);
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


        private void WriteError(TextBox ctrl, Exception ex)
        {
            var buf = new StringBuilder();
            buf.AppendLine("---* 例外 *---");
            buf.AppendLine(ex.ToString());
            buf.AppendLine();
            buf.AppendLine("---* 設定情報 *---");
            buf.AppendLine("SERVICE URI:" + CL.ServiceUri);
            ctrl.Text = buf.ToString();
        }
    }
}