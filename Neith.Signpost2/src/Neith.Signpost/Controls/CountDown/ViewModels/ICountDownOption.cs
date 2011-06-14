using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Signpost
{
    /// <summary>
    /// カウントダウンタイマーのリセット値を管理します。
    /// </summary>
    public interface ICountDownOption
    {
        /// <summary>設定名</summary>
        string Name { get; }

        /// <summary>時間</summary>
        TimeSpan Span { get; }
    }
}