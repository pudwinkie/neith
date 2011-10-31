using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;

namespace Neith.Signpost.Logger.XIV
{
    public static class XIVAnalysis
    {
        /// <summary>入力情報</summary>
        public class SrcItem
        {
            public DateTimeOffset time { get; set; }
            public int id { get; set; }
            public string who { get; set; }
            public string mes { get; set; }

            public XElement InputElement { get; set; }
            public IDictionary<string, XElement> Property { get; set; }
            public IDictionary<string, XElement> Source { get; set; }
        }

        public static IEnumerable<SrcItem> ToSrcItem(this IEnumerable<XElement> items)
        {
            return items
                .ToSrcItemImpl()
                .Select(a =>
                {
                    if (a.Source == null) return a;
                    try {
                        a.time = (DateTimeOffset)(a.Source["time"].Attribute(XN.datetime));
                        a.id = int.Parse(a.Source["id"].Value);
                        a.who = a.Source["who"].Value;
                        a.mes = a.Source["mes"].Value;
                        return a;
                    }
                    catch { return a; }
                });
        }


        private static IEnumerable<SrcItem> ToSrcItemImpl(this IEnumerable<XElement> items)
        {
            return items.Select(item =>
            {
                var property = item.ToItemPropertyDictionary();
                var rc = new SrcItem
                {
                    InputElement = item,
                    Property = property,
                };
                var source = property["source"];
                if (source == null) return rc;
                rc.Source = source.ToItemPropertyDictionary();
                return rc;
            });
        }





    }
}
