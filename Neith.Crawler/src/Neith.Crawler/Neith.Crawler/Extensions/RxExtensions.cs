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
        /// <summary>
        /// Timestampで設定された時間まで待機します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rx"></param>
        /// <returns></returns>
        public static IObservable<T> DelayTimestamped<T>(this IObservable<Timestamped<T>> rx)
        {
            return rx
                .SelectMany(timed => Observable
                    .FromAsyncPattern<T>(
                        (callback, state) => {
                            return BeginDelay(timed.Timestamp, callback);
                        },
                        async => {
                            return timed.Value;
                        })())
                ;
        }

        /// <summary>
        /// 非同期待機関数。
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private static IAsyncResult BeginDelay(DateTimeOffset time, AsyncCallback callback)
        {
            var ar = new DelayResult();
            var dueTime = time - DateTimeOffset.Now;
            if (dueTime <= TimeSpan.Zero) {
                ar.AsyncWaitHandle = null;
                ar.CompletedSynchronously = true;
                ar.IsCompleted = true;
                callback(ar);
                return ar;
            }
            ar.AsyncWaitHandle =
                new EventWaitHandle(false, EventResetMode.ManualReset);
            var timer = new Timer((s) => {
                using (ar) {
                    ar.CompletedSynchronously = false;
                    ar.IsCompleted = true;
                    callback(ar);
                }
            }, null, dueTime, TimeSpan.FromMilliseconds(-1));
            ar.Timer = timer;
            return ar;
        }

        private class DelayResult : IAsyncResult, IDisposable
        {
            internal Timer Timer { get; set; }
            public object AsyncState { get { return Timer; } }
            public WaitHandle AsyncWaitHandle { get; internal set; }
            public bool CompletedSynchronously { get; internal set; }
            public bool IsCompleted { get; internal set; }
            public DelayResult()
            {
                AsyncWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
                CompletedSynchronously = false;
                IsCompleted = false;
            }

            public void Dispose()
            {
                Timer.Dispose(AsyncWaitHandle);
                AsyncWaitHandle.Dispose();
            }
        }




    }
}
