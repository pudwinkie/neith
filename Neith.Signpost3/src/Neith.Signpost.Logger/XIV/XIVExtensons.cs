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

        private static readonly XName _P = "p";
        private static readonly XName _B = "b";
        private static readonly XName _LI = "li";
        private static readonly XName _UL = "ul";
        private static readonly XName _DIV = "div";
        private static readonly XName _SPAN = "span";
        private static readonly XName _DATETIME = "datetime";
        private static readonly XName _TIME = "time";

        private static readonly XName _ITEMSCOPE = "itemscope";
        private static readonly XName _ITEMPROP = "itemprop";

        private static readonly XAttribute SCOPE = new XAttribute(_ITEMSCOPE, "");

        private static XAttribute PROP(object name) { return new XAttribute(_ITEMPROP, name); }

        private static XElement TIME(string name, DateTimeOffset time)
        {
            var t1 = time.ToUniversalTime();
            var t2 = time.ToLocalTime().ToString("G");
            return new XElement(
                _TIME,
                PROP(name),
                new XAttribute(_DATETIME, t1),
                t2);
        }

        private static XElement B(string name, object value)
        {
            return new XElement(
                _B,
                PROP(name),
                value);
        }
        private static XElement LI(string name, object value)
        {
            return new XElement(
                _B,
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
            var source = new XElement(_UL, PROP("source"), SCOPE,
                LI("message", item.Message),
                LI("actId", item.MessageTypeID),
                LI("action", item.MessageType.ToString()));

            return new XElement(_P, SCOPE,
                TIME("time", item.Time),
                new XElement(_SPAN,
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
