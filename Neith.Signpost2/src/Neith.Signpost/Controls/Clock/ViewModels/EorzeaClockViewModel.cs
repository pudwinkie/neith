using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using ReactiveUI;
using Neith.Util;

namespace Neith.Signpost
{
    public class EorzeaClockViewModel : ReactiveValidatedObject, IDisposable
    {
        #region フィールド
        IDisposable TaskPropertyChanged;
        IDisposable TaskTimer;


        #endregion
        #region プロパティ
        public EorzeaClockModel Model { get; protected set; }

        /// <summary>更新間隔（ゲーム内の秒）</summary>
        [Required]
        [Range(1,int.MaxValue)]
        public int SpanSecond
        {
            get { return _SpanSecond; }
            set { this.RaiseAndSetIfChanged(a => a.SpanSecond, value); }
        }
        private int _SpanSecond = 20;

        /// <summary>trueの時、定時更新を行ないます。trueの時はタイマー更新を行いません。</summary>
        [Required]
        public bool IsTimerUpdate
        {
            get { return _IsTimerUpdate; }
            set { this.RaiseAndSetIfChanged(a => a.IsTimerUpdate, value); }
        }
        private bool _IsTimerUpdate = false;


        #endregion
        #region 初期化・開放
        public EorzeaClockViewModel(EorzeaClockModel model)
        {
            Model = model;
            TaskPropertyChanged = Changed.Subscribe(args =>
            {
                switch (args.PropertyName) {
                    case "IsTimerUpdate": UpdateIsTimerUpdate(); break;
                    case "SpanSecond": UpdateSpanSecond(); break;
                }
            });
        }

        public EorzeaClockViewModel()
            : this(new EorzeaClockModel(DateTimeOffset.Now))
        {
        }

        public void Dispose()
        {
            ObjectUtil.CheckDispose(ref TaskTimer);
            ObjectUtil.CheckDispose(ref TaskPropertyChanged);
        }


        #endregion
        #region 通知処理
        private void UpdateIsTimerUpdate()
        {
            // 一度タイマーを消してタイマーが有効かどうかを判断
            ObjectUtil.CheckDispose(ref TaskTimer);
            if (!IsTimerUpdate) return;

            // 一定時間で更新を行うタイマーを作成。
            var nextTime = DateTimeOffset.MinValue;
            TaskTimer = Observable.Generate(
                DateTimeOffset.Now, t => true, t => DateTimeOffset.Now,
                t =>
                {
                    if (t >= nextTime) Model.EarthTime = t;
                    return t;
                },
                t =>
                {
                    nextTime = Model.GetNextUpdateTime(SpanSecond);
                    return nextTime;
                },
                DispatcherScheduler.Instance).Subscribe();
        }

        /// <summary>
        /// 現在時刻で更新します。
        /// </summary>
        /// <returns></returns>
        private DateTimeOffset UpdateNow()
        {
            Model.EarthTime = DateTimeOffset.Now;
            return Model.GetNextUpdateTime(SpanSecond);
        }


        private void UpdateSpanSecond()
        {
            var old = IsTimerUpdate;
            try {
                IsTimerUpdate = false;
            }
            finally {
                IsTimerUpdate = old;
            }
        }

        #endregion
    }
}