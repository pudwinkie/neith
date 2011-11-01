using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Neith.Signpost.Logger.XIV
{
    public static partial class XIVAnalysis
    {
        /// <summary>入力情報</summary>
        public class SrcItem
        {
            // 入力XML情報
            public XElement InputElement { get; set; }
            public IDictionary<string, XElement> Property { get; set; }
            public IDictionary<string, XElement> Source { get; set; }

            // 入力情報
            public DateTimeOffset time { get; set; }
            public int id { get; set; }
            public string who { get; set; }
            public string mes { get; set; }

            // 計算
            public string idAct { get { return AnalysisIdDic[id]; } }

            // 出力情報
            public XElement AnalysisElement { get; set; }
        }

        /// <summary>コンバートモジュール</summary>
        public class ConvertModule
        {
            public Regex Regex { get; private set; }
            public Func<SrcItem, Match, XElement> Calc { get; private set; }
            public ConvertModule(string text, Func<SrcItem, Match, XElement> func)
            {
                Regex = new Regex("^" + text + "$", RegexOptions.Compiled);
                Calc = func;
            }
        }

        #region 簡略タグ作成コマンド群
        public static XAttribute SCOPE { get { return XIVExtensons.SCOPE; } }
        public static XElement B(string name, object value) { return XIVExtensons.B(name, value); }
        public static XElement I(string name, object value) { return XIVExtensons.I(name, value); }
        public static XElement LI(string name, object value) { return XIVExtensons.LI(name, value); }
        public static XElement META(string name, object value) { return XIVExtensons.META(name, value); }

        public static XElement SPAN(params object[] values) { return XIVExtensons.SPAN(values); }
        public static XAttribute PROP(object name) { return XIVExtensons.PROP(name); }
        public static XElement TIME(string name, DateTimeOffset time) { return XIVExtensons.TIME(name, time); }
        public static XElement ACT(object value) { return XIVExtensons.B("action", value); }

        #endregion


        public static IEnumerable<SrcItem> ToSrcItem(this IEnumerable<XElement> items)
        {
            return items
                .ToSrcItemImpl()
                .Select(item =>
                {
                    if (item.Source == null) return item;
                    try {
                        item.time = (DateTimeOffset)(item.Source["time"].Attribute(XN.datetime));
                        item.id = int.Parse(item.Source["id"].Value);
                        item.who = item.Source["who"].Value;
                        item.mes = item.Source["mes"].Value;
                        // コンバート
                        if (!AnalysisModulesDic.ContainsKey(item.id)) return item;
                        var cvItems = AnalysisModulesDic[item.id];

                        var mm = cvItems
                            .Select(cv => new { cv, m = cv.Regex.Match(item.mes) })
                            .Where(a => a.m.Success)
                            .FirstOrDefault();
                        if (mm == null) return item;

                        item.AnalysisElement = mm.cv.Calc(item, mm.m);

                        // 終了
                        return item;
                    }
                    catch { return item; }
                });
        }


        private static IEnumerable<SrcItem> ToSrcItemImpl(this IEnumerable<XElement> items)
        {
            return items.Select(item =>
            {
                var property = item.ToItemPropertyDictionary();
                var rc = new SrcItem
                {
                    InputElement = item,
                    Property = property,
                };
                var source = property["source"];
                if (source == null) return rc;
                rc.Source = source.ToItemPropertyDictionary();
                return rc;
            });
        }


    }
}
