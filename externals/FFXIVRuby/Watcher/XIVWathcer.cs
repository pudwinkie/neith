using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Reactive.Disposables;
using System.Diagnostics;
using NLog;

namespace FFXIVRuby.Watcher
{
    /// <summary>
    /// FFXIVの監視タスク。
    /// </summary>
    public partial class XIVWathcer : IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly CompositeDisposable Tasks = new CompositeDisposable();
        public void Dispose()
        {
            Tasks.Dispose();
            log.Info("XIVWathcer Disposed");
        }

        public XIVWathcer()
        {
            log.Info("XIVWathcer Start");
            logBroadcast = new BroadcastBlock<FFXIVLog>(a => a);
            TaskEx.RunEx(LogWatch).Add(Tasks);
        }
    }
}
