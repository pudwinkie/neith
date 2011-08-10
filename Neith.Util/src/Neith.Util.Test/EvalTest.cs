using System;
using System.Collections.Generic;
using System.Text;
using Neith.Util.CodeDom;

namespace Neith.Util.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class EvalTest
    {
        [Test]
        public void Test1()
        {
            Assert.AreEqual(7, JSEval.Eval("1+2*3"));
            Assert.AreEqual(4, JSEval.Eval("var a=14;var b=10; a-b;"));
            Assert.AreEqual(1, JSEval.EvalTarget(this, "target.TestValue -99"));
        }

        public int TestValue { get { return 100; } }
    }
}
