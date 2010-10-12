using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Sgml;
using System.IO;

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

        private static readonly XNamespace ns = "http://www.w3.org/1999/xhtml";


        public static XElement ToXHtmlElement(this Stream st)
        {
            using (st)
            using (var reader = new StreamReader(st, Encoding.UTF8))
            using (var sgml = new SgmlReader()) {
                sgml.DocType = "HTML";
                sgml.CaseFolding = CaseFolding.ToLower;
                sgml.InputStream = reader;
                return XDocument.Load(sgml).Root;
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
        /// 指定されたクラス名を持つaタグを列挙します。
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public static IEnumerable<string> EnLinkUrlByClassName(this IEnumerable<XElement> doc, string className)
        {
            return doc
                .SelectMany(d => d.EnLinkUrlByClassName(className))
                ;
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

        /// <summary>
        /// target Uriをsrc Uriからの相対パスと解釈し、絶対Uriを合成します。
        /// </summary>
        /// <param name="relative"></param>
        /// <param name="src"></param>
        /// <returns></returns>
        public static IEnumerable<string> MargeUri(
           this IEnumerable<string> relative, string src)
        {
            return relative
                .Select(r => r.MargeUri(src))
                ;
        }



    }
}