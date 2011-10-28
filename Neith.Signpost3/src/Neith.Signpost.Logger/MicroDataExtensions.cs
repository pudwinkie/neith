using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Xml.Linq
{
    public static class MicroDataExtensions
    {
        public static class XN
        {
            public static readonly XName itemscope = "itemscope";
            public static readonly XName itemprop = "itemprop";
        }
        /// <summary>
        /// MicroDataのプロパティ一覧を返す。
        /// 但し、新たなitemscopeがあった場合、itemscopeの子は対象としない。
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static IEnumerable<XElement> ItemProperties(this XElement src)
        {
            foreach (var item in src.Elements() ) {
                var itemprop = item.Attribute(XN.itemprop);
                if (itemprop != null) yield return item;

                var itemscope = item.Attribute(XN.itemscope);
                if (itemscope != null) continue;

                foreach (var child in item.ItemProperties()) yield return child;
            }
        }

        public static IDictionary<string, XElement> ToItemPropertyDictionary(this  XElement src)
        {
            return src
                .ItemProperties()
                .ToDictionary(a => a.Attribute(XN.itemprop).Value, a => a);
        }


    }
}
