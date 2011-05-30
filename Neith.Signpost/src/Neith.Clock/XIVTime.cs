using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Clock
{
    /// <summary>
    /// 時計換算モデル
    /// </summary>
    public class XIVTime
    {
        private static readonly DateTimeOffset ZeroTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly long ZeroTimeTick = ZeroTime.Ticks;

        private const long RATIO_EARTH = 24 * 6;
        private const long RATIO_XIV = 7;

        /// <summary>年を設定・取得します。</summary>
        public int Year { get; set; }
        /// <summary>月を設定・取得します。</summary>
        public int Month { get; set; }
        /// <summary>日を設定・取得します。</summary>
        public int Day { get; set; }
        /// <summary>時を設定・取得します。</summary>
        public int Hour { get; set; }
        /// <summary>分を設定・取得します。</summary>
        public int Minute { get; set; }
        /// <summary>秒を設定・取得します。</summary>
        public int Second { get; set; }

        /// <summary>基準時よりの経過秒を設定・取得します。</summary>
        public long TotalSecond
        {
            get
            {
                return Second
                    + 60 * Minute
                    + 60 * 60 * Hour
                    + 60 * 60 * 24 * (Day - 1)
                    + 60 * 60 * 24 * 32 * (Month - 1)
                    + 60 * 60 * 24 * 32 * 12 * Year;
            }
            set
            {
                Second = (int)(value % 60);
                Minute = (int)(value / (60) % 60);
                Hour = (int)(value / (60 * 60) % 24);
                Day = (int)(value / (60 * 60 * 24) % 32) + 1;
                Month = (int)(value / (60 * 60 * 24 * 32) % 12) + 1;
                Year = (int)(value / (60 * 60 * 24 * 32 * 12));
            }
        }

        /// <summary>
        /// 現在設定されているXIV日時情報を、地球時間(UTC)に換算します。
        /// </summary>
        /// <returns></returns>
        public DateTimeOffset ToEarth()
        {
            var tick = TotalSecond * TimeSpan.TicksPerSecond;
            tick = tick * RATIO_XIV / RATIO_EARTH;
            return new DateTimeOffset(ZeroTimeTick + tick, TimeSpan.Zero);
        }

        /// <summary>
        /// 地球時間を設定します。
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public long SetEarth(DateTimeOffset time)
        {
            var tick = time.Ticks - ZeroTimeTick;
            tick = tick * RATIO_EARTH / RATIO_XIV;
            var rc =  tick / TimeSpan.TicksPerSecond;
            TotalSecond = rc;
            return rc;
        }

    }
}
