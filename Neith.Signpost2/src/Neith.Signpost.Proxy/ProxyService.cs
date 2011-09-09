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


        /// <summary>
        /// サーバチャンネルを作成します。
        /// </summary>
        /// <returns></returns>
        private static TcpServerChannel CreateServerChannel()
        {
            // チャンネル設定の作成
            var conf = GetChannelConfig();
            // チャンネルシンク
            var serverProvider = new BinaryServerFormatterSinkProvider();
            serverProvider.TypeFilterLevel = TypeFilterLevel.Full;
            // チャンネル作成
            var ch = new TcpServerChannel(conf, serverProvider);
            ChannelServices.RegisterChannel(ch, false);
            return ch;
        }

        /// <summary>
        /// サーバ接続を作成します。
        /// 接続情報を返します。
        /// </summary>
        /// <param name="realData"></param>
        /// <returns></returns>
        public static TcpServerChannel Listen(SignpostProxy proxy)
        {
            /// チャンネルの作成
            var ch = CreateServerChannel();

            // remoteObjの公開
            RemotingServices.Marshal(proxy, ServiceName);
            Debug.WriteLine(string.Format("objurl: {0}", RemotingServices.GetObjectUri(proxy)));

            // リッスン開始
            ch.StartListening(null);
#if DEBUG
            Debug.Write("url: ");
            foreach (string url in ChannelServices.GetUrlsForObject(proxy)) {
                Debug.WriteLine(url);
                break;
            }
#endif
            return ch;
        }


        /// <summary>
        /// プロキシをアクティベートします。
        /// </summary>
        /// <param name="ctl"></param>
        /// <returns></returns>
        public static ISignpostServer Connect()
        {
            try {
                Debug.WriteLine("activate url: " + ServiceUrl);
                return (ISignpostServer)Activator.GetObject(typeof(ISignpostServer), ServiceUrl);
            }
            catch (Exception ex) {
                Trace.WriteLine(ex);
                throw ex;
            }
        }


        #endregion
        #endregion
    }
}
