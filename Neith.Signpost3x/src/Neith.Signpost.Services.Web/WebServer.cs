using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.DomainServices.Hosting;
using System.Threading.Tasks;
using Neith.Signpost.Services;
using System.ServiceModel.Description;

namespace Neith.Signpost
{
    /// <summary>
    /// Webサーバ。
    /// </summary>
    public class WebServer : IDisposable
    {
        internal List<ServiceHost> Hosts { get; private set; }

        private const int Port = 14080;

        private static readonly string BaseUrlString = string.Format("http://localhost:{0}/", Port);

        private static readonly Uri BaseUri = new Uri(BaseUrlString);
        private static readonly Uri ServiceUrl = new Uri(BaseUrlString + "ClientBin/" + SignpostContext.ServiceNameString);

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

        private class MyDomainFactory : DomainServiceHostFactory
        {
            public ServiceHost CreateSignpostServiceHost(params Uri[] baseAddresses)
            {
                var host = CreateServiceHost(typeof(SignpostService), baseAddresses);
                host.Description.Behaviors.Remove<AspNetCompatibilityRequirementsAttribute>();
                return host;
            }
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
                    var fact = new MyDomainFactory();
                    var host = fact.CreateSignpostServiceHost(ServiceUrl);
                    Hosts.Add(host);
                    Debug.WriteLine("## host setting end ##");
                }
                {
                    Debug.WriteLine("## host setting start ##");
                    var host = new ServiceHost(typeof(StaticContentsService), BaseUri);
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
                    BaseUri.Scheme, BaseUri.Port, Environment.UserName));
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
