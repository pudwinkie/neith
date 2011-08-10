using System;
using System.Collections.Generic;
using System.Text;
using Neith.Util;

namespace Neith.Util.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class DateTimeUtilTest
    {
        [Test]
        public void GetUniqueTimeStampTest()
        {
            DateTime a = DateTimeUtil.GetUniqueTimeStamp();
            DateTime b = DateTimeUtil.GetUniqueTimeStamp();
            Assert.IsTrue(a < b);
        }
    }
}
