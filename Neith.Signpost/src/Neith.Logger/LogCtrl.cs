using System;
using System.Collections.Generic;
using System.Linq;
using System.Concurrency;
using System.Text;
using System.ComponentModel;
using Neith.Logger.Model;

namespace Neith.Logger
{
    /// <summary>
    /// ログの取得管理、保存処理などの呼び出しを行います。
    /// 現在は簡易実装です。
    /// </summary>
    public class LogCtrl : Component 
    {
        public LogStore Store { get; private set; }

        private IDisposable storeTask;
        private IDisposable collectTask;

        public LogCtrl()
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
            // ログ取得を停止
            CheckDispose(ref collectTask);

            // ログ保存を停止する。
            CheckDispose(ref storeTask);
            if (disposing) Store.SteramClose();

            // 上位解放処理
            base.Dispose(disposing);
        }

        private static void CheckDispose<T>(ref T obj)
            where T : IDisposable
        {
            if (obj == null) return;
            obj.Dispose();
            obj = default(T);
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
        
    }
}
