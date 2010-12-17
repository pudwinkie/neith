using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFXIVRuby
{
    public class FFXIVLogStatus
    {
        // Fields
        private int Entry;
        private int lastReadPoint;
        private FFXIVProcess ffxiv;

        // Methods
        public FFXIVLogStatus(FFXIVProcess _ffxiv, int entry)
        {
            ffxiv = _ffxiv;
            Entry = entry;
            lastReadPoint = EntryPoint;
        }

        /// <summary>
        /// ログを全て列挙します。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FFXIVLog> GetLogs()
        {
            return FFXIVLog.GetLogs(GetLogData(), Encoding.GetEncoding("utf-8"));
        }

        /// <summary>
        /// 指定位置から最後までログを列挙します。
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public IEnumerable<FFXIVLog> GetLogs(int from)
        {
            return FFXIVLog.GetLogs(GetLogData(from, TerminalPoint - from), Encoding.GetEncoding("utf-8"));
        }

        /// <summary>
        /// 指定範囲のログを列挙します。
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public IEnumerable<FFXIVLog> GetLogs(int from, int to)
        {
            return FFXIVLog.GetLogs(GetLogData(from, to - from), Encoding.GetEncoding("utf-8"));
        }


        /// <summary>
        /// 未取得のログを列挙します。
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public IEnumerable<FFXIVLog> GetRemainLogs()
        {
            var term = TerminalPoint;
            if (lastReadPoint == term) return Enumerable.Empty<FFXIVLog>();
            var from = lastReadPoint;
            var to = term;
            lastReadPoint = term;
            return GetLogs(from, to);
        }

        private byte[] GetLogData()
        {
            return GetLogData(EntryPoint, Size);
        }

        private byte[] GetLogData(int from, int size)
        {
            return ffxiv.ReadBytes(from, size);
        }


        /// <summary>ログ領域開始位置。</summary>
        public int EntryPoint { get { return GetEntryPoint(); } }

        /// <summary>ログ領域終了位置。</summary>
        public int TerminalPoint { get { return GetTerminalPoint(); } }

        /// <summary>ログ領域のサイズ。</summary>
        public int Size { get { return TerminalPoint - EntryPoint; } }


        private int GetTerminalPoint()
        {
            return this.ffxiv.ReadInt32(Entry + 4);
        }
        private int GetEntryPoint()
        {
            return ffxiv.ReadInt32(this.Entry);
        }

    }
}
