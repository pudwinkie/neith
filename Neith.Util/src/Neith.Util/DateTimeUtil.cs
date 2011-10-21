using System;
using System.Collections.Generic;
using System.Text;

namespace Neith.Util
{
    /// <summary>
    /// 時刻ユーティリティ。
    /// </summary>
    public static class DateTimeUtil
    {
        /// <summary>
        /// DateTime.UtcNowを返します。但し前回と同じか過去の時刻だった場合に、
        /// 前回返した時刻に1tick加算した価を返し、ユニークであることを保障します。
        /// </summary>
        /// <returns></returns>
        public static DateTime GetUniqueTimeStamp()
        {
            return GetUniqueTimeStampImpl.Create();
        }
        #region 実装
        /// <summary>
        /// 必ず前回より大きい時刻を返すことを保障するタイムスタンプ発行機の実装。
        /// </summary>
        private static class GetUniqueTimeStampImpl
        {
            private static DateTime last = DateTime.MinValue;

            public static DateTime Create()
            {
                DateTime rc = DateTime.UtcNow;
                lock (typeof(GetUniqueTimeStampImpl)) {
                    if (last >= rc) {
                        rc = last.AddTicks(1);
                    }
                    last = rc;
                }
                return rc;
            }
        }
        #endregion

    }

}
