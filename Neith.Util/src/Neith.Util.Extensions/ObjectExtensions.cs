using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    /// <summary>
    /// オブジェクト拡張。
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// null値比較を行います。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNull(this object obj)
        {
            return obj == null;
        }
    }
}
