using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Neith.Signpost.Logger.XIV
{
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
        public string idAct(string keys = "")
        {
            var text = XIVAnalysis.AnalysisIdDic[id] + " " + keys;
            var items = text
                .Split(' ')
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Distinct();
            var rc = string.Join(" ", items.ToArray());
            return rc;
        }

        // 出力情報
        public XElement AnalysisElement { get; set; }
    }
}
