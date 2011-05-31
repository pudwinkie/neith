using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
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
        private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        private IDisposable storeTask;
        private IDisposable collectTask;
        private LogStore store;


        public LogStore Store { get { return store; } }

        public LogService()
            : base()
        {
            InitStoreTask();
            var col14 = new XIV.XIVCollecter();
            collectTask = Observable.FromEventPattern<NeithLogEventArgs>(col14, "Collect")
                .Subscribe(a => OnReceive(a.EventArgs.Log));
        }

        private void InitStoreTask()
        {
            // ログの保存処理
            store = LogStore.Instance;
            storeTask = Observable
                .FromEventPattern<NeithLogEventArgs>(this, "Receive")
                .SubscribeOn(Scheduler.ThreadPool)
                .Subscribe(a => Store.Store(a.EventArgs.Log));
        }

        protected override void Dispose(bool disposing)
        {
            ObjectUtil.CheckDispose(ref collectTask);
            ObjectUtil.CheckDispose(ref storeTask);
            ObjectUtil.CheckDispose(ref store);
            base.Dispose(disposing);
        }

        /// <summary>
        /// 受信イベント。
        /// </summary>
        public event NeithLogEventHandler Receive;

        /// <summary>
        /// 受信データを保存し、イベントを発行します。
        /// </summary>
        /// <param name="log"></param>
        public void OnReceive(NeithLog log)
        {
            if (Receive == null) return;
            Receive(this, new NeithLogEventArgs(log));
        }

    }
}
