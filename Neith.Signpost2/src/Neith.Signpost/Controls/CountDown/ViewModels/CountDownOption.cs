using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Input;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ReactiveUI;
using ReactiveUI.Xaml;

namespace Neith.Signpost
{
    /// <summary>
    /// カウントダウンタイマーのオプション値を作成します。
    /// </summary>
    public static class CountDownOption
    {
        public static ICountDownOption CreateOptionCommand(this CountDownTimerViewModel timer, TimeSpan span)
        {
            return new CountDownTimeOption(timer, span);
        }

        public static ICountDownOption CreateOptionCommand<T>(
            this CountDownTimerViewModel timer, TimeSpan span,
            T clock, string name, Expression<Func<T, DateTimeOffset>> expr)
            where T : ReactiveObject
        {
            return new CountDownClockOption<T>(timer, span, clock, name, expr);
        }



        private class CountDownTimeOption : CountDownTimeOptionBase
        {
            private TimeSpan _Span;
            public override TimeSpan Span { get { return _Span; } }
            public override IObservable<DateTimeOffset> RxNextTime { get { return Observable.Never<DateTimeOffset>(); } }

            public CountDownTimeOption(CountDownTimerViewModel timer, TimeSpan span)
                : base(timer)
            {
                var mm = (int)Math.Floor(span.TotalMinutes);
                Name = string.Format("{0:00}:{1:ss}", mm, span);
                _Span = span;
            }
        }



        private class CountDownClockOption<T> : CountDownTimeOption
            where T : ReactiveObject
        {
            private IDisposable TaskChangeTime;

            private IObservable<DateTimeOffset> _RxNextTime;
            public override IObservable<DateTimeOffset> RxNextTime { get { return _RxNextTime; } }

            public CountDownClockOption(CountDownTimerViewModel timer, TimeSpan span,T clock, string name, Expression<Func<T, DateTimeOffset>> expr)
                : base(timer, span)
            {
                Name = name;
                _RxNextTime = clock
                    .ObservableForProperty(expr)
                    .Select(a => a.Value);
                TaskChangeTime = RxNextTime.Subscribe(UpdateNextTime);
                var func = expr.Compile();
                UpdateNextTime(func(clock));
            }

            private void UpdateNextTime(DateTimeOffset time)
            {
                NextTime = time;
                RaisePropertyChanged("NextTime");
                RaisePropertyChanged("Span");
            }

        }


        private abstract class CountDownTimeOptionBase : ReactiveCommand, ICountDownOption, INotifyPropertyChanged
        {
            private IDisposable TaskExec;
            public string Name { get; protected set; }
            public abstract TimeSpan Span { get; }
            public DateTimeOffset NextTime { get; protected set; }
            public abstract IObservable<DateTimeOffset> RxNextTime { get; }

            public CountDownTimeOptionBase(CountDownTimerViewModel timer)
                : base(a => true)
            {
                NextTime = DateTimeOffset.MaxValue;
                TaskExec = this.Subscribe(a => { timer.Option = this; });
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void RaisePropertyChanged(string name)
            {
                if (PropertyChanged == null) return;
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }

        }

    }
}