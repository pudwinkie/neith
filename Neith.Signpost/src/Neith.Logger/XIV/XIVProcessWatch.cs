using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using FFXIVRuby;

namespace Neith.Logger.XIV
{
    public static class XIVProcessWatch
    {
        /// <summary>
        /// 要求がある限りプロセスを検索します。
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<FFXIVProcess> EnScanProcess()
        {
            while (true) {
                Process p = FFXIVMemoryProvidor.GetFFXIVGameProcess();
                if (p == null) {
                    yield return null;
                    continue;
                }
                yield return new FFXIVProcess(p);
            }
        }

        /// <summary>
        /// 要求がある限りログ領域を検索します。
        /// </summary>
        /// <param name="en14"></param>
        /// <returns></returns>
        private static FFXIVLogStatus SearchLogStatus(this FFXIVProcess xiv)
        {
            var search = new LogStatusSearcher(xiv);
            var logstat = search.SearchPLINQ();
            return logstat;
        }

        /// <summary>
        /// 要求がある限りログを読み取ります。
        /// </summary>
        /// <param name="en14"></param>
        /// <returns></returns>
        private static IEnumerable<FFXIVLog> EnReadMemoryLog(this FFXIVLogStatus stat)
        {
            var term = int.MinValue;
            var proc = stat.FFXIV.Proc;
            while (!proc.HasExited) {
                if (term == stat.TerminalPoint) {
                    yield return null;
                    continue;
                }
                term = stat.TerminalPoint;
                foreach (var log in stat.GetLogs()) yield return log;
            }
        }

        private static FFXIVLog Internal(FFXIVProcess ff14, FFXILogMessageType tp, string message)
        {
            return new FFXIVLog(ff14, (int)tp, "", message);
        }

        /// <summary>
        /// 要求がある限りログを読み取ります。
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<FFXIVLog> EnReadMemoryLog(WaitHandle wo)
        {
            if (wo.WaitOne(100)) yield break;
            yield return Internal(null, FFXILogMessageType.INTERNAL_START, "## FF14 ログ監視処理開始 ##");
            foreach (var ff14 in EnScanProcess()) {
                if (ff14 == null) {
#if DEBUG
                    yield return Internal(null, FFXILogMessageType.INTERNAL_WAIT, "FF14プロセス検索：５秒待機");
#endif
                    if (wo.WaitOne(5000)) yield break;
                    continue;
                }
                yield return Internal(ff14, FFXILogMessageType.INTERNAL_FOUND14, "## FF14 プロセス発見、ログ領域検索開始 ##");
                FFXIVLogStatus reader = null;
                for (var i = 0; i < 10; i++) {
                    reader = ff14.SearchLogStatus();
                    if (reader != null) break;
#if DEBUG
                    yield return Internal(ff14, FFXILogMessageType.INTERNAL_WAIT, "FF14ログ領域検索：５秒待機");
#endif
                    if (wo.WaitOne(5000)) yield break;
                    continue;
                }
                if (reader == null) continue;
                yield return Internal(ff14, FFXILogMessageType.INTERNAL_FOUND_LOG, "## FF14 ログ領域発見、ログ列挙開始 ##");

                foreach (var log in reader.EnReadMemoryLog()) {
                    if (log == null) {
                        if (wo.WaitOne(10)) yield break;
                        continue;
                    }
                    yield return log;
                }
                yield return Internal(ff14, FFXILogMessageType.INTERNAL_LOST14, "## FF14 プロセスロスト ##");
            }
        }

    }
}