using System;
using System.Collections.Generic;
using System.Text;
using CSUtil.Configuration;

namespace CSUtil.Test
{
  using NUnit.Framework;

  [TestFixture]
  public class AppConfigUtilTest
  {
    [Test]
    public void Test1()
    {
      Assert.AreEqual("文字列", AppConfigUtil.GetValueString("CSUtil_Test1_STR"));

    }

  }
}
