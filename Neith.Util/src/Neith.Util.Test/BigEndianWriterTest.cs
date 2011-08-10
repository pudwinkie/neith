using System;
using System.Collections.Generic;
using System.Text;
using Neith.Util.IO;
using System.IO;

namespace Neith.Util.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class BigEndianWriterTest
    {
        [Test]
        public void WriteTest()
        {
            List<byte> m = new List<byte>();
            MemoryStream ms = new MemoryStream();
            using (BigEndianWriter w = new BigEndianWriter(ms)) {
                w.Write((int)0x12345678);
                m.Add(0x12);
                m.Add(0x34);
                m.Add(0x56);
                m.Add(0x78);

                w.Write((short)0x2345);
                m.Add(0x23);
                m.Add(0x45);

                w.WriteBCD(12345678, 4);
                m.Add(0x12);
                m.Add(0x34);
                m.Add(0x56);
                m.Add(0x78);
            }
            Assert.AreEqual(m.ToArray(), ms.ToArray());
        }

    }
}
