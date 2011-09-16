using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Services;
using System.Runtime.Serialization.Formatters;
using System.Text;

namespace Neith.Signpost.Proxy
{
    public static class ProxyService
    {
        #region スタティック領域
        #region 定数定義
        public static string ChannelName { get { return Properties.Settings.Default.ChannelName; } }
        public static string ServiceName { get { return typeof(SignpostProxy).Name; } }
        public static string ChannelUrl { get { return "http://" + ChannelName; } }
        public static string ServiceUrl { get { return ChannelUrl + "/" + ServiceName; } }
        private static TimeSpan AliveCheckSpan = TimeSpan.FromSeconds(20);// 状態チェック間隔
        private const int ConnectionTimeOut = 1000 * 2;// コネクションタイムアウト


        #endregion
        #region 通信サービス静的メソッド
        /// <summary>
        /// Webサービスに接続します。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static ISignpostServiceChannel GetRestChannel(string uriString)
        {
            var uri = new Uri(uriString + "/rest");
            var factory = new WebChannelFactory<ISignpostServiceChannel>(uri);
            return factory.CreateChannel();
        }

        /// <summary>
        /// Webサービスに接続します。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static ISignpostServiceChannel GetWSChannel(string uriString)
        {
            var uri = new Uri(uriString + "/ws");
            var bind = new BasicHttpBinding();
            var ep = new EndpointAddress(uri);
            var factory = new ChannelFactory<ISignpostServiceChannel>(bind, ep);
            return factory.CreateChannel();
        }

        #endregion
        #endregion
    }
}
