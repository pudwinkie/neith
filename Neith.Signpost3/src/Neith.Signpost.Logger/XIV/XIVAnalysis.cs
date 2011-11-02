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
        /// <summary>
        /// 初期化。
        /// </summary>
        static XIVAnalysis()
        {
            InitConverters(Const.MEFContainer);
        }

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
