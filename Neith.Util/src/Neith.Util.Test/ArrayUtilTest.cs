using System;
using System.Collections.Generic;
using System.Text;
using Neith.Util;

namespace Neith.Util.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class ArrayUtilTest
    {
        [Test]
        public void EqualsTest()
        {
            byte[] src = null;
            byte[] dest = null;
            Assert.IsFalse(ArrayUtil<byte>.Equals(src, dest));

            src = new byte[] { 1, 2, 3, 4, 5 };
            Assert.IsFalse(ArrayUtil<byte>.Equals(src, dest));

            dest = new byte[] { 1, 2, 3, 4, 5 };
            Assert.IsTrue(ArrayUtil<byte>.Equals(src, dest));
        }

        [Test]
        public void EqualsTest2()
        {
            IEnumerable<byte> src = null;
            byte[] dest = null;
            Assert.IsFalse(ArrayUtil<byte>.Equals(src, dest));

            src = new byte[] { 1, 2, 3, 4, 5 };
            Assert.IsFalse(ArrayUtil<byte>.Equals(src, dest));

            dest = new byte[] { 1, 2, 3, 4, 5 };
            Assert.IsTrue(ArrayUtil<byte>.Equals(src, dest));
        }

        [Test]
        public void DumpTest()
        {
            byte[] src = new byte[18];
            for (int i = 0; i < src.Length; i++) src[i] = (byte)i;
            string match = "00000000: 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F\r\n00000010: 10 11\r\n";

            Assert.AreEqual(match, ArrayUtil<byte>.DumpHexText(src));

        }

        [Test]
        public void RefTest()
        {
            byte[] src = new byte[] { 1, 2, 3, 4, 5 };
            byte[] dest = new byte[] { 6, 7, 8, 9, 10 };
            RefTestSub(src, dest);
            Assert.AreEqual(src, dest);
        }

        private static void RefTestSub(byte[] src, byte[] dest)
        {
            for (int i = 0; i < src.Length; i++) dest[i] = src[i];
        }

    }
}
