using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Neith.Util.Runtime;

namespace Neith.Util.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class SerializeTest
    {
        [Serializable]
        public class TestValueClass
        {
            public int 漢字のフィールド;
            public TestValueClass()
            {
            }
            public TestValueClass(int v)
            {
                漢字のフィールド = v;
            }
        }

        [Test]
        public void Test1()
        {
            System.Random rand = new System.Random();
            TestValueClass target = new TestValueClass(rand.Next(int.MaxValue));

            string path1 = "SerializeTest_Test.gz";
            File.Delete(path1);
            SerializeUtil.PackedBinary.Save<TestValueClass>(target, path1);
            TestValueClass r1 = SerializeUtil.PackedBinary.Load<TestValueClass>(path1);
            Assert.AreEqual(target.漢字のフィールド, r1.漢字のフィールド);

            string path2 = "SerializeTest_Test.xml";
            File.Delete(path2);
            SerializeUtil.XML.Save<TestValueClass>(target, path2);
            TestValueClass r2 = SerializeUtil.XML.Load<TestValueClass>(path2);
            Assert.AreEqual(target.漢字のフィールド, r2.漢字のフィールド);
        }


    }
}
