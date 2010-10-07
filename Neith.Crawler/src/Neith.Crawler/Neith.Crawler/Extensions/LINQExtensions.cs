using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Linq
{
    public static class LINQExtensions
    {
        /// <summary>
        /// Null値以外の要素を通します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="q"></param>
        /// <returns></returns>
        public static ParallelQuery<T> NotNull<T>(this ParallelQuery<T> q)
            where T : class
        {
            return q.Where(v => v != null);
        }

        /// <summary>
        /// Null値以外の要素を通します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="q"></param>
        /// <returns></returns>
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> q)
            where T : class
        {
            return q.Where(v => v != null);
        }

    }
}
