using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Logger
{
    /// <summary>
    /// ユニーク値を保証する現在時刻を作成します。
    /// </summary>
    public static class UniqueTime
    {
        /// <summary>
        /// 最新のUtc時刻を返します。
        /// 直前の値以下になる場合、1tick加算した値を返します。
        /// </summary>
        public static DateTime Now
        {
            get
            {
                var now = DateTime.UtcNow;
                lock (lockObj) {
                    if (now <= lastTime) now = lastTime + OneTick;
                    lastTime = now;
                }
                return now;
            }
        }

        private static readonly object lockObj = new object();
        private static readonly TimeSpan OneTick = TimeSpan.FromTicks(1);
        private static DateTime lastTime = DateTime.MinValue;

    }
}