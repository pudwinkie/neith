using System;
using System.Collections.Generic;
using System.Text;
using Neith.Util.Configuration;

namespace Neith.Util.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class AppConfigUtilTest
    {
        [Test]
        public void Test1()
        {
            Assert.AreEqual("文字列", AppConfigUtil.GetValueString("Util_Test1_STR"));

        }

    }

}
