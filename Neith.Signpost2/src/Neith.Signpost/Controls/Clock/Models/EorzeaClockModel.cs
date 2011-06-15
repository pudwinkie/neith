using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ReactiveUI;

namespace Neith.Signpost
{
    public class EorzeaClockModel : ReactiveValidatedObject
    {
        #region 定数
        /// <summary>基準時刻</summary>
        public static readonly DateTimeOffset StartTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        /// <summary>地球の１日</summary>
        public const long RATIO_EARTH = 24 * 60 / 10;

        /// <summary>現地の１日</summary>
        public const long RATIO_GAME = 70 / 10;

        #endregion
        #region プロパティ
        /// <summary>地球時間</summary>
        [Required]
        public DateTimeOffset EarthTime
        {
            get { return _EarthTime; }
            set { this.RaiseAndSetIfChanged(a => a.EarthTime, value); }
        }
        private DateTimeOffset _EarthTime = default(DateTimeOffset);

        /// <summary>基準時を０とした時の内部経過秒数</summary>
        public long TotalSecond
        {
            get { return _TotalSecond; }
            set { this.RaiseAndSetIfChanged(a => a.TotalSecond, value); }
        }
        private long _TotalSecond = 0;


        /// <summary>年</summary>
        public int Year
        {
            get { return _Year; }
            set { this.RaiseAndSetIfChanged(a => a.Year, value); }
        }
        private int _Year = 0;


        /// <summary>月</summary>
        public int Month
        {
            get { return _Month; }
            set { this.RaiseAndSetIfChanged(a => a.Month, value); }
        }
        private int _Month = 0;


        /// <summary>日</summary>
        public int Day
        {
            get { return _Day; }
            set { this.RaiseAndSetIfChanged(a => a.Day, value); }
        }
        private int _Day = 0;


        /// <summary>時</summary>
        public int Hour
        {
            get { return _Hour; }
            set { this.RaiseAndSetIfChanged(a => a.Hour, value); }
        }
        private int _Hour = 0;


        /// <summary>分</summary>
        public int Minute
        {
            get { return _Minute; }
            set { this.RaiseAndSetIfChanged(a => a.Minute, value); }
        }
        private int _Minute = 0;


        /// <summary>秒</summary>
        public int Second
        {
            get { return _Second; }
            set { this.RaiseAndSetIfChanged(a => a.Second, value); }
        }
        private int _Second = 0;


        /// <summary>月齢</summary>
        public int Moon
        {
            get { return _Moon; }
            set { this.RaiseAndSetIfChanged(a => a.Moon, value); }
        }
        private int _Moon = 0;

        #region 次の時間（地球時間）
        /// <summary>NextTime：次にNMがポップする時刻（地球時間）</summary>
        public DateTimeOffset NMPopTime
        {
            get { return _NMPopTime; }
            private set { this.RaiseAndSetIfChanged(a => a.NMPopTime, value); }
        }
        private DateTimeOffset _NMPopTime = DateTimeOffset.MinValue;

        /// <summary>NextTime：月齢が変わる時刻（地球時間）</summary>
        public DateTimeOffset NextChangeMoonTime
        {
            get { return _NextChangeMoonTime; }
            private set { this.RaiseAndSetIfChanged(a => a.NextChangeMoonTime, value); }
        }
        private DateTimeOffset _NextChangeMoonTime = DateTimeOffset.MinValue;

        /// <summary>NextTime：次の新月（地球時間）</summary>
        public DateTimeOffset NextNewMoonTime
        {
            get { return _NextNewMoonTime; }
            private set { this.RaiseAndSetIfChanged(a => a.NextNewMoonTime, value); }
        }
        private DateTimeOffset _NextNewMoonTime = DateTimeOffset.MinValue;

        /// <summary>NextTime：次の満月（地球時間）</summary>
        public DateTimeOffset NextFullMoonTime
        {
            get { return _NextFullMoonTime; }
            private set { this.RaiseAndSetIfChanged(a => a.NextFullMoonTime, value); }
        }
        private DateTimeOffset _NextFullMoonTime = DateTimeOffset.MinValue;

