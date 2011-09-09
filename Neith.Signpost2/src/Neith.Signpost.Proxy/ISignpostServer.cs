using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Neith.Signpost.Proxy
{
    public interface ISignpostServer : INotifyPropertyChanged
    {
        /// <summary>
        /// クライアント通信が生きているならtrueを返します。
        /// 死んでいる場合は例外、あるいはfalseを返します。
        /// </summary>
        /// <returns>常にtrueを返す。</returns>
        bool IsAlive();

    }
}
