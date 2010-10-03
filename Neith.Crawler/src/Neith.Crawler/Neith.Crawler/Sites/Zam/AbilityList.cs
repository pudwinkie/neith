using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Text;

namespace Neith.Crawler.Sites.Zam
{
    public static class AbilityList
    {
        public static IObservable<bool> Task()
        {
            XNamespace ns = "http://www.w3.org/1999/xhtml";
            var baseURL = @"http://ffxiv.zam.com/ja/abilitylist.html";

            return baseURL
                .RxGetUpdateWebResponseStream()
                .ToXHtmlElement()
                .Select(doc =>
                {
                    // 次の要素を抽出
                    var next = (from a in doc.Descendants(ns + "a")
                                where (string)a.Attribute("class") == "non-box next"
                                select a).FirstOrDefault();
                    if (next != null) {
                        Debug.WriteLine("################################ AbilityList START");
                        Debug.WriteLine("NEXT要素：");
                        Debug.WriteLine(next.ToString());
                        Debug.WriteLine("################################ AbilityList END");
                    }

                    // データテーブルの抽出
                    var data = from a in doc.Descendants(ns + "table")
                               where (string)a.Attribute("class") == "datatable sortable"
                               select a;
                    Debug.WriteLine("################################ AbilityList START");
                    Debug.WriteLine("データテーブル抽出数：" + data.Count().ToString());
                    Debug.WriteLine("################################ AbilityList END");
                    return true;
                })
                .TakeLast(0);
        }

    }

}