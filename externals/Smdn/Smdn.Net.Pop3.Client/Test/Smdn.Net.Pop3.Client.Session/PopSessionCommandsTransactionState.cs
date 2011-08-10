using System;
using System.IO;
using NUnit.Framework;

namespace Smdn.Net.Pop3.Client.Session {
  [TestFixture]
  public class PopSessionCommandsTransactionStateTests : PopSessionTestsBase {
    [Test]
    public void TestNoOp()
    {
      Login(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+OK\r\n");

        Assert.IsTrue((bool)session.NoOp());

        StringAssert.AreEqualIgnoringCase("NOOP\r\n",
                                          server.DequeueRequest());
      });
    }

    [Test]
    public void TestRset()
    {
      Login(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+OK\r\n");

        Assert.IsTrue((bool)session.Rset());

        StringAssert.AreEqualIgnoringCase("RSET\r\n",
                                          server.DequeueRequest());
      });
    }

    [Test]
    public void TestStat()
    {
      Login(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+OK 2 320\r\n");

        PopDropListing dropListing;

        Assert.IsTrue((bool)session.Stat(out dropListing));

        StringAssert.AreEqualIgnoringCase("STAT\r\n",
                                          server.DequeueRequest());

        Assert.AreNotEqual(PopDropListing.Empty, dropListing);
        Assert.AreEqual(2L, dropListing.MessageCount);
        Assert.AreEqual(320L, dropListing.SizeInOctets);
      });
    }

    [Test]
    public void TestList()
    {
      Login(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+OK 2 messages (320 octets)\r\n" +
                               "1 120\r\n" + 
                               "2 200\r\n" + 
                               ".\r\n");

        PopScanListing[] scanListings;

        Assert.IsTrue((bool)session.List(out scanListings));

        StringAssert.AreEqualIgnoringCase("LIST\r\n",
                                          server.DequeueRequest());

        Assert.IsNotNull(scanListings);
        Assert.AreEqual(2, scanListings.Length);
        Assert.AreEqual(1L,   scanListings[0].MessageNumber);
        Assert.AreEqual(120L, scanListings[0].SizeInOctets);
        Assert.AreEqual(2L,   scanListings[1].MessageNumber);
        Assert.AreEqual(200L, scanListings[1].SizeInOctets);
      });
    }

    [Test]
    public void TestListWithMessageNumber()
    {
      Login(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+OK 2 200\r\n");

        PopScanListing scanListing;

        Assert.IsTrue((bool)session.List(2L, out scanListing));

        StringAssert.AreEqualIgnoringCase("LIST 2\r\n",
                                          server.DequeueRequest());

        Assert.AreNotEqual(PopScanListing.Invalid, scanListing);
        Assert.AreEqual(2L,   scanListing.MessageNumber);
        Assert.AreEqual(200L, scanListing.SizeInOctets);

        server.EnqueueResponse("-ERR no such message, only 2 messages in maildrop\r\n");

        Assert.IsFalse((bool)session.List(3L, out scanListing));

        StringAssert.AreEqualIgnoringCase("LIST 3\r\n",
                                          server.DequeueRequest());

        Assert.AreEqual(PopScanListing.Invalid, scanListing);
      });
    }

    [Test]
    public void TestUidl()
    {
      Login(new[] {PopCapability.Uidl}, delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+OK\r\n" +
                               "1 whqtswO00WBw418f9t5JxYwZ\r\n" + 
                               "2 QhdPYR:00WBw1Ph7x7\r\n" + 
                               ".\r\n");

        PopUniqueIdListing[] uidListings;

        Assert.IsTrue((bool)session.Uidl(out uidListings));

        StringAssert.AreEqualIgnoringCase("UIDL\r\n",
                                          server.DequeueRequest());

        Assert.IsNotNull(uidListings);
        Assert.AreEqual(2, uidListings.Length);
        Assert.AreEqual(1L, uidListings[0].MessageNumber);
        Assert.AreEqual("whqtswO00WBw418f9t5JxYwZ", uidListings[0].UniqueId);
        Assert.AreEqual(2L, uidListings[1].MessageNumber);
        Assert.AreEqual("QhdPYR:00WBw1Ph7x7", uidListings[1].UniqueId);
      });
    }

    [Test]
    public void TestUidlWithMessageNumber()
    {
      Login(new[] {PopCapability.Uidl}, delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+OK 2 QhdPYR:00WBw1Ph7x7\r\n");

        PopUniqueIdListing uidListing;

        Assert.IsTrue((bool)session.Uidl(2L, out uidListing));

        StringAssert.AreEqualIgnoringCase("UIDL 2\r\n",
                                          server.DequeueRequest());

        Assert.AreNotEqual(PopUniqueIdListing.Invalid, uidListing);
        Assert.AreEqual(2L, uidListing.MessageNumber);
        Assert.AreEqual("QhdPYR:00WBw1Ph7x7", uidListing.UniqueId);

        server.EnqueueResponse("-ERR no such message, only 2 messages in maildrop\r\n");

        Assert.IsFalse((bool)session.Uidl(3L, out uidListing));

        StringAssert.AreEqualIgnoringCase("UIDL 3\r\n",
                                          server.DequeueRequest());

        Assert.AreEqual(PopUniqueIdListing.Invalid,  uidListing);
      });
    }

    [Test, ExpectedException(typeof(PopIncapableException))]
    public void TestUidlIncapable()
    {
      Login(delegate(PopSession session, PopPseudoServer server) {
        PopUniqueIdListing uidListing;

        session.Uidl(1L, out uidListing);
      });
    }

    [Test]
    public void TestDele()
    {
      Login(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+OK message 1 deleted\r\n");

        Assert.IsTrue((bool)session.Dele(1L));

        StringAssert.AreEqualIgnoringCase("DELE 1\r\n",
                                          server.DequeueRequest());

        server.EnqueueResponse("-ERR no such message, only 2 messages in maildrop\r\n");

        Assert.IsFalse((bool)session.Dele(2L));

        StringAssert.AreEqualIgnoringCase("DELE 2\r\n",
                                          server.DequeueRequest());
      });
    }

    [Test]
    public void TestRetr()
    {
      Login(delegate(PopSession session, PopPseudoServer server) {
        var messageBody = "From: from\r\n" +
          "To: to\r\n" +
          "Subect: subject\r\n" +
          "Date: Mon, 1 Jan 2010 11:53:24 +0900\r\n" + 
          "\r\n" +
          "test message.\r\n";

        var expectedLength = (long)NetworkTransferEncoding.Transfer8Bit.GetByteCount(messageBody);

        server.EnqueueResponse(string.Format("+OK {0} octets\r\n{1}.\r\n", expectedLength, messageBody));

        Stream messageStream;

        Assert.IsTrue((bool)session.Retr(1L, out messageStream));

        StringAssert.AreEqualIgnoringCase("RETR 1\r\n",
                                          server.DequeueRequest());

        Assert.IsNotNull(messageStream);
        Assert.AreEqual(expectedLength, messageStream.Length);

        var reader = new StreamReader(messageStream, NetworkTransferEncoding.Transfer8Bit);

        Assert.AreEqual(messageBody, reader.ReadToEnd());

        messageStream.Close();


        server.EnqueueResponse("-ERR no such message, only 2 messages in maildrop\r\n");

        Assert.IsFalse((bool)session.Retr(3L, out messageStream));

        StringAssert.AreEqualIgnoringCase("RETR 3\r\n",
                                          server.DequeueRequest());

        Assert.IsNull(messageStream);
      });
    }

    [Test]
    public void TestRetrByteStuffed()
    {
      Login(delegate(PopSession session, PopPseudoServer server) {
        var messageBody = @"MIME-Version: 1.0
Content-Type: text/plain

..byte-stuffed
+OK response style line
-ERR response style line2
.
end of message
".Replace("\r\n", "\n").Replace("\n", "\r\n");

        var expectedLength = (long)NetworkTransferEncoding.Transfer8Bit.GetByteCount(messageBody);

        server.EnqueueResponse(string.Format("+OK {0} octets\r\n{1}.\r\n",
                                             expectedLength,
                                             messageBody.Replace("\r\n.", "\r\n..")));

        Stream messageStream;

        Assert.IsTrue((bool)session.Retr(1L, out messageStream));

        StringAssert.AreEqualIgnoringCase("RETR 1\r\n",
                                          server.DequeueRequest());

        Assert.IsNotNull(messageStream);
        Assert.AreEqual(expectedLength, messageStream.Length);

        var reader = new StreamReader(messageStream, NetworkTransferEncoding.Transfer8Bit);

        Assert.AreEqual(messageBody, reader.ReadToEnd());

        messageStream.Close();
      });
    }

    [Test]
    public void TestTop()
    {
      Login(new[] {PopCapability.Top}, delegate(PopSession session, PopPseudoServer server) {
        var messageBody = "From: from\r\n" +
          "To: to\r\n" +
          "Subect: subject\r\n" +
          "Date: Mon, 1 Jan 2010 11:53:24 +0900\r\n" + 
          "\r\n";

        server.EnqueueResponse(string.Format("+OK\r\n{0}.\r\n", messageBody));

        Stream messageStream;

        Assert.IsTrue((bool)session.Top(1L, 0, out messageStream));

        StringAssert.AreEqualIgnoringCase("TOP 1 0\r\n",
                                          server.DequeueRequest());

        Assert.IsNotNull(messageStream);

        var reader = new StreamReader(messageStream, NetworkTransferEncoding.Transfer8Bit);

        Assert.AreEqual(messageBody, reader.ReadToEnd());

        messageStream.Close();


        server.EnqueueResponse("-ERR no such message, only 2 messages in maildrop\r\n");

        Assert.IsFalse((bool)session.Top(3L, 10, out messageStream));

        StringAssert.AreEqualIgnoringCase("TOP 3 10\r\n",
                                          server.DequeueRequest());

        Assert.IsNull(messageStream);
      });
    }

    [Test, ExpectedException(typeof(PopIncapableException))]
    public void TestTopIncapable()
    {
      Login(delegate(PopSession session, PopPseudoServer server) {
        Stream messageStream;

        session.Top(1L, 0, out messageStream);
      });
    }
  }
}
