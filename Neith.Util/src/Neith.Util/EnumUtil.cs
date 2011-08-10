using System;
using System.Collections.Generic;
using System.Text;

namespace Neith.Util
{
    /// <summary>
    /// Enum操作ユーティリティ。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class EnumUtil<T>
    {
        /// <summary>
        /// 文字列をEnumに変換します。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T Parse(string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }
    }
}
