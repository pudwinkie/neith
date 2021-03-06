﻿using System;
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
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private IntPtr Entry;
        public FFXIVProcess FFXIV { get; private set; }

        private static readonly Encoding TextEncoding = Encoding.UTF8;


        public override string ToString()
        {
            return string.Format("FFXIVLogStatus Entry=0x{0,8:X}", Entry);
        }

        // Methods
        public FFXIVLogReader(FFXIVProcess _ffxiv, IntPtr entry)
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
            return GetLogs(GetLogData(), TextEncoding);
        }

        /// <summary>
        /// 指定位置から最後までログを列挙します。
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public IEnumerable<FFXIVLog> GetLogs(IntPtr from)
        {
            return GetLogs(GetLogData(from, TerminalPoint), TextEncoding);
        }

        /// <summary>
        /// 指定範囲のログを列挙します。
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public IEnumerable<FFXIVLog> GetLogs(IntPtr from, IntPtr to)
        {
            return GetLogs(GetLogData(from, to), TextEncoding);
        }

        /// <summary>
        /// バイナリデータより指定範囲のログを列挙します。
        /// </summary>
        /// <param name="logData"></param>
        /// <param name="enc"></param>
        /// <returns></returns>
        public static IEnumerable<FFXIVLog> GetLogs(byte[] logData, Encoding enc)
        {
            var buf = logData
                .SkipWhile(a => a != 0x30)
                .ToArray();
            if (logger.IsDebugEnabled) {
                logger.Debug(("DUMP:\r\n" + buf.DumpHexText()).Trim());
            }

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
            return GetLogs(buf, enc);
        }



        private byte[] GetLogData()
        {
            return GetLogData(EntryPoint, Size);
        }

        private byte[] GetLogData(IntPtr from, int size)
        {
            return FFXIV.ReadBytes(from, size);
        }
        private byte[] GetLogData(IntPtr from, IntPtr to)
        {
            return GetLogData(from, (int)to - (int)from);
        }


        /// <summary>ログ領域開始位置。</summary>
        public IntPtr EntryPoint { get { return GetEntryPoint(); } }

        /// <summary>ログ領域終了位置。</summary>
        public IntPtr TerminalPoint { get { return GetTerminalPoint(); } }

        /// <summary>ログ領域のサイズ。</summary>
        public int Size { get { return TerminalPoint.ToInt32() - EntryPoint.ToInt32(); } }


        private IntPtr GetTerminalPoint()
        {
            return (IntPtr)this.FFXIV.ReadInt32(Entry + 4);
        }
        private IntPtr GetEntryPoint()
        {
            return (IntPtr)FFXIV.ReadInt32(this.Entry);
        }

    }
}
