using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Neith.Logger.Model;
using Microsoft.Isam.Esent.Collections.Generic;

namespace Neith.Logger
{
    public sealed class LogIndex : IDisposable
    {
        [Serializable]
        public struct IndexValue
        {
           public DateTime UtcTime;
        }

        public string Name { get; private set; }
        public string IndexDir { get; private set; }
        public PersistentDictionary<string, IndexValue> Index { get; private set; }
        public Func<NeithLog, string> GetKeyFunc { get; private set; }


        public LogIndex(LogStore store, string name, Func<NeithLog, string> getKeyFunc)
        {
            Name = name;
            GetKeyFunc = getKeyFunc;
            IndexDir = store.DataDir.PathCombine("index", name).GetFullPath();
            Index = new PersistentDictionary<string, IndexValue>(IndexDir);
        }

        public void Dispose()
        {
            Index.Dispose();
        }

        public string CreateKey(NeithLog log)
        {
            var key = GetKeyFunc(log);
            if (string.IsNullOrWhiteSpace(key)) return null;
            key = key.Replace('\u0001', ' ');
            var buf = BitConverter.GetBytes(log.UtcTime.ToBinary());
            return key + '\u0001' + Convert.ToBase64String(buf);
        }

        public void Store(NeithLog log)
        {
            var key = CreateKey(log);
            if (key == null) return;
            Index.Add(CreateKey(log), new IndexValue() { UtcTime = log.UtcTime });
        }

    }
}