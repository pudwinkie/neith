using System;
using System.Collections.Generic;
using System.Concurrency;
using System.Threading;
using System.Linq;
using System.Text;

namespace ZStudy.Rx.Parallel
{
    public static class Program
    {
        static void Main(string[] args)
        {
            // スレッドプールに対してジョブを発行するパイプ
            var pipe = Observable
                .Range(1, 9, Scheduler.ThreadPool)
                ;
            var stopwatch = new System.Diagnostics.Stopwatch();

            // ■１つのパイプを実行
            Console.WriteLine("#### TEST 1 ####");
            stopwatch.Restart();
            pipe
                .ConsoleWrite("1 ")
                .Run();
            stopwatch.Stop();
            Console.WriteLine("#### TEST 1 ==> time {0:00000}ms\n\n"
                , stopwatch.ElapsedMilliseconds);
            // ---> 処理は直列化されています。
            //      １つのタスクが終わるまで次はスケジュールされません。


            // ■２つのパイプをマージ
            Console.WriteLine("#### TEST 2 ####");
            stopwatch.Restart();
            Observable.Merge(
                pipe.ConsoleWrite("1 "),
                pipe.ConsoleWrite(" 2"))
                .Run(cnt => {
                    var text = string.Format("({0:0})[--] :                         MERGE({1:00})",
                        cnt, Thread.CurrentThread.ManagedThreadId);
                    Console.WriteLine("({1:HH:mm:ss.fff}) {0}", text, DateTimeOffset.Now);
                });
            stopwatch.Stop();
            Console.WriteLine("#### TEST 2 ==> time {0:00000}ms\n\n"
                , stopwatch.ElapsedMilliseconds);
            // ---> ２つのタスクは並列実行されます。
            //      それぞれのタスクは直列に実行されています。


            // ■１つのパイプに２つの処理を続けて実行
            Console.WriteLine("#### TEST 3 ####");
            stopwatch.Restart();
            pipe
                .ConsoleWrite("1 ")
                .ConsoleWrite(" 2")
                .Run();
            stopwatch.Stop();
            Console.WriteLine("#### TEST 3 ==> time {0:00000}ms\n\n"
                , stopwatch.ElapsedMilliseconds);
            // ---> ２つの処理は直列実行されます。
            //      ２つの処理が終わるまで次のタスクはスケジュールされません。


            // ■１つのパイプに２つの処理をスケジューラ切り替えをはさんで実行
            Console.WriteLine("#### TEST 4 ####");
            stopwatch.Restart();
            pipe
                .ObserveOn(Scheduler.ThreadPool)
                .ConsoleWrite("1 ")
                .ObserveOn(Scheduler.ThreadPool)
                .ConsoleWrite(" 2")
                .Run();
            stopwatch.Stop();
            Console.WriteLine("#### TEST 4 ==> time {0:00000}ms\n\n"
                , stopwatch.ElapsedMilliseconds);
            // ---> ２つの処理は並列実行されます。
            //      パイプ内で処理が追い抜かれることはありません。


            Console.WriteLine("\n#### 終了しました、何かキーを押してください。 ####");
            Console.Read();
        }

        public static IObservable<int> ConsoleWrite(this IObservable<int> pipe, string name)
        {
            var rand = new Random(name.GetHashCode());
            return pipe
                .Do(count => {
                    var sleepTime = rand.Next(999);
                    var title = string.Format("({0:0})[{1}]", count, name);
                    Console.WriteLine("({0:HH:mm:ss.fff}) {1} : START({2:00}/{3:000}ms)"
                        , DateTimeOffset.Now, title
                        , Thread.CurrentThread.ManagedThreadId, sleepTime);

                    Thread.Sleep(sleepTime);

                    Console.WriteLine("({0:HH:mm:ss.fff}) {1} :                 END({2:00})"
                        , DateTimeOffset.Now, title
                        , Thread.CurrentThread.ManagedThreadId, sleepTime);
                })
                ;
        }

    }

}
