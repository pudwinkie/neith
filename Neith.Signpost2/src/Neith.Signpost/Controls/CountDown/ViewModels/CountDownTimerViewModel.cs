using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using ReactiveUI;
using ReactiveUI.Xaml;
using Neith.Util;
using System.Windows.Input;

namespace Neith.Signpost
{
    public class CountDownTimerViewModel : ReactiveObject, IDisposable
    {
        #region フィールド
        IDisposable TaskPropertyChanged;
        IDisposable TaskTimer;

        #endregion
        #region プロパティ
        /// <summary>タイマー設定一覧</summary>
        public ReactiveCollection<ICountDownOption> OptionList { get; private set; }

        /// <summary>タイマー設定</summary>
        public ICountDownOption Option { get { return _Option; } set { this.RaiseAndSetIfChanged(a => a.Option, value); } }
        private ICountDownOption _Option = null;

        /// <summary>ステータス</summary>
        public CountDownTimerStatus Status { get { return _Status; } set { this.RaiseAndSetIfChanged(a => a.Status, value); } }
        private CountDownTimerStatus _Status = CountDownTimerStatus.Reset;

        /// <summary>ステータス：テキスト</summary>
        public string StatusText { get { return "Timer" + Status.ToString(); } }

        /// <summary>残り時間表示モード</summary>
        public CountDownTimerRemainStatus RemainStatus { get { return _RemainStatus; } set { this.RaiseAndSetIfChanged(a => a.RemainStatus, value); } }
        private CountDownTimerRemainStatus _RemainStatus = CountDownTimerRemainStatus.Normal;

        /// <summary>残り時間表示モード：テキスト</summary>
        public string RemainStatusText { get { return "Remain" + RemainStatus.ToString(); } }


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
                    case CountDownTimerStatus.Reset:
                        Remain = Option.Span;
                        Status = CountDownTimerStatus.Run;
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
                    case "RemainStatus": raisePropertyChanged("RemainStatusText"); break;
                    case "Remain": ChangeRemain(); break;
                    case "Option": ChangeOption(); break;
#if DEBUG
                    case "FinTime": Debug.WriteLine(string.Format("FinTime={0}", FinTime)); break;
#endif
                }
            });

            var clock = AppModel.Clock.Model;

            OptionList = new ReactiveCollection<ICountDownOption>(new[]{
                this.CreateOptionCommand(TimeSpan.FromMinutes(30) ,clock ,"NM Pop",a=>a.NMPopTime ),
                this.CreateOptionCommand(TimeSpan.FromSeconds(10)),
                this.CreateOptionCommand(TimeSpan.FromSeconds(30)),
                this.CreateOptionCommand(TimeSpan.FromMinutes(1)),
                this.CreateOptionCommand(TimeSpan.FromMinutes(2)),
                this.CreateOptionCommand(TimeSpan.FromMinutes(3)),
                this.CreateOptionCommand(TimeSpan.FromMinutes(5)),
                this.CreateOptionCommand(TimeSpan.FromMinutes(10)),
                this.CreateOptionCommand(TimeSpan.FromMinutes(15)),
                this.CreateOptionCommand(TimeSpan.FromMinutes(20)),
                this.CreateOptionCommand(TimeSpan.FromMinutes(30)),
                this.CreateOptionCommand(TimeSpan.FromMinutes(60)),
                this.CreateOptionCommand(TimeSpan.FromMinutes(90)),
            });
            Option = OptionList[0];
        }

        public void Dispose()
        {
            ObjectUtil.CheckDispose(ref TaskTimer);
            ObjectUtil.CheckDispose(ref TaskPropertyChanged);
        }

        #endregion
        #region ステータス変更処理

        private void ChangeOption()
        {
            Status = CountDownTimerStatus.Init;
        }

        private void ChangeStatus()
        {
            // どっちにせよタイマーは止める
            DeleteAllTimerTask();

            // リセット処理
            switch (Status) {
                case CountDownTimerStatus.Init:
                    Status = CountDownTimerStatus.Reset;
                    return;
                case CountDownTimerStatus.Reset:
                    if (Option.NextTime != DateTimeOffset.MaxValue) StartResetTimerTask();
                    else Remain = Option.Span;
                    return;
                case CountDownTimerStatus.Pause:
                    var remain = FinTime - DateTimeOffset.Now;
                    if (remain < TimeSpan.Zero) remain = TimeSpan.Zero;
                    Remain = remain;
                    return;
                case CountDownTimerStatus.Run:
                    StartCountDownTask();
                    return;
            }
        }

        private void DeleteAllTimerTask()
        {
            ObjectUtil.CheckDispose(ref TaskTimer);
        }

        /// <summary>
        /// カウントダウン開始。
        /// </summary>
        private void StartCountDownTask()
        {
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
        private static readonly TimeSpan OneTick = TimeSpan.FromTicks(1);


        /// <summary>
        /// リセット状態で動くタイマー。
        /// </summary>
        private void StartResetTimerTask()
        {
            // 終了時間の更新タスク
            FinTime = Option.NextTime;
            var TaskUpdateFinTime = Option.RxNextTime
                .Subscribe(a => { FinTime = a; });

            // 計測開始
            var nextTime = DateTimeOffset.Now;
            TaskTimer = Observable.Generate(
                nextTime, now => true, now => DateTimeOffset.Now,
                now =>
                {
                    var remain = FinTime - now;
                    if (remain <= TimeSpan.Zero) remain = OneTick;
                    Remain = remain;
                    return now;
                },
                now =>
                {
                    if (Remain <= OneTick) return now + TimeSpan.FromSeconds(0.1);
                    var nextRemain = TimeSpan.FromTicks((Remain.Ticks / TimerSpan) * TimerSpan);
                    return now + (Remain - nextRemain);
                },
                DispatcherScheduler.Instance)
                .Finally(() =>
                {
                    TaskUpdateFinTime.Dispose();
                })
                .Subscribe();
        }


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