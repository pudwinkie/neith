using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Runtime.Serialization;

namespace Neith.Signpost.Proxy
{
    [Serializable]
    public class SignpostProxy : MarshalByRefObject, ISignpostServer
    {
        /// <summary>
        /// リモート通信チャネルを取得又は設定します。
        /// </summary>
        public TcpServerChannel Channel { get { return _Channel; } internal set { _Channel = value; } }
        [NonSerialized]
        private TcpServerChannel _Channel;



        /// <summary>
        /// クライアント通信が生きているならtrueを返します。
        /// 死んでいる場合は例外、あるいはfalseを返します。
        /// </summary>
        /// <returns>常にtrueを返す。</returns>
        public bool IsAlive()
        {
            return true;
        }


        public event PropertyChangedEventHandler PropertyChanged;

    }
}