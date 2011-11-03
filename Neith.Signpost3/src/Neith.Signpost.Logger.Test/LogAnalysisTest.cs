using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using Neith.Signpost.Logger.XIV;
using Neith.Signpost.Logger.Model;

namespace Neith.Signpost.Logger.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class LogAnalysisTest
    {
        [Test]
        public void Test03_2Analysis()
        {
            var q1 = XIVExtensons.EnXElement(Const.InputLogPath, XN.p.LocalName)
                .Where(a => a.Attribute(XN.itemscope) != null)
                .ToSrcItem();
            SrcItem ngSrc = null;

            var items = q1
                .Select(a =>
                {
                    var ng = a.AnalysisElement == null ? "!" : "";
                    var cv = a.AnalysisElement == null ? "" : a.AnalysisElement.ToString();
                    if (a.AnalysisElement == null && ngSrc == null) ngSrc = a;

                    var rc = new string[] {
                        a.time.ToString("O"),
                        ng,
                        a.id.ToString(),
                        a.who,
                        a.mes,
                        cv,
                    };
                    foreach (var item in rc) item.IsNot(null);
                    return rc;
                });
            var header = new string[] { "time", "!", "id", "who", "mes", "cv" };
            var csv = Enumerable
                .Repeat(header, 1)
                .Concat(items);

            Neith.Util.CsvUtil.WriteCsv(Const.ConvertCsvPath, csv);

            // 結果の情報
            if (ngSrc != null)
            {
                Debug.WriteLine("■NG Item");
                Debug.WriteLine(ngSrc.InputElement.ToString());
            }
            {
                Debug.WriteLine("■呼び出しが０のモジュール");
                var qModule = XIVAnalysis.AnalysisModules
                    .Where(a => a.CallCount == 0)
                    .OrderByDescending(a => a.CallCount);
                foreach (var item in qModule)
                {
                    Debug.WriteLine(item);
                }
            }
#if false
            {
                Debug.WriteLine("■モジュール呼び出し回数");
                var qModule = XIVAnalysis.AnalysisModules
                    .Where(a => a.CallCount > 0)
                    .OrderByDescending(a => a.CallCount);
                foreach (var item in qModule) {
                    Debug.WriteLine(item);
                }
            }
#endif

        }





    }
}