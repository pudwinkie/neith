using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Wintellect.Sterling;
using Wintellect.Sterling.Database;
using Neith.Signpost.Logger.Model;

namespace Neith.Signpost.Logger
{
    public class LogDBService : IDisposable
    {
        public static LogDBService Instance { get; private set; }

        static LogDBService()
        {
            Instance = new LogDBService();
        }


        private readonly CompositeDisposable Tasks = new CompositeDisposable();
        public void Dispose()
        {
            // 終了ログを作る
            var item = new NeithLog
            {
                Application = "Neith.Signpost.Logger",
                Sender = "Neith.Signpost.Logger",
                Title = "END SERVICE",
            };
            Post(item);

            // 終了
            Tasks.Dispose();
        }


        public SterlingEngine DBEngine { get; private set; }
        public LogDBFileInstance Current { get; private set; }
        private ActionBlock<NeithLog> AddLogBlock { get; set; }


        private LogDBService()
        {
            DBEngine = new SterlingEngine().Add(Tasks);
            DBEngine.Activate();
            Current = new LogDBFileInstance(this, DateTime.UtcNow);

            var instance = Current.Instance;

            AddLogBlock = new ActionBlock<NeithLog>(log =>
            {
                if (Tasks.IsDisposed) return;
                instance.Save(log);
            });

            // 開始ログを作る
            var item = new NeithLog
            {
                Application = "Neith.Signpost.Logger",
                Sender = "Neith.Signpost.Logger",
                Title = "START SERVICE",
            };
            Post(item);
        }

        /// <summary>
        /// ログを保存します。
        /// </summary>
        /// <param name="log"></param>
        public void Post(NeithLog log)
        {
            AddLogBlock.Post(log);
        }

    }
}
