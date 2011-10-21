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

        public LogDBService Service { get; private set; }

        public ISterlingDatabaseInstance Instance { get; private set; }

        public List<TableKey<NeithLog, DateTime>> AllLogsKV { get { return Instance.Query<NeithLog, DateTime>(); } }

        internal LogDBFileInstance(LogDBService service, DateTime date)
        {
            Service = service;
            DBPath = string.Format("logger/{0:yyyyMM}/{0:dd}/", date.ToUniversalTime());
            var db = Service.DBEngine.SterlingDatabase;
            Instance = db.RegisterDatabase<LogDB>(new FileSystemDriver(DBPath));
        }

    }
}
