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

        public static IObservable<bool> Task()
        {
            var baseURL = @"http://ffxiv.zam.com/ja/abilitylist.html";

            return baseURL
                .RxGetCrowlUpdate()
                .ToResponseStream()
                .ToXHtmlElement()
                .Select(doc => {
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
                .TakeLast(0)
                ;
        }



        public static IObservable<bool> Task2()
        {
            var baseURL = @"http://ffxiv.zam.com/ja/abilitylist.html";

            var getSub = new AsyncSubject<string>();

            
            var parseSub = new AsyncSubject<XElement>();






            return parseSub.Select(ParseTask);
        }

        /*
        private static IObservable<bool> TaskGet(this IObservable<string> rxUrl
            , IObserver<XElement> parseTask
            , IObserver<string> getTask)
        {
            return rxUrl
                .ToUpdateWebResponseStream()
                .ToXHtmlElement()
                .Do(parseTask.OnNext)
                .Select(doc => {
                    // 次のデータは？
                    var nextURL = GetNextURL(doc);
                    if (nextURL != null) getTask.OnNext(nextURL);
                    else {
                        getTask.OnCompleted();
                        parseTask.OnCompleted();
                    }
                    return true;
                });
        }
        */
        private static string GetNextURL(XElement doc)
        {
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

        /// <summary>
        /// テーブルの解析
        /// </summary>
        /// <param name="rxTable"></param>
        /// <returns></returns>
        public static bool ParseTask(XElement doc)
        {
            // データテーブルの抽出
            var data = from a in doc.Descendants(ns + "table")
                       where (string)a.Attribute("class") == "datatable sortable"
                       select a;
            Debug.WriteLine("################################ AbilityList START");
            Debug.WriteLine("データテーブル抽出数：" + data.Count().ToString());
            Debug.WriteLine("################################ AbilityList END");
            return true;
        }

    }

}