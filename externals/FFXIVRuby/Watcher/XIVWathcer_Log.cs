using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public partial class XIVWathcer
    {
        /// <summary>ログ配信。</summary>
        private readonly BroadcastBlock<FFXIVLog> logBroadcast;

        /// <summary>ログ配信。</summary>
        public ISourceBlock<FFXIVLog> LogSource { get { return logBroadcast; } }

        private async Task LogWatch(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var xiv = await ScanProcess(token); if (xiv == null) continue;
                var reader = await SearchLogArea(xiv, token); if (reader == null) continue;
                await ReadLog(reader, token);
            }
        }

        /// <summary>
        /// プロセスを検索します。
        /// </summary>
        /// <returns></returns>
        private async Task<FFXIVProcess> ScanProcess(CancellationToken token)
        {
            logger.Trace("Scan XIV...");
            while (!token.IsCancellationRequested)
            {
                Process p = FFXIVMemoryProvidor.GetFFXIVGameProcess();
                if (p != null)
                {
                    logger.Trace("FFXIV found.");
                    return new FFXIVProcess(p);
                }
                await TaskEx.Delay(WaitScanProcess, token);
            }
            return null;
        }
        private static readonly TimeSpan WaitScanProcess = TimeSpan.FromSeconds(5);



        /// <summary>
        /// ログ領域を検索します。
        /// </summary>
        /// <param name="en14"></param>
        /// <returns></returns>
        private async Task<FFXIVLogReader> SearchLogArea(FFXIVProcess xiv, CancellationToken token)
        {
            for (var i = 0; i < 10; i++)
            {
                if (token.IsCancellationRequested) return null;
                var search = new LogStatusSearcher(xiv);
                var reader = search.SearchPLINQ(token);
                if (reader != null) return reader;
                await TaskEx.Delay(WaitScanProcess, token);
            }
            return null;
        }

        /// <summary>
        /// ログを読み込みます。
        /// </summary>
        /// <returns></returns>
        private async Task ReadLog(FFXIVLogReader reader, CancellationToken token)
        {
            logger.Trace("Read XIV log...");
            var from = int.MaxValue;
            var proc = reader.FFXIV.Proc;
            while (!proc.HasExited)
            {
                if (token.IsCancellationRequested) return;
                if (from == reader.TerminalPoint)
                {
                    await TaskEx.Delay(WaitReadLog, token);
                    continue;
                }
                var to = reader.TerminalPoint;
                if (from > to) from = reader.EntryPoint;
                foreach (var item in reader.GetLogs(from, to))
                {
                    if (token.IsCancellationRequested) return;
                    logBroadcast.Post(item);
                }
                from = to;
            }
        }
        private static readonly TimeSpan WaitReadLog = TimeSpan.FromMilliseconds(20);



    }
}
