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
        public static IObservable<T> Parallel<T>(this IObservable<T> pipe)
        {
            var readPipe = pipe.Publish(Scheduler.ThreadPool);
            readPipe.Connect();
            return readPipe;
        }

        public static IObservable<T> ReturnPool<T>(this T value)
        {
            return Observable
                .Return(value, Scheduler.ThreadPool)
                ;
        }
    }
}
