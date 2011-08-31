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
            var count = src.Select(a => a.Count).Sum();
            return src.ToCombineArray(count);
        }

        /// <summary>
        /// 指定長さの要素を切りだして返します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static T[] ToCombineArray<T>(this IEnumerable<ArraySegment<T>> src, int count)
        {
            var dst = new T[count];
            var dstOffset = 0;
            src.CopyTo(dst, dstOffset, count);
            return dst;
        }

        /// <summary>
        /// 指定した位置の要素を配列にコピーします。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="dstOffset"></param>
        /// <param name="count"></param>
        public static void CopyTo<T>(this IEnumerable<ArraySegment<T>> src, T[] dst, int dstOffset, int count)
        {
            foreach (var item in src) {
                var copyCount = count < item.Count ? count : item.Count;
                Buffer.BlockCopy(item.Array, item.Offset, dst, dstOffset, copyCount);
                count -= copyCount;
                if (count <= 0) break;
                dstOffset += item.Count;
            }
        }

        /// <summary>
        /// 全てを連結した要素を返します。
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static T[] ToCombineArray<T>(this IEnumerable<T[]> src)
        {
            var count = src.Select(a => a.Length).Sum();
            return src.ToCombineArray(count);
        }

        /// <summary>
        /// 指定長さの要素を切りだして返します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static T[] ToCombineArray<T>(this IEnumerable<T[]> src, int count)
        {
            var dst = new T[count];
            var dstOffset = 0;
            src.CopyTo(dst, dstOffset, count);
            return dst;
        }

        /// <summary>
        /// 指定した位置の要素を配列にコピーします。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="dstOffset"></param>
        /// <param name="count"></param>
        public static void CopyTo<T>(this IEnumerable<T[]> src, T[] dst, int dstOffset, int count)
        {
            foreach (var item in src) {
                var copyCount = count < item.Length ? count : item.Length;
                Buffer.BlockCopy(item, 0, dst, dstOffset, copyCount);
                count -= copyCount;
                if (count <= 0) break;
                dstOffset += item.Length;
            }
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

        /// <summary>
        /// 全てを連結したIListを返します。
        /// </summary>
        /// <param name="segs"></param>
        /// <returns></returns>
        public static SegList<T> ToList<T>(this IEnumerable<ArraySegment<T>> segs)
        {
            return new SegList<T>(segs);
        }

        /// <summary>
        /// ArraySegment連結リスト。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class SegList<T> : IList<T>
        {
            private readonly IEnumerable<ArraySegment<T>> Segs;
            private ArraySegment<T> Target;
            private int StartIndex = 0;
            private int EndIndex = -1;

            public int Count { get; private set; }

            public bool IsReadOnly{get{return true;}}

            public SegList(IEnumerable<ArraySegment<T>> segs)
            {
                Segs = segs;
                Count = 0;
                foreach (var seg in Segs) Count += seg.Count;
            }

            private bool Valid(int index)
            {
                return StartIndex <= index && index <= EndIndex;
            }

            private void SetTarget(int index)
            {
                StartIndex = 0;
                EndIndex = -1;
                foreach (var seg in Segs) {
                    Target = seg;
                    StartIndex = EndIndex + 1;
                    EndIndex = StartIndex + Target.Count - 1;
                    if (Valid(index)) return;
                }
                throw new ArgumentOutOfRangeException();
            }

            private T GetValue(int index)
            {
                if (!Valid(index)) SetTarget(index);
                return Target.Array[Target.Offset + index - StartIndex];
            }

            public IEnumerable<T> ToEnumerable()
            {
                foreach (var seg in Segs) {
                    var start = seg.Offset;
                    var stop = seg.Offset + seg.Count;
                    for (int i = start; i < stop; i++) yield return seg.Array[i];
                }
            }

            #region IList<T> メンバー

            public int IndexOf(T item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, T item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public T this[int index]
            {
                get { return GetValue(index); }
                set { throw new NotImplementedException(); }
            }

            #endregion

            #region ICollection<T> メンバー

            public void Add(T item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(T item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }


            public bool Remove(T item)
            {
                throw new NotImplementedException();
            }

            #endregion

            #region IEnumerable<T> メンバー

            public IEnumerator<T> GetEnumerator()
            {
                return ToEnumerable().GetEnumerator();
            }

            #endregion

            #region IEnumerable メンバー

            Collections.IEnumerator Collections.IEnumerable.GetEnumerator()
            {
                return ToEnumerable().GetEnumerator();
            }

            #endregion
        }

    }
}
