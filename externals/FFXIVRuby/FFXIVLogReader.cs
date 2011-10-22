using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FFXIVRuby
{
    public class FFXIVLogReader
    {
        // Fields
        private int Entry;
        public FFXIVProcess FFXIV { get; private set; }

        private static readonly Encoding TextEncoding = Encoding.UTF8;


        public override string ToString()
        {
            return string.Format("FFXIVLogStatus Entry=0x{0,8:X}", Entry);
        }

        // Methods
        public FFXIVLogReader(FFXIVProcess _ffxiv, int entry)
        {
            FFXIV = _ffxiv;
            Entry = entry;
        }

        /// <summary>
        /// ログを全て列挙します。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FFXIVLog> GetLogs()
        {
            return GetLogs(GetLogData(), TextEncoding, FFXIV);
        }

        /// <summary>
        /// 指定位置から最後までログを列挙します。
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public IEnumerable<FFXIVLog> GetLogs(int from)
        {
            return GetLogs(GetLogData(from, TerminalPoint - from), TextEncoding, FFXIV);
        }

        /// <summary>
        /// 指定範囲のログを列挙します。
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public IEnumerable<FFXIVLog> GetLogs(int from, int to)
        {
            return GetLogs(GetLogData(from, to - from), TextEncoding, FFXIV);
        }

        /// <summary>
        /// バイナリデータより指定範囲のログを列挙します。
        /// </summary>
        /// <param name="logData"></param>
        /// <param name="enc"></param>
        /// <returns></returns>
        public static IEnumerable<FFXIVLog> GetLogs(byte[] logData, Encoding enc, FFXIVProcess ff14)
        {
            var buf = logData
                .SkipWhile(a => a != 0x30)
                .ToArray();
            var input = enc.GetString(TABConvertor.TabEscape(buf));
            var matchs = regex.Matches(input);
            var strArray = regex.Split(input);
            for (var j = 1; j < strArray.Length; j++) {
                var strArray2 = strArray[j].Split(new char[] { ':' }, 2, StringSplitOptions.None);
                var strType = matchs[j - 1].Value.TrimEnd(new char[] { ':' });
                var numType = int.Parse(strType, NumberStyles.AllowHexSpecifier);
                var strWho = strArray2[0].Replace("\0", "").Trim(); 
                var strMes = strArray2[1].Replace("\0", "");
                var item = new FFXIVLog(numType, strWho, strMes);
                yield return item;
            }
        }
        private static Regex regex = new Regex("[0-9A-F]{4}:");



        public static IEnumerable<FFXIVLog> GetLogs(string path, Encoding enc)
        {
            var buf = File.ReadAllBytes(path);
            return GetLogs(buf, enc, null);
        }



        private byte[] GetLogData()
        {
            return GetLogData(EntryPoint, Size);
        }

        private byte[] GetLogData(int from, int size)
        {
            return FFXIV.ReadBytes(from, size);
        }


        /// <summary>ログ領域開始位置。</summary>
        public int EntryPoint { get { return GetEntryPoint(); } }

        /// <summary>ログ領域終了位置。</summary>
        public int TerminalPoint { get { return GetTerminalPoint(); } }

        /// <summary>ログ領域のサイズ。</summary>
        public int Size { get { return TerminalPoint - EntryPoint; } }


        private int GetTerminalPoint()
        {
            return this.FFXIV.ReadInt32(Entry + 4);
        }
        private int GetEntryPoint()
        {
            return FFXIV.ReadInt32(this.Entry);
        }

    }
}
