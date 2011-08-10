using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace System
{
    public static class ConvertExtensions
    {
        /// <summary>
        /// 16進文字列を数値に変換します。
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int HexToInt32(this string text)
        {
            return int.Parse(text, NumberStyles.AllowHexSpecifier);
        }

    }
}
