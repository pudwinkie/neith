using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
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
                XN.li,
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
                TIME("time", item.Time),
                LI("id", item.MessageTypeID),
                LI("who", item.Who),
                LI("mes", item.Message));

            return new XElement(XN.p, SCOPE,
                TIME("time", item.Time),
                B("application", "XIVWathcer"),
                B("sender", item.Who),
                B("actId", "0x" + (item.MessageTypeID.ToString("X")).PadLeft(4, '0')),
                B("action", item.MessageType.ToString()),
                B("body", item.Message),
                source
                );
        }


        /// <summary>
        /// microdataをToFFXIVLogに変換します。
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public static FFXIVLog ToFFXIVLogOld(this XElement microdata)
        {
            var p = microdata.ToItemPropertyDictionary();
            var time = (DateTime)(p["time"].Attribute(XN.datetime));
            var source = p["source"];
            var sp = source.ToItemPropertyDictionary();
            var item = new FFXIVLog
            {
                Time = time,
                MessageTypeID = int.Parse(sp["actId"].Value),
                Who = p["sender"].Value,
                Message = sp["message"].Value
            };
            return item;
        }





        /// <summary>
        /// log一括書き込み用の
        /// </summary>
        /// <param name="items"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static XDocument CreateLogDocument(this IEnumerable<XElement> items, string title)
        {
            return new XDocument(
                new XDocumentType("html", null, null, null),
                new XElement(XN.html,
                    new XElement(XN.head,
                        new XElement(XN.meta, new XAttribute(XN.charset, "utf-8")),
                        new XElement(XN.title, title)),
                        new XElement(XN.link,
                            new XAttribute(XN.rel, "stylesheet"),
                            new XAttribute(XN.href, "../microdata.css")),
                    new XStreamingElement(XN.body, items)));
        }

        /// <summary>
        /// 指定エレメント名のXElementを読めるだけ読み切ります。
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IEnumerable<XElement> EnXElement(string uri, string key)
        {
            using (XmlReader reader = XmlReader.Create(uri, xmlSetting)) {
                reader.MoveToContent();
                while (true) {
                    try {
                        if (!reader.Read()) yield break;
                    }
                    catch (XmlException) { yield break; }
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == key) {
                        yield return XElement.ReadFrom(reader) as XElement;
                    }
                }
            }
        }
        private static readonly XmlReaderSettings xmlSetting = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Parse,
        };


    }
}
