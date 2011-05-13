using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Logger.Model
{
    public static class NeithLogMixin
    {
        /// <summary>
        /// 要素を追加します。
        /// </summary>
        /// <param name="list"></param>
        /// <param name="item"></param>
        public static void Add(this ICollection<NeithLogVM> list, NeithLog item)
        {
            list.Add(new NeithLogVM(item));
        }

        /// <summary>
        /// 要素を追加します。
        /// </summary>
        /// <param name="list"></param>
        /// <param name="item"></param>
        public static void Add(this ICollection<NeithLogVM> list, IEnumerable<NeithLog> items)
        {
            foreach (var item in items) list.Add(item);
        }

    }
}
