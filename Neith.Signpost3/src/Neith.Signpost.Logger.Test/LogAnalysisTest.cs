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
        public void Test02()
        {
            var items = XIVExtensons.EnXElement(Const.ConvertLogPath, XN.p.LocalName)
                .Where(a => a.Attribute(XN.itemscope) != null)
                ;
            foreach (var src in items.ToSrcItem()) {
                src.mes.IsNot(null);

            }
        }





    }
}
