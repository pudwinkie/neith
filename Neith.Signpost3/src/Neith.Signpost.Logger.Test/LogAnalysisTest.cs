﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;

namespace Neith.Signpost.Logger.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class LogAnalysisTest
    {
        private static Stream GetLogStream()
        {
            var log = File.OpenRead(Const.XmlLogPath);
            var mem = new MemoryStream(Encoding.UTF8.GetBytes("</body></html>"));
            return new CombineReadStream(log, mem);
        }




        [Test]
        public void ReadTest()
        {
            var setting = new XmlReaderSettings
            {
                DtdProcessing = System.Xml.DtdProcessing.Parse,
                IgnoreComments = true,

            };
            using (var st = GetLogStream())
            using (var reader = XmlReader.Create(st, setting)) {
                var doc = XDocument.Load(reader);
                var items = doc
                    .Descendants(XN.p)
                    .Where(a => a.Attribute(XN.itemscope) != null)
                    ;
                Debug.WriteLine(string.Format("items.Count={0}", items.Count()));
                foreach (var item in items) {
                    var property = item.ToItemPropertyDictionary();
                    var source = property["source"];
                    if (source == null) continue;
                    var srcProp = source.ToItemPropertyDictionary();
                    var actId = srcProp["actId"];
                }
            }
        }






    }
}