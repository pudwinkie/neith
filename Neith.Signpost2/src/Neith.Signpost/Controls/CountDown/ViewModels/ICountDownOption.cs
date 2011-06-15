using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Neith.Signpost
{
    /// <summary>
    /// カウントダウンタイマーのリセット値を管理します。
    /// </summary>
    public interface ICountDownOption : ICommand
    {
        /// <summary>設定名</summary>
        string Name { get; }

        /// <summary>時間</summary>
        TimeSpan Span { get; }

        /// <summary>指定時刻タイマーの目標時刻</summary>
        DateTimeOffset NextTime { get;}

        /// <summary>指定時刻タイマーの目標時刻通知</summary>
        IObservable<DateTimeOffset> RxNextTime { get; }
    }
}