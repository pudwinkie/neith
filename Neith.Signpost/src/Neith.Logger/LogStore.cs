using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Neith.Util.Reflection;
using Neith.Logger.Model;
using Microsoft.Isam.Esent.Collections.Generic;

namespace Neith.Logger
{
    /// <summary>
    /// ログデータベースストア
    /// </summary>
    public class LogStore : IDisposable
    {
        /// <summary>インスタンス</summary>
        public static readonly LogStore Instance = new LogStore(Const.Folders.Log);

        /// <summary>格納ディレクトリ</summary>
        public string DataDir { get; private set; }

        /// <summary>格納ディレクトリ</summary>
        public string MastarDir { get { return DataDir.PathCombine("mastar").GetFullPath(); } }

        /// <summary>ログデータストアの本体</summary>
        public PersistentDictionary<DateTime, NeithLog> Dic { get; private set; }

        /// <summary>Index: Actor</summary>
        public LogIndex IndexActor { get; private set; }

        /// <summary>Index一覧</summary>
        public IEnumerable<LogIndex> Indexes { get; private set; }

        private LogStore(string dir)
        {
            DataDir = dir;
            Directory.CreateDirectory(MastarDir);
            Dic = new PersistentDictionary<DateTime, NeithLog>(MastarDir);

            // Index
            List<LogIndex> indexes = new List<LogIndex>();

            IndexActor = new LogIndex(DataDir, "actor", a => a.Actor); indexes.Add(IndexActor);

            Indexes = indexes.ToArray();
        }

        /// <summary>
        /// 解放処理。
        /// </summary>
        public void Dispose()
        {
            if (Dic == null) return;
            Dic.Dispose();
            Dic = null;
            foreach (var index in Indexes) index.Dispose();
        }


        /// <summary>
        /// ログの格納
        /// </summary>
        /// <param name="neithLog"></param>
        public void Store(NeithLog log)
        {
            Dic.Add(log.UtcTime, log);
            foreach (var index in Indexes) index.Store(log);
        }

        /// <summary>
        /// Indexの再構築
        /// </summary>
        /// <param name="neithLog"></param>
        public void ReIndex()
        {
            Indexes.AsParallel().ForAll(index =>
            {
                index.Index.Clear();
                foreach (var log in Dic.Values) index.Store(log);
            });
        }



    }
}