using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NUnit.Framework
{
    public static class AssertExtensions
    {
        public static void AreEqual(this decimal actual, decimal expected) { Assert.AreEqual(expected, actual); }
        public static void AreEqual(this int actual, int expected) { Assert.AreEqual(expected, actual); }
        public static void AreEqual(this long actual, long expected) { Assert.AreEqual(expected, actual); }
        public static void AreEqual(this uint actual, uint expected) { Assert.AreEqual(expected, actual); }
        public static void AreEqual(this ulong actual, ulong expected) { Assert.AreEqual(expected, actual); }
        public static void AreEqual(this object actual, object expected) { Assert.AreEqual(expected, actual); }

        public static void AreEqual(this double actual, double expected, double delta) { Assert.AreEqual(expected, actual, delta); }
        public static void AreEqual(this float actual, float expected, float delta) { Assert.AreEqual(expected, actual, delta); }


        public static void AreNotEqual(this decimal actual, decimal expected) { Assert.AreNotEqual(expected, actual); }
        public static void AreNotEqual(this int actual, int expected) { Assert.AreNotEqual(expected, actual); }
        public static void AreNotEqual(this long actual, long expected) { Assert.AreNotEqual(expected, actual); }
        public static void AreNotEqual(this uint actual, uint expected) { Assert.AreNotEqual(expected, actual); }
        public static void AreNotEqual(this ulong actual, ulong expected) { Assert.AreNotEqual(expected, actual); }
        public static void AreNotEqual(this object actual, object expected) { Assert.AreNotEqual(expected, actual); }

    }
}
