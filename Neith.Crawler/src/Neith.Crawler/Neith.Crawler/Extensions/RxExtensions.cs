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
        public static IObservable<T> AsParallel<T>(this IObservable<T> pipe)
        {
            // 現在は並列化は実現できていないのでそのまま返す
            // 本当は処理をばらしてMergeしたい。追い抜きOKの時に使う
            return pipe;

            /*
            var readPipe = pipe.Publish(Scheduler.ThreadPool);
            readPipe.Connect();
            return readPipe;
             */
        }

        public static IObservable<T> ReturnPool<T>(this T value)
        {
            return Observable
                .Return(value, Scheduler.ThreadPool)
                ;
        }
    }
}
