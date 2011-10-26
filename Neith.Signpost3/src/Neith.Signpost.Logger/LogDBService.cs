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
    public class LogDBService : IDisposable, ITargetBlock<NeithLog>, IDataflowBlock
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly CompositeDisposable Tasks = new CompositeDisposable();
        public void Dispose()
        {
            logger.Debug("Dispose START");
            // 終了ログを作る
            var item = new NeithLog
            {
                Time = Neith.Util.DateTimeUtil.GetUniqueTimeStamp(),
                Application = "Neith.Signpost.Logger",
                Sender = "Neith.Signpost.Logger",
                Action = "END SERVICE",
            };
            this.SendAsync(item).Wait();

            // 全ログを列挙
            try
            {
                foreach (var kv in Database.AllLogsKV)
                {
                    var rec = kv.LazyValue.Value;
#if false
                    var src = rec.Source as FFXIVRuby.FFXIVLog;
                    if (src != null)
                    {
                        var newAct = src.MessageType.ToString();
                        if (rec.Action != newAct)
                        {
                            rec.Action = newAct;
                            Database.Instance.Save(rec);
                        }
                        if (rec.Action == "SYSTEM_ERROR") Database.Instance.Delete(rec);
                    }
#else
                    logger.Debug(rec);
#endif
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            // 終了
            Tasks.Dispose();
            logger.Debug("Dispose END");
        }


        public SterlingEngine DBEngine { get; private set; }
        public LogDBFileInstance Database { get; private set; }
        private ITargetBlock<NeithLog> LogTarget { get; set; }


        public LogDBService(string rootPath)
        {
            var pathProvider = new Neith.Sterling.Server.FileSystem.PathProvider(rootPath);

            // DB作成
            DBEngine = new SterlingEngine().Add(Tasks);
            DBEngine.Activate();
            Database = new LogDBFileInstance(this, DateTime.UtcNow, pathProvider);

            var instance = Database.Instance;

            LogTarget = new ActionBlock<NeithLog>(log =>
            {
                if (Tasks.IsDisposed) return;
                logger.Debug(log);
                instance.Save(log);
            });

            // 開始ログを作る
            this.Post(new NeithLog
            {
                Time = Neith.Util.DateTimeUtil.GetUniqueTimeStamp(),
                Application = "Neith.Signpost.Logger",
                Sender = "Neith.Signpost.Logger",
                Action = "START SERVICE",
            });
        }

        public LogDBService()
            : this(Neith.Util.Reflection.AssemblyUtil
                .GetCallingAssemblyDirctory()
                .PathCombine("database"))
        {
        }




        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, NeithLog messageValue, ISourceBlock<NeithLog> source, bool consumeToAccept)
        {
            return LogTarget.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        public void Complete()
        {
            LogTarget.Complete();
        }

        public Task Completion
        {
            get { return LogTarget.Completion; }
        }

        public void Fault(Exception exception)
        {
            LogTarget.Fault(exception);
        }
    }
}
