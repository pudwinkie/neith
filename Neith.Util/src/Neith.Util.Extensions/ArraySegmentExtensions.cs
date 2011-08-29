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
        /// <param name="segs"></param>
        /// <returns></returns>
        public static T[] ToCombineArray<T>(this IEnumerable<ArraySegment<T>> segs)
        {
            var size = segs.Select(a => a.Count).Sum();
            var dst = new T[size];
            var dstOffset = 0;
            foreach (var src in segs) {
                Buffer.BlockCopy(src.Array, src.Offset, dst, dstOffset, src.Count);
                dstOffset += src.Count;
            }
            return dst;
        }

        /// <summary>
        /// 単純配列にコピーして返します。
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static T[] ToArray<T>(this ArraySegment<T> src)
        {
            var dst = new T[src.Count];
            Buffer.BlockCopy(src.Array, src.Offset, dst, 0, src.Count);
            return dst;
        }


    }
}
