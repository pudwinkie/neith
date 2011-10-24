using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Threading;
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
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly CancellationTokenSource CTS = new CancellationTokenSource();
        private readonly CompositeDisposable Tasks = new CompositeDisposable();
        public void Dispose()
        {
            try
            {
                CTS.Cancel();
                Tasks.Dispose();
            }
            catch (Exception ex) { logger.Error(ex); }
            logger.Info("XIVWathcer Disposed");
        }

        private readonly Dispatcher Dispatcher;

        public XIVWathcer(Dispatcher dispatcher)
        {
            logger.Info("XIVWathcer Start");
            Dispatcher = dispatcher;
            logBroadcast = new BroadcastBlock<FFXIVLog>(a => a);

            InitLog();

            Start();
        }

        public void Start()
        {
            CTS.Add(Tasks);
            TaskEx.RunEx(() => LogWatch(CTS.Token)).Add(Tasks);
        }
    }
}
