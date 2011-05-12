using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Neith.Util.Random;
using Neith.Logger.Model;

namespace Neith.Logger.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class ProtoTest
    {
        [Test]
        public void SerialTest1()
        {
            var item = NeithLog.Create();
            item.Host = "Host";
            item.Application = "あぷりけ～しょんなのよ";

            using (var st = new MemoryStream()) {
                st.Serialize(item);
                var buf = st.ToArray();
                st.Position = 0;
                var item2 = st.Deserialize<NeithLog>();
                item.Timestamp.Ticks.AreEqual(item2.Timestamp.Ticks);
                item.Host.AreEqual(item2.Host);
                item.Application.AreEqual(item2.Application);
            }
        }

        [Test]
        public void SerialTest2()
        {
            var qIN = EnRandomLog().Take(20).ToArray();
            var st = new MemoryStream();
            qIN.SerializeAll(st);
            st.Seek(0, SeekOrigin.Begin);
            var qOUT = st.EnDeserialize<NeithLog>().ToArray();
            Assert.IsTrue(qIN.SequenceEqual(qOUT));
        }

        private static IEnumerable<NeithLog> EnRandomLog()
        {
            var r = new Xorshift();
            while (true) {
                var log = NeithLog.Create();
                 log.Collector = r.NextText32();
                 log.Host = r.NextText32();
                 log.Pid = r.NextInt32();
                 log.Application = r.NextText32();
                 log.Domain  = r.NextText32();
                 log.User  = r.NextText32();
                 log.Analyzer  = r.NextText32();
                 log.AddRange(
                     r.EnTestPair()
                     .Take((int)(1 + r.NextFloat() * 16)));
                 yield return log;
            }
        }



    }

    internal static class ProtoTestExtensions
    {
        public static string NextText32(this RandomBase r)
        {
            return string.Concat(r.EnUInt32()
                .Select(a => a.ToString("X8"))
                .Take(4));
        }

        public static IEnumerable<KeyValuePair<string, string>> EnTestPair(this RandomBase r)
        {
            while (true) yield return
                  new KeyValuePair<string, string>(r.NextText32(), r.NextText32());
        }

        public static void AddRange<K, V>(this IDictionary<K, V> dic, IEnumerable<KeyValuePair<K, V>> items)
        {
            foreach (var pair in items) dic.Add(pair);
        }

    }
}