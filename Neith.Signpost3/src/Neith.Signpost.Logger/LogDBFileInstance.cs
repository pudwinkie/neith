using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Disposables;
using Wintellect.Sterling;
using Wintellect.Sterling.Keys;
using Neith.Sterling.Server.FileSystem;
using Neith.Signpost.Logger.Model;

namespace Neith.Signpost.Logger
{
    public class LogDBFileInstance
    {
        public string DBPath { get; private set; }

        public ISterlingDatabaseInstance Instance { get; private set; }

        public List<TableKey<NeithLog, DateTime>> AllLogsKV { get { return Instance.Query<NeithLog, DateTime>(); } }

        public LogDBFileInstance(SterlingEngine engine, DateTime date, PathProvider pathProvider)
        {
            DBPath = string.Format("logger/{0:yyyyMM}/", date.ToUniversalTime());
            var db = engine.SterlingDatabase;
            Instance = db.RegisterDatabase<LogDB>(new FileSystemDriver(pathProvider, DBPath));
        }

        public LogDBFileInstance(SterlingEngine engine, DateTime date, string rootPath)
            : this(engine, date, new PathProvider(rootPath))
        {
        }

    }
}
