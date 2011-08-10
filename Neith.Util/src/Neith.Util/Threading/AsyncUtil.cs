﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Neith.Util.Threading
{
    /// <summary>
    /// 非同期呼び出しユーティリティ関数。
    /// </summary>
    public static class AsyncUtil
    {
        /// <summary>
        /// カレントスレッドプールで非同期に関数を実行します。
        /// </summary>
        /// <param name="start"></param>
        public static void Call(ThreadStart start)
        {
            start.BeginInvoke(delegate(IAsyncResult ar) { return; }, null);
        }
    }
}
