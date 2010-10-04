using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Study.Google.Test
{
    using NUnit.Framework;
    [TestFixture]
    public class JsonXML
    {
        private const string xmldoc =
@"<?xml version='1.0'?>
<names xmlns='http://www.piedey.co.jp/example/linqtoxml200811'>
  <name id='X'>Xenogears</name>
  <name id='M'>Mystic Quest</name>
  <name id='L'>LEGEND OF MANA</name>
</names>
";

        private const string jsondoc =
@"{
	""J"": ""hello1"",
	""S"": ""hello2"",
	""O STRING"": ""hello3"",
	""N ARRAY"": [""J"",""S"",""O"",""N\nN""]
}
";

        [Test]
        public void XMLTest()
        {
            var doc = XElement.Parse(xmldoc);
            XNamespace ex =
                "http://www.piedey.co.jp/example/linqtoxml200811";

            var query = from n in doc.Descendants(ex + "name")
                        where n.Attribute("id").Value == "M"
                        select n;

            query.Count().AreEqual(1);
            var item = query.FirstOrDefault();
            item.Value.AreEqual("Mystic Quest");
        }

        [Test]
        public void ToJsonTest()
        {
            using(var r = JsonReaderWriterFactory.CreateJsonReader(
                Encoding.UTF8.GetBytes(jsondoc),XmlDictionaryReaderQuotas.Max))
            using (var buf = new MemoryStream()) {
                using (var w = JsonReaderWriterFactory.CreateJsonWriter(buf)) {
                    w.WriteNode(r, true);
                }
                var text = Encoding.UTF8.GetString(buf.ToArray());
                Debug.WriteLine(text);

            }
            using (var r = JsonReaderWriterFactory.CreateJsonReader(
                Encoding.UTF8.GetBytes(jsondoc), XmlDictionaryReaderQuotas.Max)) {
                var xl = XElement.Load(r);
                Debug.WriteLine(xl);

                var J = (from a in xl.Elements("J") select a).First();
                Debug.WriteLine(J);

            }



        }

    }
}
