using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Neith.Signpost.Logger.Model;
using Wintellect.Sterling;
using Wintellect.Sterling.Database;

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
                Time = Neith.Util.DateTimeUtil.GetUniqueTimeStamp(),
                Application = "Neith.Signpost.Logger",
                Sender = "Neith.Signpost.Logger",
                Title = "END SERVICE",
            };
            AddLogBlock.SendAsync(item).Wait();

            // 全ログを列挙
            foreach (var kv in Database.AllLogsKV) {
                var l = kv.LazyValue.Value;
                Debug.WriteLine(l);
            }

            // 終了
            Tasks.Dispose();
        }


        public SterlingEngine DBEngine { get; private set; }
        public LogDBFileInstance Database { get; private set; }
        private ActionBlock<NeithLog> AddLogBlock { get; set; }


        private LogDBService()
        {
            // ルートパスを切り替える
            var rootPath = Neith.Util.Reflection.AssemblyUtil
                .GetCallingAssemblyDirctory()
                .PathCombine("database");
            Neith.Sterling.Server.FileSystem.PathProvider.RootPath = rootPath;

            // DB作成
            DBEngine = new SterlingEngine().Add(Tasks);
            DBEngine.Activate();
            Database = new LogDBFileInstance(this, DateTime.UtcNow);

            var instance = Database.Instance;

            AddLogBlock = new ActionBlock<NeithLog>(log =>
            {
                if (Tasks.IsDisposed) return;
                instance.Save(log);
            });

            // 開始ログを作る
            Post(new NeithLog
            {
                Time = Neith.Util.DateTimeUtil.GetUniqueTimeStamp(),
                Application = "Neith.Signpost.Logger",
                Sender = "Neith.Signpost.Logger",
                Title = "START SERVICE",
            });
        }

        /// <summary>
        /// ログを保存します。
        /// </summary>
        /// <param name="log"></param>
        public bool Post(NeithLog log)
        {
            return AddLogBlock.Post(log);
        }

    }
}
