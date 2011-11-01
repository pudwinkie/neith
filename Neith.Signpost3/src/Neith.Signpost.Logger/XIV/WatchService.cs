using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Threading;
using FFXIVRuby.Watcher;
using Neith.Signpost.Logger;
using Neith.Signpost.Logger.Model;
using Neith.Util.RX.ComponentModel;

namespace Neith.Signpost.Logger.XIV
{
    public class WatchService : IDisposable, ISourceBlock<NeithLog>
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private CompositeDisposable Tasks = new CompositeDisposable();
        public void Dispose()
        {
            Tasks.Dispose();
        }

        private XIVWathcer Watcher { get; set; }
        private ISourceBlock<NeithLog> LogSource { get; set; }
        private readonly Dispatcher Dispatcher;

        /// <summary>プロセスを見つけた状態。</summary>
        public RxProperty<bool> IsOnline { get { return Watcher.IsOnline; } }

        /// <summary>ログ読み込み中の状態。</summary>
        public RxProperty<bool> IsReading { get { return Watcher.IsReading; } }


        public WatchService(Dispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            var Watcher = new XIVWathcer(Dispatcher).Add(Tasks);

            // DBへの書き出し
            var trans = new TransformManyBlock<FFXIVRuby.FFXIVLog, NeithLog>(async a =>
            {
                var item = await a.ToNeithLog();
                if (item.Action == "SYSTEM_ERROR") return Enumerable.Empty<NeithLog>();
                return new[] { item };
            });
            Watcher.LogSource.LinkTo(trans, false).Add(Tasks);
            LogSource = trans;

            // microdata書き出し
            var dir = Const.XmlLogPath.GetDirectoryName();
            Directory.CreateDirectory(dir);
            var st = File.CreateText(Const.XmlLogPath).Add(Tasks);
            TaskEx.RunEx(() => st.WriteAsync(Const.MICRO_DATA_HTML_HEADER));
            var write = new ActionBlock<FFXIVRuby.FFXIVLog>(async a =>
            {
                await st.WriteAsync(a.ToMicroData().ToString());
            });
            Watcher.LogSource.LinkTo(write, false).Add(Tasks);
        }



        public void Start()
        {
            Watcher.Start();
        }



        #region ISourceBlock<NeithLog>

        public NeithLog ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<NeithLog> target, out bool messageConsumed)
        {
            return LogSource.ConsumeMessage(messageHeader, target, out  messageConsumed);
        }

        public IDisposable LinkTo(ITargetBlock<NeithLog> target, bool unlinkAfterOne)
        {
            return LogSource.LinkTo(target, unlinkAfterOne);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<NeithLog> target)
        {
            LogSource.ReleaseReservation(messageHeader, target);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<NeithLog> target)
        {
            return LogSource.ReserveMessage(messageHeader, target);
        }

        public void Complete()
        {
            LogSource.Complete();
        }

        public Task Completion
        {
            get { return LogSource.Completion; }
        }

        public void Fault(Exception exception)
        {
            LogSource.Fault(exception);
        }

        #endregion
    }
}
