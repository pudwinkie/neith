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

namespace Neith.Signpost
{
    /// <summary>
    /// Webサーバ。
    /// </summary>
    public class WebServer : IDisposable
    {
        public List<ServiceHost> Hosts { get; private set; }

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
            TaskEx.Run(StartupCode);
        }

        /// <summary>
        /// 非同期スタートアップコードの実行。
        /// </summary>
        private async void StartupCode()
        {
            var uri = new Uri("http://localhost:14080/");
            Hosts = new List<ServiceHost>();
            var host = new DomainServiceHost(typeof(SignpostService), uri);
            Hosts.Add(host);
            host.Description.Behaviors.Remove<AspNetCompatibilityRequirementsAttribute>();
            try {
                await host.OpenAsync();
                Debug.WriteLine("host[{0}] Open.. Status={1}, SingletonInstance={2}",
                    host.BaseAddresses.FirstOrDefault(),
                    host.State, host.SingletonInstance);
            }
            catch (AddressAccessDeniedException aadEx) {
                var buf = new StringBuilder();
                buf.AppendLine(aadEx.ToString());
                buf.AppendLine();
                buf.AppendLine("----* 以下のコマンドを管理権限で実行してください *----");
                buf.AppendLine(string.Format(
                    "netsh http add urlacl url={0}://+:{1}/ user={2}",
                    uri.Scheme, uri.Port, Environment.UserName));
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