        #endregion
        #endregion
        public EorzeaClockModel()
        {
            this.PropertyChanged += EorzeaClockModel_PropertyChanged;
        }

        public EorzeaClockModel(DateTimeOffset time)
            : this()
        {
            EarthTime = time;
        }

        void EorzeaClockModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case "EarthTime":
                    UpdateEarthTime(EarthTime);
                    break;

                case "TotalSecond":
                    UpdateTotalSecond();
                    break;

            }
        }

        private void UpdateEarthTime(DateTimeOffset earth)
        {
            var span = earth - StartTime;
            var tick = span.Ticks;
            TotalSecond = tick * RATIO_EARTH / RATIO_GAME / TimeSpan.TicksPerSecond;
            // 次の時間設定
            NMPopTime = GetNMPopTime();
            NextChangeMoonTime = GetChangeMoonTime();
            var moon = GetNextNewFullMoon();
            NextNewMoonTime = moon.Item1;
            NextFullMoonTime = moon.Item2;
        }

        private void UpdateTotalSecond()
        {
            Second = (int)(TotalSecond % 60);
            Minute = (int)(TotalSecond / 60 % 60);
            Hour = (int)(TotalSecond / (60 * 60) % 24);
            Day = (int)(TotalSecond / (60 * 60 * 24) % 32 + 1);
            Month = (int)(TotalSecond / (60 * 60 * 24 * 32) % 12 + 1);
            Year = (int)(TotalSecond / (60 * 60 * 24 * 32 * 12));

            Moon = (int)(TotalSecond / (60 * 60 * 24 * 4) % 8);
        }

        /// <summary>
        /// 次に更新を行うべき時刻を返します。
        /// </summary>
        /// <param name="secSpan">現地時間での更新間隔（単位：秒）</param>
        /// <returns></returns>
        public DateTimeOffset GetNextUpdateTime(int secSpan)
        {
            return GetNextUpdateTime(secSpan, 1);
        }
        public DateTimeOffset GetNextUpdateTime(int secSpan, int count)
        {
            var nextSec = (TotalSecond / secSpan + count) * secSpan;
            return ToDateTime(nextSec);
        }

        /// <summary>
        /// 現地時刻の累積秒を地球時刻に変換します。
        /// </summary>
        /// <param name="totalSec"></param>
        /// <returns></returns>
        public static DateTimeOffset ToDateTime(long totalSec)
        {
            var tick = totalSec * TimeSpan.TicksPerSecond * RATIO_GAME / RATIO_EARTH;
            return StartTime + TimeSpan.FromTicks(tick);
        }

        /// <summary>
        /// 次に月齢が変わる時刻を返します。
        /// </summary>
        /// <returns></returns>
        private DateTimeOffset GetChangeMoonTime()
        {
            return GetNextUpdateTime(60 * 60 * 24 * 4);
        }

        /// <summary>
        /// 次の新月・満月の時刻を返します。
        /// </summary>
        /// <returns>次の新月・満月の時刻</returns>
        private Tuple<DateTimeOffset, DateTimeOffset> GetNextNewFullMoon()
        {
            var m1 = GetNextUpdateTime(60 * 60 * 24 * 16, 1);
            var m2 = GetNextUpdateTime(60 * 60 * 24 * 16, 2);
            var m3 = GetNextUpdateTime(60 * 60 * 24 * 32);
            return new Tuple<DateTimeOffset, DateTimeOffset>(m3, m1 == m3 ? m2 : m1);
        }

        /// <summary>
        /// 次にNMが沸く時刻を返します。地球時間で正５分間隔。
        /// </summary>
        /// <param name="earth"></param>
        /// <returns></returns>
        private DateTimeOffset GetNMPopTime()
        {
            var ratio = TimeSpan.TicksPerMinute * 5;
            var ticks = (EarthTime.Ticks / ratio + 1) * ratio;
            return new DateTimeOffset(ticks, EarthTime.Offset);
        }

    }
}