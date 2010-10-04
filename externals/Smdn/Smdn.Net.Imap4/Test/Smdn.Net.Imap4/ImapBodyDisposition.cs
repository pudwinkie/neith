using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapBodyDispositionTests {
    [Test]
    public void TestConstruct()
    {
      var disp = new ImapBodyDisposition("attachment", new Dictionary<string, string>() {
        {"filename", "test.txt"},
      });

      Assert.AreEqual("attachment", disp.Type);
      Assert.IsTrue(disp.IsAttachment);
      Assert.IsNotNull(disp.Parameters);
      Assert.AreEqual(1, disp.Parameters.Count);
      Assert.AreEqual("test.txt", disp.Filename);

      disp = new ImapBodyDisposition("inline", null);

      Assert.AreEqual("inline", disp.Type);
      Assert.IsTrue(disp.IsInline);
      Assert.IsNotNull(disp.Parameters);
      Assert.AreEqual(0, disp.Parameters.Count);
      Assert.IsNull(disp.Filename);
    }
  }
}
