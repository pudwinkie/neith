using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neith.Logger.Model;
using Microsoft.Isam.Esent.Collections.Generic;

namespace Neith.Logger
{
    public static class LogIndexExtensions
    {
        public static IEnumerable<NeithLog> WhereKey(
            this LogIndex index,
            string key,LogStore store)
        {
            return index.Index
                .Where(a => a.Key.StartsWith(key))
                .Select(a => a.Value.ToLog(store));
        }

        public static NeithLog ToLog(this LogIndex.IndexValue value, LogStore store)
        {
            return store.Dic[value.UtcTime];
        }

    }
}