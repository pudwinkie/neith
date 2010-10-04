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
            return rxSt.Select(ToXHtmlElement);
        }

        public static XElement ToXHtmlElement(this Stream st)
        {
            using (st)
            using (var reader = new StreamReader(st, Encoding.UTF8)) {
                var src = reader.ReadToEnd();
                //Debug.WriteLine("################################ HTML READ START");
                //Debug.WriteLine(src);
                //Debug.WriteLine("################################ HTML READ END");
                var text = src
                    .Replace("&nbsp;", "&#32;")
                    .Replace("&laquo;", "&#171;")
                    .Replace("&raquo;", "&#187;")
                    ;
                try {
                    return XElement.Parse(text);
                }
                catch (Exception) {
                    Debug.WriteLine("XHTML 読み込み失敗！");
                    Debug.WriteLine("################################ HTML READ START");
                    Debug.WriteLine(text);
                    Debug.WriteLine("################################ HTML READ END");
                    throw;
                }
            }
        }



    }
}
