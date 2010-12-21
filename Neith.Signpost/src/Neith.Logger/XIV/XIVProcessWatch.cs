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
        public static IEnumerable<FFXIVProcess> EnScanProcess()
        {
            while (true) {
                Process p = FFXIVMemoryProvidor.GetFFXIVGameProcess();
                if (p == null) {
                    Thread.Sleep(5000);
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
        public static IEnumerable<FFXIVLogStatus> EnSearchLogStatus(this IEnumerable<FFXIVProcess> en14)
        {
            foreach (var xiv in en14) {
                var search = new LogStatusSearcher(xiv);
                var logstat = search.SearchPLINQ();
                if (logstat == null) {
                    Thread.Sleep(5000);
                    continue;
                }
                yield return logstat;
            }
        }

        /// <summary>
        /// 要求がある限りログを読み取ります。
        /// </summary>
        /// <param name="en14"></param>
        /// <returns></returns>
        public static IEnumerable<FFXIVLog> EnReadMemoryLog(this IEnumerable<FFXIVLogStatus> enStat)
        {
            foreach (var stat in enStat) {
                int term = int.MinValue;
                while (!stat.FFXIV.Proc.HasExited) {
                    if (term == stat.TerminalPoint) {
                        Thread.Sleep(10);
                        continue;
                    }
                    term = stat.TerminalPoint;
                    foreach (var log in stat.GetLogs()) yield return log;
                }
            }
        }

        /// <summary>
        /// 要求がある限りログを読み取ります。
        /// </summary>
        /// <param name="en14"></param>
        /// <returns></returns>
        public static IEnumerable<FFXIVLog> EnReadMemoryLog()
        {
            return EnScanProcess()
                .EnSearchLogStatus()
                .EnReadMemoryLog()
                ;
        }



    }
}
