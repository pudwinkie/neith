﻿using System;
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
        public static readonly DateTimeOffset StartTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        public const long EARTH_ONE_DAY = 24 * 60 / 10;
        public const long GAME_ONE_DAY = 70 / 10;

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
            TotalSecond = tick * EARTH_ONE_DAY / GAME_ONE_DAY / TimeSpan.TicksPerSecond;
        }

        private void UpdateTotalSecond()
        {
            Second = (int)(TotalSecond) % 60;
            Minute = (int)(TotalSecond / 60) % 60;
            Hour = (int)(TotalSecond / (60 * 60)) % 24;
            Day = (int)(TotalSecond / (60 * 60 * 24)) % 32 + 1;
            Month = (int)(TotalSecond / (60 * 60 * 24 * 32)) % 12 + 1;
            Year = (int)(TotalSecond / (60 * 60 * 24 * 32 * 12)) + 1;
        }

        public DateTimeOffset GetNextUpdateTime(int secSpan)
        {
            var nextSec = (TotalSecond / secSpan + 1) * secSpan;
            return ToDateTime(nextSec);
        }

        public static DateTimeOffset ToDateTime(long totalSec)
        {
            var tick = totalSec * TimeSpan.TicksPerSecond * GAME_ONE_DAY / EARTH_ONE_DAY;
            return StartTime + TimeSpan.FromTicks(tick);
        }




    }
}