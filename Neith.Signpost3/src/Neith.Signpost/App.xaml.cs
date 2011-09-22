using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.DomainServices.Hosting;
using Neith.Signpost.Services;

namespace Neith.Signpost
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        public List<ServiceHost> Hosts { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            TaskEx.Run(StartupCode);
        }

        /// <summary>
        /// 非同期スタートアップコードの実行。
        /// </summary>
        private void StartupCode()
        {
            var uri = new Uri("http://localhost:1414/");
            Hosts = new List<ServiceHost>();
            Hosts.Add(new DomainServiceHost(typeof(SignpostService), uri));
        }

    }
}
