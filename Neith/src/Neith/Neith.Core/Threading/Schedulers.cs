using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Concurrency;
using System.Threading.Tasks;

namespace Neith.Threading
{
    /// <summary>
    /// スケジューラ管理。
    /// </summary>
    internal static class Schedulers
    {
        /// <summary>通常のバックグラウンドタスクスケジューラ</summary>
        internal static readonly IScheduler NormalScheduler;
        /// <summary>長期実行タスクタスクスケジューラ</summary>
        internal static readonly IScheduler LongRunningScheduler;



        /// <summary>通常のバックグラウンドタスクファクトリ</summary>
        internal static readonly TaskFactory NormalFactory;
        /// <summary>長期実行タスクファクトリ</summary>
        internal static readonly TaskFactory LongRunningFactory;

        static Schedulers()
        {
            NormalFactory = new TaskFactory();
            NormalScheduler = new TaskPoolScheduler(NormalFactory);

            LongRunningFactory = new TaskFactory(
                TaskCreationOptions.LongRunning, TaskContinuationOptions.LongRunning);
            LongRunningScheduler = new TaskPoolScheduler(NormalFactory);


        }

    }
}
