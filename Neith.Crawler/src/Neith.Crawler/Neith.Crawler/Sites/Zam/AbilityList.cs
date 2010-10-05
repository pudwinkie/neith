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
        private const string startURL = @"http://ffxiv.zam.com/ja/abilitylist.html";

        /// <summary>
        /// 実行タスクを定義します。
        /// </summary>
        /// <returns></returns>
        public static IObservable<Unit> Task()
        {
            return startURL
                .RxPageCrowl(GetNextPage, ParseTable);
        }

        /// <summary>
        /// 次ページへのリンクを探し、URLを返します。
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static string GetNextPage(XElement doc)
        {
            return doc
                .GetLinkUrlByClassName("non-box next")
                .MargeUri(startURL)
                ;
        }

        private static IObservable<Unit> ParseTable(IObservable<XElement> rxDoc)
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
                    return new Unit();
                })
                ;
        }


        private static readonly XNamespace ns = "http://www.w3.org/1999/xhtml";
    }

}