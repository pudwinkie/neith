using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Neith.Logger.Model;
using ProtoBuf;

namespace Neith.Logger.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class ProtoTest
    {
        [Test]
        public void SerialTest1()
        {
            var item = new Log();
            item.Timestamp = DateTimeOffset.Now;
            item.Host = "Host";
            item.Pid = 123;
            item.Application = "あぷりけ～しょんなのよ";

            using (var st = new MemoryStream()) {
                Serializer.Serialize(st, item);
                var buf = st.ToArray();
                st.Position = 0;
                var item2 = Serializer.Deserialize<Log>(st);
                item.Timestamp.Ticks.AreEqual(item2.Timestamp.Ticks);
                item.Host.AreEqual(item2.Host);
                item.Pid.AreEqual(item2.Pid);
                item.Application.AreEqual(item2.Application);
            }
        }
    }
}
