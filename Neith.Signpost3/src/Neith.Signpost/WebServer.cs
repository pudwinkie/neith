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
        private void StartupCode()
        {
            var uri = new Uri("http://localhost:14080/");
            Hosts = new List<ServiceHost>();
            var host = new DomainServiceHost(typeof(SignpostService), uri);
            host.Description.Behaviors.Remove<AspNetCompatibilityRequirementsAttribute>();
            host.BeginOpen(ac =>
            {
                try {
                    host.EndOpen(ac);
                    Debug.WriteLine("host[{0}] Open.. Status={1}, SingletonInstance={2}",
                        host.BaseAddresses.FirstOrDefault(),
                        host.State, host.SingletonInstance);
                }
                catch (Exception ex) {
                    var buf = new StringBuilder();
                    buf.AppendLine(ex.ToString());
                    

                    //Environment.UserName;

                    Debug.WriteLine(buf.ToString());
                }
            }, null);
            Hosts.Add(host);
        }
    }
}
