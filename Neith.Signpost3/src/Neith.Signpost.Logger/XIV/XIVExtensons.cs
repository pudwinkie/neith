using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using FFXIVRuby;
using Neith.Signpost.Logger.Model;

namespace Neith.Signpost.Logger.XIV
{
    public static class XIVExtensons
    {
        /// <summary>
        /// FFXIVLogオブジェクトをログに変換します。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static async Task<NeithLog> ToNeithLog(this FFXIVLog item)
        {
            return new NeithLog
            {
                Time = item.Time,
                Application = "XIVWathcer",
                Sender = item.Who,
                ActionID = "0x" + (item.MessageTypeID.ToString("X")).PadLeft(4, '0'),
                Action = item.MessageType.ToString(),
                Body = item.Message,
                Source = item,
            };
        }

    }
}
