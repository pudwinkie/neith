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
        private static readonly XNamespace ns = "http://www.w3.org/1999/xhtml";

        /// <summary>
        /// 実行タスクを定義します。
        /// </summary>
        /// <returns></returns>
        public static Unit Task()
        {
            startURL
                .EnPageCrowl(GetNextUrl)
                .AsParallel()
                .SelectMany(ParseList).NotNull()
                .Distinct()
                .Select(url => url.ToXHtmlElement()).NotNull()
                .ForAll(ParseItem)
                ;
            return new Unit();
        }

        /// <summary>
        /// 要素から次ページへのリンクを抽出して返します。
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static string GetNextUrl(this XElement doc)
        {
            return doc
                .GetLinkUrlByClassName("non-box next")
                .MargeUri(startURL)
                ;
        }

        /// <summary>
        /// 一覧ページより詳細ページへのリンクを抽出して返します。
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static IEnumerable<string> ParseList(this XElement doc)
        {
            // リンクタグ
            return doc
                .EnLinkUrlByClassName("icon-link")
                .MargeUri(startURL)
                ;
        }

        /// <summary>
        /// 詳細ページを解析します。
        /// </summary>
        /// <param name="doc"></param>
        private static void ParseItem(this XElement doc)
        {
            // アイテム情報の抽出処理

            
        }

    }

}