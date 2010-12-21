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
        private static IEnumerable<FFXIVLogStatus> EnSearchLogStatus(this IEnumerable<FFXIVProcess> en14)
        {
            foreach (var xiv in en14) {
                yield return SearchLogStatus(xiv);
            }
        }

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
            int term = int.MinValue;
            while (!stat.FFXIV.Proc.HasExited) {
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
        /// <param name="en14"></param>
        /// <returns></returns>
        public static IEnumerable<FFXIVLog> EnReadMemoryLog()
        {
            yield return Internal(null, FFXILogMessageType.INTERNAL_START, "## FF14 ログ監視処理開始 ##");
            foreach (var ff14 in EnScanProcess()) {
                if (ff14 == null) {
#if DEBUG
                    yield return Internal(null, FFXILogMessageType.INTERNAL_WAIT, "FF14プロセス検索：５秒待機");
#endif
                    Thread.Sleep(5000);
                    continue;
                }
                FFXIVLogStatus reader = null;
                for (var i = 0; i < 10; i++) {
                    reader = ff14.SearchLogStatus();
                    if (reader != null) break;
#if DEBUG
                    Internal(ff14, FFXILogMessageType.INTERNAL_WAIT, "FF14ログ領域検索：５秒待機");
#endif
                    Thread.Sleep(5000);
                    continue;
                }
                if (reader == null) continue;

                foreach (var log in reader.EnReadMemoryLog()) {
                    if (log == null) {
                        Thread.Sleep(10);
                        continue;
                    }
                    yield return log;
                }
            }
            yield break;

        }



    }
}
