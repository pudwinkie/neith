using System;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading.Tasks;
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
            LogSubject = new Subject<FFXIVLog>().Add(Tasks);
            TaskEx.RunEx(LogWatch).Add(Tasks);
        }
    }
}
