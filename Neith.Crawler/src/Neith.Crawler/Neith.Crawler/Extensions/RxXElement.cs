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
        private static readonly XNamespace ns = "http://www.w3.org/1999/xhtml";

        public static IObservable<XElement> ToXHtmlElement(this IObservable<Stream> rxSt)
        {
            return rxSt.Select(ToXHtmlElement);
        }

        public static XElement ToXHtmlElement(this Stream st)
        {
            using (st)
            using (var reader = new StreamReader(st, Encoding.UTF8)) {
                var src = reader.ReadToEnd();
                Debug.WriteLine("################################ HTML READ START");
                Debug.WriteLine(src);
                Debug.WriteLine("################################ HTML READ END");
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

        /// <summary>
        /// 指定されたクラス名を持つaタグを列挙します。
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public static IEnumerable<string> EnLinkUrlByClassName(this XElement doc, string className)
        {
            return from a in doc.Descendants(ns + "a")
                   where (string)a.Attribute("class") == className
                   select a into a
                   select (string)a.Attribute("href");
        }

        /// <summary>
        /// 指定されたクラス名を持つaタグを検索します。
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public static string GetLinkUrlByClassName(this XElement doc, string className)
        {
            return doc
                .EnLinkUrlByClassName(className)
                .FirstOrDefault();
        }

        /// <summary>
        /// target Uriをsrc Uriからの相対パスと解釈し、絶対Uriを合成します。
        /// </summary>
        /// <param name="relative"></param>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string MargeUri(this string relative, string src)
        {
            if (relative == null) return null;
            var uri = new Uri(src);
            var newUri = new Uri(uri, relative);
            return newUri.AbsoluteUri;
        }


    }
}
