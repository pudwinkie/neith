using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Signpost.Test
{
    using NUnit.Framework;
    [TestFixture]
    public class ByteArrayTest : AssertionHelper
    {
        [Test]
        public void TestByteArray()
        {
            var src = Enumerable.Range(0, 256).Select(a => (byte)a).ToArray();
            for (var i = 0; i < 256; i++) {
                var a = SubArray1(src, i);
                var b = SubArray2(src, i);
                Expect(a.SequenceEqual(b));
            }
        }

        public static byte[] SubArray1(byte[] src, int length)
        {
            var dst = new byte[length];
            Array.Copy(src, dst, length);
            return dst;
        }
        public static byte[] SubArray2(byte[] src, int length)
        {
            return src.Take(length).ToArray();
        }

    }
}
