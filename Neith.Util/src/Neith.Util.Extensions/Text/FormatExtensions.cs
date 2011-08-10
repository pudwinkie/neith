using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Text
{
    /// <summary>
    /// string.Formatへの短縮構文
    /// </summary>
    public static class FormatExtensions
    {
        /// <summary>
        /// 指定した文字列の 1 つ以上の書式項目を、指定したオブジェクトの文字列形式に置換します。
        /// </summary>
        /// <param name="format">複合書式指定文字列。</param>
        /// <param name="arg0">書式指定するオブジェクト。</param>
        /// <returns>書式項目が arg0 の文字列形式に置換された format のコピー。</returns>
        public static string _(this string format, object arg0)
        {
            return string.Format(format, arg0);
        }

        /// <summary>
        /// 指定した文字列の書式項目を、指定した 2 つのオブジェクトの文字列形式に置換します。
        /// </summary>
        /// <param name="format">複合書式指定文字列。</param>
        /// <param name="arg0">書式指定する第 1 オブジェクト。</param>
        /// <param name="arg1">書式指定する第 2 オブジェクト。</param>
        /// <returns>書式項目が arg0 と arg1 の文字列形式に置換された format のコピー。</returns>
        public static string _(this string format, object arg0, object arg1)
        {
            return string.Format(format, arg0, arg1);
        }

        /// <summary>
        /// 指定した文字列の書式項目を、指定した 3 つのオブジェクトの文字列形式に置換します。
        /// </summary>
        /// <param name="format">複合書式指定文字列。</param>
        /// <param name="arg0">書式指定する第 1 オブジェクト。</param>
        /// <param name="arg1">書式指定する第 2 オブジェクト。</param>
        /// <param name="arg2">書式指定する第 3 オブジェクト。</param>
        /// <returns>書式項目が arg0、arg1、および arg2 の文字列形式に置換された format のコピー。</returns>
        public static string _(this string format, object arg0, object arg1, object arg2)
        {
            return string.Format(format, arg0, arg1, arg2);
        }

        /// <summary>
        /// 指定した文字列の書式項目を、指定した配列内の対応するオブジェクトの文字列形式に置換します。
        /// </summary>
        /// <param name="format">複合書式指定文字列。</param>
        /// <param name="args">0 個以上の書式設定対象オブジェクトを含んだオブジェクト配列。</param>
        /// <returns>書式項目が args の対応するオブジェクトの文字列形式に置換された format のコピー。</returns>
        public static string _(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

    }
}