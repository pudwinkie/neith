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
        /// 全てを連結した要素を返します。
        /// </summary>
        /// <param name="arrays"></param>
        /// <returns></returns>
        public static T[] ToCombineArray<T>(this IEnumerable<T[]> arrays)
        {
            var size = arrays.Select(a => a.Length).Sum();
            var dst = new T[size];
            var dstOffset = 0;
            foreach (var src in arrays) {
                Buffer.BlockCopy(src, 0, dst, dstOffset, src.Length);
                dstOffset += src.Length;
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
            return src.ToArray(src.Count);
        }

        /// <summary>
        /// 単純配列にコピーして返します。
        /// </summary>
        /// <param name="src"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static T[] ToArray<T>(this ArraySegment<T> src, int count)
        {
            var dst = new T[count];
            Buffer.BlockCopy(src.Array, src.Offset, dst, 0, count);
            return dst;
        }

    }
}
