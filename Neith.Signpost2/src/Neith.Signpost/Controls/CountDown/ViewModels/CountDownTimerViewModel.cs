using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using ReactiveUI;
using ReactiveUI.Xaml;
using Neith.Util;
using System.Windows.Input;

namespace Neith.Signpost
{
    public enum CountDownTimerStatus
    {
        Reset,
        Run,
        Pause,
        Fin,
    }

    public class CountDownTimerViewModel : ReactiveObject, IDisposable
    {
        #region フィールド
        IDisposable TaskPropertyChanged;
        IDisposable TaskTimer;

        #endregion
        #region プロパティ
        /// <summary>計測時間</summary>
        public TimeSpan Span { get { return _Span; } set { this.RaiseAndSetIfChanged(a => a.Span, value); } }
        private TimeSpan _Span = TimeSpan.Zero;

        /// <summary>ステータス</summary>
        public CountDownTimerStatus Status { get { return _Status; } set { this.RaiseAndSetIfChanged(a => a.Status, value); } }
        private CountDownTimerStatus _Status = CountDownTimerStatus.Reset;

        /// <summary>ステータステキスト</summary>
        public string StatusText { get { return "Timer" + Status.ToString(); } }

        /// <summary>終了時刻</summary>
        public DateTimeOffset FinTime { get { return _FinTime; } set { this.RaiseAndSetIfChanged(a => a.FinTime, value); } }
        private DateTimeOffset _FinTime = DateTimeOffset.MinValue;

        /// <summary>残り時間</summary>
        public TimeSpan Remain { get { return _Remain; } set { this.RaiseAndSetIfChanged(a => a.Remain, value); } }
        private TimeSpan _Remain = TimeSpan.Zero;

        /// <summary>分</summary>
        public int Minute { get { return _Minute; } set { this.RaiseAndSetIfChanged(a => a.Minute, value); } }
        private int _Minute = 0;

        /// <summary>秒</summary>
        public int Second { get { return _Second; } set { this.RaiseAndSetIfChanged(a => a.Second, value); } }
        private int _Second = 0;

        /// <summary>合計秒</summary>
        public int RemainSecond { get { return _RemainSecond; } set { this.RaiseAndSetIfChanged(a => a.RemainSecond, value); } }
        private int _RemainSecond = 0;

        /// <summary>残りカウンタ値(0.1秒単位)</summary>
        public int RemainCount { get { return _RemainCount; } set { this.RaiseAndSetIfChanged(a => a.RemainCount, value); } }
        private int _RemainCount = 0;

        /// <summary>0.5秒で点滅するマーク</summary>
        public bool Mark { get { return _Mark; } set { this.RaiseAndSetIfChanged(a => a.Mark, value); } }
        private bool _Mark = false;

        #endregion
        #region コマンド

        public ICommand StartOrPause{get;private set;}

        public ICommand Reset{get;private set;}

        #endregion
        #region コンストラクタ
        public CountDownTimerViewModel()
        {
            StartOrPause = ReactiveCommand.Create(a => true, a =>
                {
                    switch (Status) {
                        case CountDownTimerStatus.Run:
                            Status = CountDownTimerStatus.Pause;
                            break;
                        case CountDownTimerStatus.Fin:
                            Status = CountDownTimerStatus.Reset;
                            break;
                        default:
                            Status = CountDownTimerStatus.Run;
                            break;
                    }
                });

            Reset = ReactiveCommand.Create(a => true, a =>
                {
                    Status = CountDownTimerStatus.Reset;
                });

            TaskPropertyChanged = this.Changed.Subscribe(args =>
            {
                switch (args.PropertyName) {
                    case "Status": ChangeStatus(); raisePropertyChanged("StatusText"); break;
                    case "Remain": ChangeRemain(); break;
                    case "Span": ChangeSpan(); break;
                }
            });
            Span = TimeSpan.FromSeconds(10);
        }

        public void Dispose()
        {
            ObjectUtil.CheckDispose(ref TaskTimer);
            ObjectUtil.CheckDispose(ref TaskPropertyChanged);
        }

        #endregion
        #region ステータス変更処理

        private void ChangeSpan()
        {
            Status = CountDownTimerStatus.Reset;
            Remain = Span;
        }

        private void ChangeStatus()
        {
            // どっちにせよタイマーは止める
            ObjectUtil.CheckDispose(ref TaskTimer);

            // リセット処理
            switch (Status) {
                case CountDownTimerStatus.Reset:
                    Remain = Span;
                    return;
                case CountDownTimerStatus.Pause:
                    var remain = FinTime - DateTimeOffset.Now;
                    if (remain < TimeSpan.Zero) remain = TimeSpan.Zero;
                    Remain = remain;
                    return;
                case CountDownTimerStatus.Run: break;
                default: return;
            }

            // 計測開始
            var startTime = DateTimeOffset.Now;
            FinTime = startTime + Remain;
            var hasNext = true;
            TaskTimer = Observable.Generate(
                startTime, now => hasNext, now => DateTimeOffset.Now,
                now =>
                {
                    var remain = FinTime - now;
                    if (remain < TimeSpan.Zero) {
                        remain = TimeSpan.Zero;
                        hasNext = false;
                    }
                    Remain = remain;
                    return now;
                },
                now =>
                {
                    var nextRemain = TimeSpan.FromTicks((Remain.Ticks / TimerSpan) * TimerSpan);
                    return now + (Remain - nextRemain);
                },
                DispatcherScheduler.Instance).Subscribe();
        }
        private const long TimerSpan = TimeSpan.TicksPerSecond / 10;
        private const long MarkSpan = TimeSpan.TicksPerSecond / 2;


        #endregion
        #region カウントの更新処理

        private void ChangeRemain()
        {
            if (Remain <= TimeSpan.Zero) Status = CountDownTimerStatus.Fin;
            var tick = Remain.Ticks;

            RemainCount = (int)(tick / TimerSpan);
            RemainSecond = (int)(tick / TimeSpan.TicksPerSecond);
            Second = (int)(tick / TimeSpan.TicksPerSecond % 60);
            Minute = (int)(tick / TimeSpan.TicksPerMinute);
            var mark = (int)(tick / MarkSpan % 2);
            Mark = mark == 1;
        }



        #endregion
    }
}