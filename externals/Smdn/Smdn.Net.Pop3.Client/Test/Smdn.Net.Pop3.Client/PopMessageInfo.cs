using System;
using System.IO;
using NUnit.Framework;

using Smdn.Net.Pop3.Client.Session;

namespace Smdn.Net.Pop3.Client {
  [TestFixture]
  public class PopMessageInfoTests {
    private void TestMessage(Action<PopPseudoServer, PopMessageInfo> action)
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // STAT
        server.EnqueueResponse("+OK 1 120\r\n");
        // LIST
        server.EnqueueResponse("+OK 1 120\r\n");

        var message = client.GetMessage(1L);

        Assert.IsNotNull(message);
        Assert.AreEqual(1L, message.MessageNumber);
        Assert.AreEqual(120L, message.Length);
        Assert.IsFalse(message.IsMarkedAsDeleted);

        server.DequeueRequest();
        server.DequeueRequest();

        action(server, message);
      });
    }

    private void TestDeletedMessage(Action<PopPseudoServer, PopMessageInfo> action)
    {
      TestMessage(delegate(PopPseudoServer server, PopMessageInfo message) {
        // DELE
        server.EnqueueResponse("+OK\r\n");

        message.MarkAsDeleted();

        server.DequeueRequest(); // DELE

        Assert.IsTrue(message.IsMarkedAsDeleted);

        // STAT
        server.EnqueueResponse("+OK 0 0\r\n");

        Assert.AreEqual(0L, message.Client.MessageCount);
        Assert.AreEqual(0L, message.Client.TotalSize);

        server.DequeueRequest(); // STAT

        action(server, message);
      });
    }

    private void TestClosedSessionMessage(Action<PopMessageInfo> action)
    {
      TestMessage(delegate(PopPseudoServer server, PopMessageInfo message) {
        message.Client.Disconnect();

        action(message);
      });
    }

    private static void GetMessageBody(out string messageBody, out string messageByteStuffedBody, out int octets)
    {
      messageBody = @"MIME-Version: 1.0
Content-Type: text/plain

..byte-stuffed
+OK response style line
-ERR response style line2
.
end of message
".Replace("\r\n", "\n").Replace("\n", "\r\n");

      messageByteStuffedBody = messageBody.Replace("\r\n.", "\r\n..");

      octets = NetworkTransferEncoding.Transfer8Bit.GetByteCount(messageBody);
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestPropertyGetClientSessionClosed()
    {
      TestClosedSessionMessage(delegate(PopMessageInfo message) {
        Assert.IsNull(message.Client);
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestPropertyGetMessageNumberSessionClosed()
    {
      TestClosedSessionMessage(delegate(PopMessageInfo message) {
        Assert.AreEqual(0L, message.MessageNumber);
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestPropertyGetLengthSessionClosed()
    {
      TestClosedSessionMessage(delegate(PopMessageInfo message) {
        Assert.AreEqual(0L, message.Length);
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestPropertyGetIsMarkedAsDeletedSessionClosed()
    {
      TestClosedSessionMessage(delegate(PopMessageInfo message) {
        Assert.IsTrue(message.IsMarkedAsDeleted);
      });
    }

    [Test]
    public void TestPropertyGetUniqueId()
    {
      TestMessage(delegate(PopPseudoServer server, PopMessageInfo message) {
        // UIDL
        server.EnqueueResponse("+OK 1 whqtswO00WBw418f9t5JxYwZ\r\n");

        Assert.AreEqual("whqtswO00WBw418f9t5JxYwZ", message.UniqueId);

        Assert.AreEqual(server.DequeueRequest(), "UIDL 1\r\n");

        // retrieve again
        Assert.AreEqual("whqtswO00WBw418f9t5JxYwZ", message.UniqueId);
      });
    }

    [Test]
    public void TestPropertyGetUniqueIdDeleted()
    {
      TestDeletedMessage(delegate(PopPseudoServer server, PopMessageInfo message) {
        try {
          Assert.IsNotNull(message.UniqueId);
          Assert.Fail("PopMessageDeletedException not thrown");
        }
        catch (PopMessageDeletedException ex) {
          Assert.AreEqual(message.MessageNumber, ex.MessageNumber);
        }
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestPropertyGetUniqueIdSessionClosed()
    {
      TestClosedSessionMessage(delegate(PopMessageInfo message) {
        Assert.IsNotNull(message.UniqueId);
      });
    }

    [Test]
    public void TestMarkAsDeleted()
    {
      TestMessage(delegate(PopPseudoServer server, PopMessageInfo message) {
        // DELE
        server.EnqueueResponse("+OK\r\n");

        message.MarkAsDeleted();

        Assert.AreEqual(server.DequeueRequest(), "DELE 1\r\n");

        Assert.IsTrue(message.IsMarkedAsDeleted);

        // STAT
        server.EnqueueResponse("+OK 0 0\r\n");

        Assert.AreEqual(0L, message.Client.MessageCount);
        Assert.AreEqual(0L, message.Client.TotalSize);

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");

        // delete again
        message.MarkAsDeleted();

        Assert.IsTrue(message.IsMarkedAsDeleted);
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestMarkAsDeletedSessionClosed()
    {
      TestClosedSessionMessage(delegate(PopMessageInfo message) {
        message.MarkAsDeleted();
      });
    }

    [Test]
    public void TestOpenReadRetr()
    {
      TestMessage(delegate(PopPseudoServer server, PopMessageInfo message) {
        message.Client.DeleteAfterRetrieve = false;

        int octets;
        string messageBody, byteStuffedMessageBody;

        GetMessageBody(out messageBody, out byteStuffedMessageBody, out octets);

        // RETR
        server.EnqueueResponse("+OK\r\n" +
                               byteStuffedMessageBody +
                               ".\r\n");

        using (var stream = message.OpenRead()) {
          Assert.AreEqual(server.DequeueRequest(), "RETR 1\r\n");

          Assert.IsFalse(message.IsMarkedAsDeleted);

          Assert.IsNotNull(stream);
          Assert.AreEqual(octets, stream.Length);
          Assert.AreEqual(messageBody, (new StreamReader(stream, NetworkTransferEncoding.Transfer7Bit)).ReadToEnd());
        }
      });
    }

    [Test]
    public void TestOpenReadTop()
    {
      TestMessage(delegate(PopPseudoServer server, PopMessageInfo message) {
        message.Client.DeleteAfterRetrieve = false;

        int octets;
        string messageBody, byteStuffedMessageBody;

        GetMessageBody(out messageBody, out byteStuffedMessageBody, out octets);

        // RETR
        server.EnqueueResponse("+OK\r\n" +
                               byteStuffedMessageBody +
                               ".\r\n");

        using (var stream = message.OpenRead(3)) {
          Assert.AreEqual(server.DequeueRequest(), "TOP 1 3\r\n");

          Assert.IsFalse(message.IsMarkedAsDeleted);

          Assert.IsNotNull(stream);
          Assert.AreEqual(octets, stream.Length);
          Assert.AreEqual(messageBody, (new StreamReader(stream, NetworkTransferEncoding.Transfer7Bit)).ReadToEnd());
        }
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestOpenReadSessionClosed()
    {
      TestClosedSessionMessage(delegate(PopMessageInfo message) {
        Assert.IsNull(message.OpenRead());
      });
    }

    [Test]
    public void TestOpenReadDeleteAfterRetrieve()
    {
      TestMessage(delegate(PopPseudoServer server, PopMessageInfo message) {
        message.Client.DeleteAfterRetrieve = true;

        int octets;
        string messageBody, byteStuffedMessageBody;

        GetMessageBody(out messageBody, out byteStuffedMessageBody, out octets);

        // RETR
        server.EnqueueResponse("+OK\r\n" +
                               byteStuffedMessageBody +
                               ".\r\n");
        // DELE
        server.EnqueueResponse("+OK\r\n");

        using (var stream = message.OpenRead()) {
          Assert.AreEqual(server.DequeueRequest(), "RETR 1\r\n");
          Assert.AreEqual(server.DequeueRequest(), "DELE 1\r\n");

          Assert.IsTrue(message.IsMarkedAsDeleted);

          Assert.IsNotNull(stream);
          Assert.AreEqual(octets, stream.Length);
          Assert.AreEqual(messageBody, (new StreamReader(stream, NetworkTransferEncoding.Transfer7Bit)).ReadToEnd());

          // STAT
          server.EnqueueResponse("+OK 0 0\r\n");

          Assert.AreEqual(0L, message.Client.MessageCount);
          Assert.AreEqual(0L, message.Client.TotalSize);

          Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        }
      });
    }

    [Test]
    public void TestReadAllBytes()
    {
      TestMessage(delegate(PopPseudoServer server, PopMessageInfo message) {
        int octets;
        string messageBody, byteStuffedMessageBody;

        GetMessageBody(out messageBody, out byteStuffedMessageBody, out octets);

        // RETR
        server.EnqueueResponse("+OK\r\n" +
                               byteStuffedMessageBody +
                               ".\r\n");

        Assert.AreEqual(NetworkTransferEncoding.Transfer7Bit.GetBytes(messageBody),
                        message.ReadAllBytes());
      });
    }

    [Test]
    public void TestSave()
    {
      const string outfile = "retrieved.eml";

      try {
        if (File.Exists(outfile))
          File.Delete(outfile);

        TestMessage(delegate(PopPseudoServer server, PopMessageInfo message) {
          int octets;
          string messageBody, byteStuffedMessageBody;

          GetMessageBody(out messageBody, out byteStuffedMessageBody, out octets);

          // RETR
          server.EnqueueResponse("+OK\r\n" +
                                 byteStuffedMessageBody +
                                 ".\r\n");

          message.Save(outfile);

          Assert.IsTrue(File.Exists(outfile));
          Assert.AreEqual(octets, (new FileInfo(outfile)).Length);
          Assert.AreEqual(NetworkTransferEncoding.Transfer7Bit.GetBytes(messageBody),
                          File.ReadAllBytes(outfile));
        });
      }
      finally {
        if (File.Exists(outfile))
          File.Delete(outfile);
      }
    }

    [Test]
    public void TestSaveOpenArgumentExceptionFileNotCreate()
    {
      const string outfile = "retrieved.eml";

      try {
        if (File.Exists(outfile))
          File.Delete(outfile);

        TestMessage(delegate(PopPseudoServer server, PopMessageInfo message) {
          try {
            message.Save(outfile, -2);
            Assert.Fail("ArgumentException not throw");
          }
          catch (ArgumentException) {
          }

          Assert.IsFalse(File.Exists(outfile));
        });
      }
      finally {
        if (File.Exists(outfile))
          File.Delete(outfile);
      }
    }

    [Test]
    public void TestWriteToStream()
    {
      TestMessage(delegate(PopPseudoServer server, PopMessageInfo message) {
        int octets;
        string messageBody, byteStuffedMessageBody;

        GetMessageBody(out messageBody, out byteStuffedMessageBody, out octets);

        // RETR
        server.EnqueueResponse("+OK\r\n" +
                               byteStuffedMessageBody +
                               ".\r\n");

        var outputStream = new MemoryStream();

        message.WriteTo(outputStream);

        Assert.AreEqual(octets, outputStream.Length);

        outputStream.Close();

        Assert.AreEqual(NetworkTransferEncoding.Transfer7Bit.GetBytes(messageBody),
                        outputStream.ToArray());
      });
    }

    [Test]
    public void TestWriteToBinaryWriter()
    {
      TestMessage(delegate(PopPseudoServer server, PopMessageInfo message) {
        int octets;
        string messageBody, byteStuffedMessageBody;

        GetMessageBody(out messageBody, out byteStuffedMessageBody, out octets);

        // RETR
        server.EnqueueResponse("+OK\r\n" +
                               byteStuffedMessageBody +
                               ".\r\n");

        var outputStream = new MemoryStream();
        var writer = new BinaryWriter(outputStream);

        message.WriteTo(writer);

        Assert.AreEqual(octets, outputStream.Length);

        outputStream.Close();

        Assert.AreEqual(NetworkTransferEncoding.Transfer7Bit.GetBytes(messageBody),
                        outputStream.ToArray());
      });
    }

    [Test]
    public void TestReadAllLines()
    {
      TestMessage(delegate(PopPseudoServer server, PopMessageInfo message) {
        int octets;
        string messageBody, byteStuffedMessageBody;

        GetMessageBody(out messageBody, out byteStuffedMessageBody, out octets);

        // RETR
        server.EnqueueResponse("+OK\r\n" +
                               byteStuffedMessageBody +
                               ".\r\n");

        Assert.AreEqual(messageBody.TrimEnd(),
                        string.Join("\r\n", message.ReadAllLines()));
      });
    }

    [Test]
    public void TestReadAllText()
    {
      TestMessage(delegate(PopPseudoServer server, PopMessageInfo message) {
        int octets;
        string messageBody, byteStuffedMessageBody;

        GetMessageBody(out messageBody, out byteStuffedMessageBody, out octets);

        // RETR
        server.EnqueueResponse("+OK\r\n" +
                               byteStuffedMessageBody +
                               ".\r\n");

        Assert.AreEqual(messageBody,
                        message.ReadAllText());
      });
    }

    [Test]
    public void TestOpenReadDeleted()
    {
      TestDeletedMessage(delegate(PopPseudoServer server, PopMessageInfo message) {
        try {
          message.OpenRead();
          Assert.Fail("PopMessageDeletedException not thrown");
        }
        catch (PopMessageDeletedException ex) {
          Assert.AreEqual(message.MessageNumber, ex.MessageNumber);
        }
      });
    }
  }
}
