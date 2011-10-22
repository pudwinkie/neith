using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neith.Signpost.Logger.Model;
using FFXIVRuby;

namespace Neith.Signpost.Logger.XIV
{
    public static class XIVExtensons
    {
        public static NeithLog ToNeithLog(this FFXIVLog item)
        {
            return new NeithLog
            {
                Time = item.Time,
                Application = "XIVWathcer",
                Sender = item.Who,
                Action = item.MessageType.ToString(),
                Body = item.Message,
                Source = item,
            };
        }

    }
}
