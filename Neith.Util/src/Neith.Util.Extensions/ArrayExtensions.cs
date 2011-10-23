using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
   public static class ArrayExtensions
    {
        /// <summary>
        /// 値を文字列に変換するデリゲート
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public delegate string ToStringDelgate<T>(T value);

        /// <summary>
        /// 列挙要素をダンプしてテキストに変換します。
        /// </summary>
        /// <param name="values"></param>
        /// <param name="toString"></param>
        /// <param name="lineFeedSpan"></param>
        /// <param name="lineHeaderFormat"></param>
        /// <returns></returns>
        public static string DumpText<T>(this IEnumerable<T> values, ToStringDelgate<T> toString, int lineFeedSpan, string lineHeaderFormat)
        {
            StringBuilder buf = new StringBuilder();
            int count = 0;
            foreach (T value in values)
            {
                if ((count % lineFeedSpan) == 0)
                {
                    if (count != 0) buf.AppendLine();
                    buf.AppendFormat(count.ToString(lineHeaderFormat) + ": ");
                }
                else
                {
                    buf.Append(' ');
                }
                count++;
                buf.AppendFormat(toString(value));
            }
            buf.AppendLine();
            return buf.ToString();
        }

        /// <summary>
        /// １６進ダンプのテキストを作成します。
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string DumpHexText<T>(this IEnumerable<T> values)
        {
            return DumpText(
              values, ToHex2Text<T>, 16, "X8");
        }

        private static string ToHex2Text<T>(T value)
        {
            return string.Format("{0:X2}", value);
        }

    }
}
