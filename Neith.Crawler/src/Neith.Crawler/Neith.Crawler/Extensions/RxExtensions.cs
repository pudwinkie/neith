using System;
using System.Collections.Generic;
using System.Concurrency;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System.Linq
{
    public static class RxExtensions
    {
        public static IObservable<T> DelayTimestamped<T>(this IObservable<Timestamped<T>> rx)
        {
            return rx
                .RemoveTimestamp()
                ;

        }


    }
}
