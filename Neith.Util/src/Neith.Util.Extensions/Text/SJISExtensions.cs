using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Text
{
    /// <summary>
    /// SJIS限定便利関数
    /// </summary>
    public static class SJISExtensions
    {
        /// <summary>
        /// SJISエンコーディングの実装。
        /// </summary>
        public static readonly Encoding CP932 = Encoding.GetEncoding(932);

        /// <summary>
        /// ShiftJISエンコーディング。
        /// </summary>
        public static Encoding ShiftJIS { get { return CP932; } }


        /// <summary>
        /// 入力文字列をSJIS文字列とみなし、指定半角文字列数となるように半角スペースを詰める。
        /// SJIS以外の文字列の場合は何も行わない。
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string PadRightSJIS(this string text, int length)
        {
            try {
                var padding = GetPadCount(text, length);
                if (padding <= 0) return text;
                return text + new String(' ', padding);
            }
            catch (EncoderFallbackException) {
                return text;
            }
        }

        /// <summary>
        /// 入力文字列をSJIS文字列とみなし、指定半角文字列数となるように半角スペースを詰める。
        /// SJIS以外の文字列の場合は何も行わない。
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string PadLeftSJIS(this string text, int length)
        {
            try {
                var padding = GetPadCount(text, length);
                if (padding <= 0) return text;
                return new String(' ', padding) + text;
            }
            catch (EncoderFallbackException) {
                return text;
            }
        }

        private static int GetPadCount(string text, int length)
        {
            var cnt = CP932.GetByteCount(text);
            var padding = length - cnt;
            return padding;
        }
    }
}
