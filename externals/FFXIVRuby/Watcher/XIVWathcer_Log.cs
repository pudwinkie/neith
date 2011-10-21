using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Diagnostics;
using NLog;

namespace FFXIVRuby.Watcher
{
    /// <summary>
    /// FFXIVの監視タスク。
    /// </summary>
    public partial class XIVWathcer 
    {
        private readonly Subject<FFXIVLog> LogSubject;

        /// <summary>ログ通知。</summary>
        public IObservable<FFXIVLog> RxLog { get { return LogSubject; } }



        private async Task LogWatch()
        {
            while (true) {
                var xiv = await ScanProcess();
                var reader = await SearchLogArea(xiv);
                if (reader == null) continue;
                await ReadLog(reader);
            }
        }

        /// <summary>
        /// プロセスを検索します。
        /// </summary>
        /// <returns></returns>
        private async Task<FFXIVProcess> ScanProcess()
        {
            log.Trace("Scan XIV...");
            while (true) {
                Process p = FFXIVMemoryProvidor.GetFFXIVGameProcess();
                if (p != null) {
                    log.Trace("FFXIV found.");
                    return new FFXIVProcess(p);
                }
                await TaskEx.Delay(WaitScanProcess);
            }
        }
        private static readonly TimeSpan WaitScanProcess = TimeSpan.FromSeconds(5);



        /// <summary>
        /// ログ領域を検索します。
        /// </summary>
        /// <param name="en14"></param>
        /// <returns></returns>
        private async Task<FFXIVLogReader> SearchLogArea(FFXIVProcess xiv)
        {
            for (var i = 0; i < 10; i++) {
                var search = new LogStatusSearcher(xiv);
                var reader = search.SearchPLINQ();
                if (reader != null) return reader;
                await TaskEx.Delay(WaitScanProcess);
            }
            return null;
        }

        /// <summary>
        /// ログを読み込みます。
        /// </summary>
        /// <returns></returns>
        private async Task ReadLog(FFXIVLogReader reader)
        {
            log.Trace("Read XIV log...");
            var from = int.MaxValue;
            var proc = reader.FFXIV.Proc;
            while (!proc.HasExited) {
                if (from == reader.TerminalPoint) {
                    await TaskEx.Delay(WaitReadLog);
                    continue;
                }
                var to = reader.TerminalPoint;
                if (from > to) from = reader.EntryPoint;
                foreach (var item in reader.GetLogs(from, to)) {
                    LogSubject.OnNext(item);
                }
                from = to;
            }
        }
        private static readonly TimeSpan WaitReadLog = TimeSpan.FromMilliseconds(50);



    }
}
