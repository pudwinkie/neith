using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Logger.Model
{
    /// <summary>
    /// 収集処理モジュールのインターフェース。
    /// </summary>
    public interface ICollector : ILoggerModule
    {
        /// <summary>
        /// 収集イベント。
        /// </summary>
        event LogEventHandler Collect;
    }

}
