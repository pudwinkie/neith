using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Logger.Model
{
    /// <summary>
    /// 収集処理モジュールのインターフェース。
    /// </summary>
    public interface ICollector : IDisposable
    {
        /// <summary>モジュール名</summary>
        string Name { get; }

        /// <summary>
        /// 収集タスクを返す。
        /// </summary>
        /// <returns></returns>
        IObservable<Log> RxCollect();
    }

}
