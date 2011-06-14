using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Neith.Signpost
{
    /// <summary>
    /// カウントダウンタイマーのオプション値を作成します。
    /// </summary>
    public static class CountDownOption
    {
        public static ICountDownOption Create(TimeSpan span)
        {
            return new CountDownTimeOption(span);
        }


        private class CountDownTimeOption : ICountDownOption
        {
            public string Name { get; private set; }
            public TimeSpan Span { get; private set; }

            public CountDownTimeOption(TimeSpan span)
            {
                Name = span.ToString();
                Span = span;
            }

        }




    }
}
