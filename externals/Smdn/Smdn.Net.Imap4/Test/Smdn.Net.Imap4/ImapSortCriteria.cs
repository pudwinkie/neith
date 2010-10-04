using System;
using NUnit.Framework;

namespace Smdn.Protocols.Imap4 {
  [TestFixture]
  public class ImapSortCriteriaTest {
    [Test]
    public void TestCombine()
    {
      var c1 = ImapSortCriteria.From;
      var c2 = ImapSortCriteria.ArrivalReverse;

      Assert.AreEqual("(FROM REVERSE ARRIVAL)", ImapSortCriteria.Combine(c1, c2).ToString());
      Assert.AreEqual("(FROM REVERSE ARRIVAL)", (c1 + c2).ToString());
      Assert.AreEqual("(FROM REVERSE ARRIVAL)", c1.CombineWith(c2).ToString());

      Assert.AreEqual("(REVERSE ARRIVAL FROM)", (c2 + c1).ToString());
    }
  }
}