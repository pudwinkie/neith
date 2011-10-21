using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neith.Signpost.Logger.Model;
using Wintellect.Sterling.Database;

namespace Neith.Signpost.Logger
{
    public class LogDB : BaseDatabaseInstance
    {
        public override string Name { get { return "NeithLogDatabase"; } }

        public const string LOG_SENDER_TIME = "LOG_SENDER_TIME";

        protected override List<ITableDefinition> RegisterTables()
        {
            return new List<ITableDefinition> 
            {
                CreateTableDefinition<NeithLog,DateTime>(a=>a.Time)
                .WithIndex<NeithLog,string,DateTime>(LOG_SENDER_TIME,a=>a.Sender),
            };
        }
    }
}