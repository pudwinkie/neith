using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Concurrency;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Neith.Logger.Model;
using Neith.Util;

namespace Neith.Logger
{
    /// <summary>
    /// ログの取得管理、保存処理などの呼び出しを行います。
    /// 現在は簡易実装です。
    /// </summary>
    public class LogService : Component
    {
        public LogStore Store { get; private set; }

        private IDisposable storeTask;
        private IDisposable collectTask;

        public LogService()
            : base()
        {
            InitStoreTask();
            var col14 = new XIV.XIVCollecter();
            collectTask = Observable.FromEvent<LogEventArgs>(col14, "Collect")
                .Subscribe(a => OnReceive(a.EventArgs.Log));
        }

        private void InitStoreTask()
        {
            // ログの保存処理
            Store = LogStore.Instance;
            storeTask = Observable
                .FromEvent<LogEventArgs>(this, "Receive")
                .SubscribeOn(Scheduler.ThreadPool)
                .Subscribe(a => Store.Store(a.EventArgs.Log));
        }

        protected override void Dispose(bool disposing)
        {
            ObjectUtil.CheckDispose(ref collectTask);
            ObjectUtil.CheckDispose(ref storeTask);
            if (disposing) Store.SteramClose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// 受信イベント。
        /// </summary>
        public event LogEventHandler Receive;

        /// <summary>
        /// 受信データを保存し、イベントを発行します。
        /// </summary>
        /// <param name="log"></param>
        public void OnReceive(Log log)
        {
            if (Receive == null) return;
            Receive(this, new LogEventArgs(log));
        }

        public void OldLogConvert()
        {
            Directory
                .GetFiles(Const.Folders.Log, "*.log", SearchOption.AllDirectories)
                .AsParallel()
                .ForAll(path =>
                {
                    Debug.WriteLine("CONV: " + path);
                    var newPath = Path.GetFileNameWithoutExtension(path) + ".new";
                    path.EnDeserialize<Log>()
                        .Select(log =>
                        {
                            log.Collector = "XIV.XIVCollecter";
                            log.Analyzer = "XIV.XIVAnalyzer";
                            return log;
                        })
                        .SerializeAll(newPath)
                        ;
                });
        }
    }
}
