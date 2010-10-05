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
        private static readonly XNamespace ns = "http://www.w3.org/1999/xhtml";

        /// <summary>
        /// 実行タスクを定義します。
        /// </summary>
        /// <returns></returns>
        public static IObservable<bool> Task()
        {
            return @"http://ffxiv.zam.com/ja/abilitylist.html"
                .RxPageCrowl(GetNextPage, ParseTable);
        }

        private static string GetNextPage(XElement doc)
        {
            // 次のページがあればURLを返す
            var next = (from a in doc.Descendants(ns + "a")
                        where (string)a.Attribute("class") == "non-box next"
                        select a).FirstOrDefault();
            if (next != null) {
                Debug.WriteLine("################################ AbilityList START");
                Debug.WriteLine("NEXT要素：");
                Debug.WriteLine(next.ToString());
                Debug.WriteLine("################################ AbilityList END");
            }
            return null;
        }

        private static IObservable<bool> ParseTable(IObservable<XElement> rxDoc)
        {
            return rxDoc
                .Select(doc => {
                    // データテーブルの抽出処理
                    var data = from a in doc.Descendants(ns + "table")
                               where (string)a.Attribute("class") == "datatable sortable"
                               select a;
                    Debug.WriteLine("################################ AbilityList START");
                    Debug.WriteLine("データテーブル抽出数：" + data.Count().ToString());
                    Debug.WriteLine("################################ AbilityList END");
                    return true;
                })
                ;
        }

    }

}