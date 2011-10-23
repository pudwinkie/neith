using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Neith.Util
{
    /// <summary>
    /// 値値の配列走査のためのジェネリックユーティティです。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class ArrayUtil<T> where T : IEquatable<T>
    {
        /// <summary>
        /// お互いの要素がすべて一致したときにtrueを返します。
        /// いずれかがnullの場合は常にfalseを返します。
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public static bool Equals(T[] src, T[] dest)
        {
            if (src == null) return false;
            if (dest == null) return false;
            if (src.Length != dest.Length) return false;
            for (int i = 0; i < src.Length; i++) {
                if (!src[i].Equals(dest[i])) return false;
            }
            return true;
        }

        /// <summary>
        /// お互いの要素がすべて一致したときにtrueを返します。
        /// いずれかがnullの場合は常にfalseを返します。
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public static bool Equals(ICollection<T> src, ICollection<T> dest)
        {
            if (src == null) return false;
            if (dest == null) return false;
            if (src.Count != dest.Count) return false;
            return EqualsSubIEnumerable(src, dest);
        }

        /// <summary>
        /// お互いの要素がすべて一致したときにtrueを返します。
        /// いずれかがnullの場合は常にfalseを返します。
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public static bool Equals(IEnumerable<T> src, IEnumerable<T> dest)
        {
            if (src == null) return false;
            if (dest == null) return false;
            return EqualsSubIEnumerable(src, dest);
        }

        private static bool EqualsSubIEnumerable(IEnumerable<T> src, IEnumerable<T> dest)
        {
            IEnumerator<T> dEn = dest.GetEnumerator();
            foreach (T sVal in src) {
                if (!dEn.MoveNext()) return false;
                if (!sVal.Equals(dEn.Current)) return false;
            }
            return !dEn.MoveNext();
        }


        /// <summary>
        /// 配列をランダムにシャッフルします。
        /// </summary>
        /// <param name="array">シャッフルする配列</param>
        /// <returns></returns>
        public static T[] Shuffle(ref T[] array)
        {
            System.Random rand = new System.Random();
            for (int i = 0; i < array.Length; i++) {
                int dst = rand.Next(array.Length);
                T swap = array[i];
                array[i] = array[dst];
                array[dst] = swap;
            }
            return array;
        }


    }
}
