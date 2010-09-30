using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Diagnostics;
using Neith.Crawler.Util;

namespace Neith.Crawler.Sites.Neith
{
    public static class NeithXFN
    {
        public static IObservable<bool> ReadTypes()
        {
            return @"http://spreadsheets.google.com/pub?key=0AlnLTLNQTaTJdGFZb1c2RTFuV01fUnBxbThNaGpWUXc&single=true&gid=0&output=csv"
                .RxGetUpdateWebResponseStream()
                .Select(st => {
                    if (st == null) {
                        Debug.WriteLine("[NeithXFN::ReadTypes] False!");
                        return false;
                    }
                    var qType = from a in CsvUtil.ReadCsv(st, Encoding.UTF8).FitCsv()
                                let el = new TypeElement(a)
                                select el;
                    // ここで読み切る
                    using (st) qType = qType.ToArray();

                    // ツリー構造の解析
                    var qKeys = from a in qType group a by a.ParentKey;
                    var qParent = from c in qKeys
                                  join p in qType on c.Key equals p.Key
                                  select new { p.ParentKey, Parent = p, Children = c };
                    foreach (var pair in qParent) {
                        var p = pair.Parent;
                        foreach (var child in pair.Children) {
                            child.Parent = p;
                            p.Children.Add(child);
                        }
                    }

                    // ファイル出力
                    foreach (var el in qType) {
                        var path = el.GetPath(Const.NeithXFNTypesDir);
                        Directory.CreateDirectory(path.ToDirectoryName());
                        File.WriteAllText(path, el.ToHTML());
                    }
                    return true;
                });
        }

        private static IEnumerable<string[]> FitCsv(this IEnumerable<string[]> csv)
        {
            var key = new string[] { "", "", "" };
            foreach (var line in csv.Skip(1)) {
                bool isFound = false;
                for (int i = 0; i < key.Length; i++) {
                    if (!isFound) {
                        var item = (line[i]).Trim();
                        if (!string.IsNullOrEmpty(item)) {
                            isFound = true;
                            key[i] = item;
                        }
                        line[i] = key[i];
                    }
                    else line[i] = "";
                }
                yield return line;
            }
        }

        private class TypeElement
        {
            public string Key { get; private set; }
            public string[] KeyArray { get; private set; }
            public int Level { get { return KeyArray.Length; } }

            public string URL
            {
                get
                {
                    return "http://xfn.vbel.net" + Key;
                }
            }
            
            public string KeyName { get; private set; }
            public string Name { get; private set; }
            public string NameEn { get; private set; }
            public string NameEnRead { get; private set; }

            public string ParentKey { get; private set; }
            public TypeElement Parent { get; set; }

            public IList<TypeElement> Children { get; private set; }

            public TypeElement(string[] csv)
                : base()
            {
                Children = new List<TypeElement>();
                ParentKey = "";
                Key = "/";
                KeyName = "root";
                var keyArray = new List<string>();
                for (int i = 0; i < 3; i++) {
                    var item = csv[i];
                    if (string.IsNullOrEmpty(item)) break;
                    ParentKey = Key;
                    Key += item + "/";
                    keyArray.Add(item);
                    KeyName = item;
                }
                KeyArray = keyArray.ToArray();
                Name = csv[3];
                NameEn = csv.Length > 4 ? csv[4] : "";
                NameEnRead = csv.Length > 5 ? csv[5] : "";
            }

            public override string ToString()
            {
                return string.Format("{0,-24}:{1}"
                    , Key, Name, ParentKey);
            }

            public string GetPath(string basePath)
            {
                return basePath
                    .PathConbine(KeyArray)
                    .PathConbine("index.html")
                    .ToFullPath();
            }

            public string GetLink(string rel)
            {
                return string.Format(@"<a rel=""{0}"" href=""{1}"">{2}</a>"
                       , rel, URL, Name.ToHtmlEncode());
            }

            public string ToHTML()
            {
                var format =
@"<!DOCTYPE html>
<html lang=""ja"">
<head>
  <meta charset=""utf-8"">
  <title>{1}</title>
  <link rel=""stylesheet"" href=""http://xfn.vbel.net/NeithXFN.css"">
</head>
<body class=""{0}"">
<h1>{1}</h1>
<ul class=""parent"">{2}</ul>
<ul class=""child"">{3}</ul>
<address class=""note"">
  presented by
  <a href=""http://twitter.jp/lucia_neith_vb/"">Lucia@VesperBell</a> 
  under
  <a rel=""license"" href=""http://creativecommons.org/publicdomain/zero/1.0/""> 
  <img src=""http://i.creativecommons.org/l/zero/1.0/80x15.png""/> 
  </a>
</address>
</body>
</html>";
                // bodyのclass
                var bodyClass = "globalNeithXFN NeithXFN " + KeyName;

                // 名前
                var name = Name.ToHtmlEncode();

                // 親リスト
                var parentBuf = new StringBuilder();
                parentBuf.AppendLine("\n<li>" + name + "</li>");
                foreach (var item in EnParentHTML()) 
                    parentBuf.AppendLine("<li>" + item + "</li>");

                // 子リスト
                var childBuf = new StringBuilder();
                childBuf.AppendLine();
                foreach (var item in from c in Children select c.GetLink("child"))
                    childBuf.AppendLine("<li>" + item + "</li>");

                // 合成
                return format
                    .FormatText(bodyClass, name, parentBuf.ToString(), childBuf.ToString());
            }

            private IEnumerable<string> EnParentHTML()
            {
                return EnParentHTML(1);
            }

            private IEnumerable<string> EnParentHTML(int level)
            {
                if (Parent == null) yield break;
                var rel = "kin";
                if (level == 1) rel = "parent";
                yield return Parent.GetLink(rel);
                foreach (var item in Parent.EnParentHTML(level + 1)) yield return item;
            }


        }

    }
}
