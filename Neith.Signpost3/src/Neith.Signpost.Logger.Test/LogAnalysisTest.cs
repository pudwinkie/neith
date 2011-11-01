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
        public void Test01()
        {
            Debug.WriteLine("Test01");
            var items = XIVExtensons.EnXElement(Const.InputLogPath, XN.p.LocalName)
                .Where(a => a.Attribute(XN.itemscope) != null)
                ;
            var output = Convert2(items)
                .CreateLogDocument("log");
            output.Save(Const.ConvertLogPath);
        }

        private static IEnumerable<XElement> Convert2(IEnumerable<XElement> items)
        {
            var count = 0;
            foreach (var item in items) {
                var xiv = item.ToFFXIVLogOld();
                yield return xiv.ToMicroData();
                count++;
            }
            Debug.WriteLine(string.Format("items.Count={0}", count));
        }


        [Test]
        public void Test03()
        {
            var q1 = XIVExtensons.EnXElement(Const.ConvertLogPath, XN.p.LocalName)
                .Where(a => a.Attribute(XN.itemscope) != null)
                .ToSrcItem();
            XIVAnalysis.SrcItem ngSrc = null;

            var items = q1
                .Select(a =>
                {
                    var ng = a.AnalysisElement == null ? "!" : "";
                    var cv = a.AnalysisElement == null ? "" : a.AnalysisElement.ToString();
                    if (a.AnalysisElement == null && ngSrc == null) ngSrc = a;

                    return new string[] {
                        a.time.ToString("O"),
                        ng,
                        a.id.ToString(),
                        a.who,
                        a.mes,
                        cv,
                    };
                });
            var header = new string[] { "time", "!", "id", "who", "mes", "cv" };
            var csv = Enumerable
                .Repeat(header, 1)
                .Concat(items);

            Neith.Util.CsvUtil.WriteCsv(Const.ConvertCsvPath, csv);
            if (ngSrc != null) {
                Debug.WriteLine("■NG Item");
                Debug.WriteLine(ngSrc.InputElement.ToString());
            }
        }





    }
}