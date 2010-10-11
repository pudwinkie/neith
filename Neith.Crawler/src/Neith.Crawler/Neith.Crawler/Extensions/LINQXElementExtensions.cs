using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace System.Xml.Linq
{
    public static class LINQXElementExtensions
    {
        public static IEnumerable<XElement> Descendants(this IEnumerable<XElement> el, XName name)
        {
            return el
                .SelectMany(a => a.Descendants(name))
                ;
        }
    }
}