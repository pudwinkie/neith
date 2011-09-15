using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public static string ChannelUrl { get { return "tcp://" + ChannelName; } }
        public static string ServiceUrl { get { return ChannelUrl + "/" + ServiceName; } }
        private static TimeSpan AliveCheckSpan = TimeSpan.FromSeconds(20);// 状態チェック間隔
        private const int ConnectionTimeOut = 1000 * 2;// コネクションタイムアウト


        #endregion
        #region 通信サービス静的メソッド
        /// <summary>
        /// チャンネル設定。
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, object> GetChannelConfig()
        {
            var conf = new Dictionary<string, object>();
            conf["name"] = string.Empty;
            conf["portName"] = ChannelName;
            conf["exclusiveAddressUse"] = true;
            conf["secure"] = false;
            conf["connectionTimeout"] = ConnectionTimeOut;
            return conf;
        }




        #endregion
        #endregion
    }
}
