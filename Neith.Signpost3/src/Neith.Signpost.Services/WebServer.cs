using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Neith.Signpost.Services;
using System.ServiceModel.Description;

namespace Neith.Signpost.Services
{
    /// <summary>
    /// 組み込みWebサーバ
    /// </summary>
    public class WebServer : IDisposable
    {
        internal List<ServiceHost> Hosts { get; private set; }


        public void Dispose()
        {
            if (Hosts != null) {
                Hosts
                    .AsParallel()
                    .Where(a => a.State == CommunicationState.Opened)
                    .ForAll(a => a.Close());
            }
        }


        public WebServer()
        {
            TaskEx.Run(() =>
            {
                StartupDomainService();
            });
        }


        /// <summary>
        /// ドメインサービスの起動。
        /// </summary>
        private async void StartupDomainService()
        {
            try {
                Hosts = new List<ServiceHost>();
                {
                    Debug.WriteLine("## host setting start ##");
                    var host = new ServiceHost(typeof(SignpostService), Const.ServiceUrl);
                    host
                        .AddServiceEndpoint(typeof(ISignpostContext), new BasicHttpBinding(), "");
                    Hosts.Add(host);
                    Debug.WriteLine("## host setting end ##");
                }
                {
                    Debug.WriteLine("## host setting start ##");
                    var host = new ServiceHost(typeof(StaticContentsService), Const.BaseUri);
                    host
                        .AddServiceEndpoint(typeof(IStaticContents), new WebHttpBinding(), "")
                        .Behaviors.Add(new WebHttpBehavior());
                    Hosts.Add(host);
                    Debug.WriteLine("## host setting end ##");
                }
                foreach (var h in Hosts) {
                    Debug.WriteLine("## host[{0}] Open...");
                    await h.OpenAsync();
                    Debug.WriteLine("## host[{0}] Opened. Status={1}, SingletonInstance={2}",
                        h.BaseAddresses.FirstOrDefault(),
                        h.State, h.SingletonInstance);
                }
            }
            catch (AddressAccessDeniedException aadEx) {
                var buf = new StringBuilder();
                buf.AppendLine(aadEx.ToString());
                buf.AppendLine();
                buf.AppendLine("----* 以下のコマンドを管理権限で実行してください *----");
                buf.AppendLine(string.Format(
                    "netsh http add urlacl url={0}://+:{1}/ user={2}",
                    Const.BaseUri.Scheme, Const.BaseUri.Port, Environment.UserName));
                Debug.WriteLine(buf.ToString());
            }
            catch (Exception ex) {
                var buf = new StringBuilder();
                buf.AppendLine(ex.ToString());
                Debug.WriteLine(buf.ToString());
            }
        }
    }
}
