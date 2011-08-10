using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Neith.Util.Threading
{
    /// <summary>
    /// 並行処理ユーティリティ。
    /// </summary>
    public static class Para
    {
        /// <summary>
        /// 非同期エミュレーターラッパー。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gen"></param>
        /// <returns></returns>
        public static IEnumerable<T> Enumerable<T>(IEnumerable<T> gen)
        {
            // 非同期生成処理を起動
            Queue<T> queue = new Queue<T>();
            bool isCompleted = false;
            bool isWait = false;
            ThreadStart exec = delegate()
            {
                foreach (T item in gen) {
                    lock (queue) {
                        queue.Enqueue(item);
                        if (isWait) Monitor.Pulse(queue);
                    }
                }
                lock (queue) {
                    isCompleted = true;
                    if (isWait) Monitor.Pulse(queue);
                }
            };
            exec.BeginInvoke(null, null);

            // 非同期読み込み処理
            while (true) {
                // キューが０の時は状態待機
                if (queue.Count == 0) {
                    lock (queue) {
                        while (queue.Count == 0) {
                            if (isCompleted) yield break;
                            isWait = true;
                            Monitor.Wait(queue);
                            isWait = false;
                        }
                    }
                }
                T item;
                lock (queue) {
                    item = queue.Dequeue();
                }
                yield return item;
            }
        }

        /// <summary>
        /// 列挙要素に対して並列に処理を実行します。
        /// 実行順序は保障されません。
        /// 全ての処理の実行が完了するまで処理はブロックされます。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gen"></param>
        /// <param name="act"></param>
        public static void ForEach<T>(IEnumerable<T> gen, Action<T> act)
        {
            List<WaitHandle> waits = new List<WaitHandle>();
            foreach (T item in gen) {
                IAsyncResult rc = act.BeginInvoke(item, null, null);
                waits.Add(rc.AsyncWaitHandle);
            }
            WaitHandle.WaitAll(waits.ToArray());
        }
        private delegate void ForEachFunc<T>(IEnumerable<T> gen, Action<T> act);

        /// <summary>
        /// 全て非同期で実行するForEachです。
        /// 実行順序は保障されません。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gen"></param>
        /// <param name="act"></param>
        /// <returns></returns>
        public static IAsyncResult AsyncForEach<T>(IEnumerable<T> gen, Action<T> act)
        {
            ForEachFunc<T> func = ForEach<T>;
            return func.BeginInvoke(gen, act, null, null);
        }

    }
}
