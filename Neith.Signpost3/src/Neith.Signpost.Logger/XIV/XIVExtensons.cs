using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
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

        private static readonly XAttribute SCOPE = new XAttribute(XN.itemscope, "");

        private static XAttribute PROP(object name) { return new XAttribute(XN.itemprop, name); }

        private static XElement TIME(string name, DateTimeOffset time)
        {
            var t1 = time.ToUniversalTime();
            var t2 = time.ToLocalTime().ToString("G");
            return new XElement(
                XN.time,
                PROP(name),
                new XAttribute(XN.datetime, t1),
                t2);
        }

        private static XElement B(string name, object value)
        {
            return new XElement(
                XN.b,
                PROP(name),
                value);
        }
        private static XElement LI(string name, object value)
        {
            return new XElement(
                XN.b,
                PROP(name),
                value);
        }



        /// <summary>
        /// FFXIVLogオブジェクトをmicrodataに変換します。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static XElement ToMicroData(this FFXIVLog item)
        {
            var source = new XElement(XN.ul, PROP("source"), SCOPE,
                LI("message", item.Message),
                LI("actId", item.MessageTypeID),
                LI("action", item.MessageType.ToString()));

            return new XElement(XN.p, SCOPE,
                TIME("time", item.Time),
                new XElement(XN.span,
                    B("application", "XIVWathcer"),
                    B("sender", item.Who),
                    B("actId" , "0x" + (item.MessageTypeID.ToString("X")).PadLeft(4, '0')),
                    B("action", item.MessageType.ToString()),
                    B("body"  , item.Message)),
                source
                );
        }



    }
}
