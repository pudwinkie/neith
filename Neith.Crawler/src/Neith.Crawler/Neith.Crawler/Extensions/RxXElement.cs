using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace System.Xml.Linq
{
    public static class RxXElement
    {

        public static IObservable<XElement> ToXHtmlElement(this IObservable<Stream> rxSt)
        {
            return rxSt.Select(LINQXElementExtensions.ToXHtmlElement);
        }

    }
}
