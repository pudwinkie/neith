using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Neith.Util.Reflection;

namespace Study.Google.Test
{
    using NUnit.Framework;
    [TestFixture]
    public class MakeXFN
    {
        [Test]
        public void WriteXFN()
        {
            var json = JObject.Parse(MakeXFNConst.JsonDoc);
            var dllpath = AssemblyUtil.GetCallingAssemblyDirctory();
            var xfnPath = Path.Combine(dllpath
                , @"..", @"..", @"..", @"..", @"..", @".."
                , @"xfn");
            xfnPath = Path.GetFullPath(xfnPath);

            foreach (var item in EnJson(new string[] { }, json, null)) {
                var keys = item.Item1;
                var attr = item.Item2;
                var links = item.Item3;
                OutputHTML(xfnPath, keys, attr, links);
            }
        }

        private static void OutputHTML(string xfnPath, string[] keys, Tuple<string, int> attr, IEnumerable<Tuple<string, string, string>> links)
        {
            var title = attr.Item1;
            var level = attr.Item2;
            var indexPaths = keys.Concat(new[] { "index.html" });
            var paths = new[] { xfnPath }
                .Concat(indexPaths)
                .ToArray();
            var path = Path.GetFullPath(Path.Combine(paths));
            var uri = MakeXFNConst.XfnURI + string.Join("/", indexPaths);

            // リンクの作成
            var buf = new StringBuilder();
            foreach (var link in links) {
                var rel = link.Item1;
                var key = link.Item2;
                var name = link.Item3;
                var q1 = keys.Concat(new[] { key });
                if (rel == "parent") q1 = keys.Take(keys.Length - 1);
                var q2 = q1.Concat(new[] { "" });
                var linkUri = MakeXFNConst.XfnURI + string.Join("/", q2.ToArray());
                buf.AppendFormat(LinkTemplate, rel, linkUri, name);
            }
            var html = string.Format(Template, title, "/style.css", buf.ToString());
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, html);

        }

        private const string Template = @"<!DOCTYPE html>
<html lang=""ja"">
<head>
  <meta charset=""utf-8"">
  <title>{0}</title>
  <link rel=""stylesheet"" href=""{1}"">
</head>
<body>
<h1>{0}</h1>
{2}
</body>
</html>
";
        private const string LinkTemplate = @"<a rel=""{0}"" href=""{1}"">{2}</a>" + "\n";


        private IEnumerable<Tuple<
                string[],
                Tuple<string, int>,
                IEnumerable<Tuple<string, string, string>>
                >>
            EnJson(string[] baseKey, JObject json, JObject parent)
        {
            // リンク要素
            var items = EnXfnLinks(json, parent).ToArray();
            // 属性
            var attr = JsonAttr(json);
            // 自分を返す
            yield return new Tuple<
                string[],
                Tuple<string, int>,
                IEnumerable<Tuple<string, string, string>>>(
                    baseKey, attr, items);

            // 子要素を返す
            if (items.Length == 0) yield break;
            foreach (KeyValuePair<string, JToken> p in json) {
                if (p.Key == "$") continue;
                var childKey = baseKey.Concat(new[] { p.Key }).ToArray();
                var childJson = p.Value as JObject;
                foreach (var child in EnJson(childKey, childJson, json)) {
                    yield return child;
                }
            }
        }

        /// <summary>
        /// Xfnのリンク要素羅列を返す
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private IEnumerable<Tuple<string, string, string>> EnXfnLinks(JObject json, JObject parent)
        {
            if (parent != null) {
                var attr = JsonAttr(parent);
                yield return new Tuple<string, string, string>("parent", "", attr.Item1);
            }
            foreach (KeyValuePair<string, JToken> p in json) {
                if (p.Key == "$") continue;
                var attr = JsonAttr(p.Value);
                yield return new Tuple<string, string, string>("child", p.Key, attr.Item1);
            }
        }

        /// <summary>
        /// Jsonの属性要素を返す。
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private Tuple<string, int> JsonAttr(JToken token)
        {
            var attr = token.SelectToken("$").First();

            string ja = attr.Values<string>("ja")
                .DefaultIfEmpty("").First();
            int lv = attr.Values<int>("level")
                .DefaultIfEmpty(0).First();
            return new Tuple<string, int>(ja, lv);
        }


    }
}