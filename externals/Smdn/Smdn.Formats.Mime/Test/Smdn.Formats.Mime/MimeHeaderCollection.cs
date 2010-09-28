using System;
using NUnit.Framework;

namespace Smdn.Formats.Mime {
  [TestFixture]
  public class MimeHeaderCollectionTests {
    /*
    [Test]
    public void TestMultipleHeader()
    {
      var message = 
        "MIME-Version: 1.0\r\n" +
        "Received: from A by B for C\r\n" +
        "Received: from X by Y for Z\r\n" +
        "Received: from 1 by 2 for 3\r\n";

      var mime = MimeMessage.Create(message);

      Assert.IsTrue(mime.Headers.Contains("Received"));
      Assert.AreEqual("from A by B for C", mime.Headers["Received"][0]);
      Assert.AreEqual("from X by Y for Z", mime.Headers["RECEIVED"][1]);
      Assert.AreEqual("from 1 by 2 for 3", mime.Headers["received"][2]);
    }
    */
  }
}
