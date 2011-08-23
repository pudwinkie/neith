using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    /// <summary>
    /// ArraySegment拡張。
    /// </summary>
    public static class ArraySegmentExtensions
    {
        /// <summary>
        /// 全てを連結した要素を返します。
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static T[] ToCombineArray<T>(this IEnumerable<ArraySegment<T>> src)
        {
            var size = src.Select(a => a.Count).Sum();
            var buf = new T[size];
            var srcOffset = 0;
            foreach (var seg in src) {
                Buffer.BlockCopy(buf, srcOffset, seg.Array, seg.Offset, seg.Count);
                srcOffset += seg.Count;
            }
            return buf;
        }
    }
}
