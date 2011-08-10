using System;
using System.Text;
using System.IO;
using NUnit.Framework;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif
using Smdn.IO;
using Smdn.Net.Pop3.Client.Session;

namespace Smdn.Net.Pop3.Client {
  [TestFixture]
  public class PopClientOperationsDownloadTests {
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

    private void TestMessage(Action<PopPseudoServer, PopClient, string> action)
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        int octets;
        string messageBody, byteStuffedMessageBody;

        GetMessageBody(out messageBody, out byteStuffedMessageBody, out octets);

        // STAT
        server.EnqueueResponse("+OK 3 200\r\n");
        // LIST
        server.EnqueueResponse("+OK 3 200\r\n");
        // RETR
        server.EnqueueResponse("+OK\r\n" +
                               byteStuffedMessageBody +
                               ".\r\n");

        action(server, client, messageBody);

        // QUIT
        server.EnqueueResponse("+OK\r\n");

        client.Disconnect(true);

        Assert.AreEqual(server.DequeueRequest(), "QUIT\r\n");
      });
    }

    private void TestFirstMessage(Action<PopPseudoServer, PopClient, string> action)
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        int octets;
        string messageBody, byteStuffedMessageBody;

        GetMessageBody(out messageBody, out byteStuffedMessageBody, out octets);

        // STAT
        server.EnqueueResponse("+OK 5 1200\r\n");
        // LIST
        server.EnqueueResponse("+OK 1 200\r\n");
        // RETR
        server.EnqueueResponse("+OK\r\n" +
                               byteStuffedMessageBody +
                               ".\r\n");

        action(server, client, messageBody);

        // QUIT
        server.EnqueueResponse("+OK\r\n");

        client.Disconnect(true);

        Assert.AreEqual(server.DequeueRequest(), "QUIT\r\n");
      });
    }

    private void TestLastMessage(Action<PopPseudoServer, PopClient, string> action)
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        int octets;
        string messageBody, byteStuffedMessageBody;

        GetMessageBody(out messageBody, out byteStuffedMessageBody, out octets);

        // STAT
        server.EnqueueResponse("+OK 5 1200\r\n");
        // LIST
        server.EnqueueResponse("+OK 5 200\r\n");
        // RETR
        server.EnqueueResponse("+OK\r\n" +
                               byteStuffedMessageBody +
                               ".\r\n");

        action(server, client, messageBody);

        // QUIT
        server.EnqueueResponse("+OK\r\n");

        client.Disconnect(true);

        Assert.AreEqual(server.DequeueRequest(), "QUIT\r\n");
      });
    }

    [Test]
    public void TestDownloadMessageAsText()
    {
      TestMessage(delegate(PopPseudoServer server, PopClient client, string expectedResult) {
        Assert.AreEqual(expectedResult,
                        client.DownloadMessageAsText(3L, Encoding.UTF8));

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 3\r\n");
        Assert.AreEqual(server.DequeueRequest(), "RETR 3\r\n");
      });
    }

    [Test]
    public void TestDownloadFirstMessageAsText()
    {
      TestFirstMessage(delegate(PopPseudoServer server, PopClient client, string expectedResult) {
        Assert.AreEqual(expectedResult,
                        client.DownloadFirstMessageAsText(Encoding.UTF8));

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 1\r\n");
        Assert.AreEqual(server.DequeueRequest(), "RETR 1\r\n");
      });
    }

    [Test]
    public void TestDownloadLastMessageAsText()
    {
      TestLastMessage(delegate(PopPseudoServer server, PopClient client, string expectedResult) {
        Assert.AreEqual(expectedResult,
                        client.DownloadLastMessageAsText(Encoding.UTF8));

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 5\r\n");
        Assert.AreEqual(server.DequeueRequest(), "RETR 5\r\n");
      });
    }

    [Test]
    public void TestDownloadMessageAsByteArray()
    {
      TestMessage(delegate(PopPseudoServer server, PopClient client, string expectedResult) {
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(expectedResult),
                                  client.DownloadMessageAsByteArray(3L));

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 3\r\n");
        Assert.AreEqual(server.DequeueRequest(), "RETR 3\r\n");
      });
    }

    [Test]
    public void TestDownloadFirstMessageAsByteArray()
    {
      TestFirstMessage(delegate(PopPseudoServer server, PopClient client, string expectedResult) {
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(expectedResult),
                                  client.DownloadFirstMessageAsByteArray());

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 1\r\n");
        Assert.AreEqual(server.DequeueRequest(), "RETR 1\r\n");
      });
    }

    [Test]
    public void TestDownloadLastMessageAsByteArray()
    {
      TestLastMessage(delegate(PopPseudoServer server, PopClient client, string expectedResult) {
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(expectedResult),
                                  client.DownloadLastMessageAsByteArray());

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 5\r\n");
        Assert.AreEqual(server.DequeueRequest(), "RETR 5\r\n");
      });
    }

    [Test]
    public void TestDownloadMessageAsStream()
    {
      TestMessage(delegate(PopPseudoServer server, PopClient client, string expectedResult) {
        using (var stream = client.DownloadMessageAsStream(3L)) {
          FileAssert.AreEqual(new MemoryStream(Encoding.UTF8.GetBytes(expectedResult)),
                              stream);
        }

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 3\r\n");
        Assert.AreEqual(server.DequeueRequest(), "RETR 3\r\n");
      });
    }

    [Test]
    public void TestDownloadFirstMessageAsStream()
    {
      TestFirstMessage(delegate(PopPseudoServer server, PopClient client, string expectedResult) {
        using (var stream = client.DownloadFirstMessageAsStream()) {
          FileAssert.AreEqual(new MemoryStream(Encoding.UTF8.GetBytes(expectedResult)),
                              stream);
        }

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 1\r\n");
        Assert.AreEqual(server.DequeueRequest(), "RETR 1\r\n");
      });
    }

    [Test]
    public void TestDownloadLastMessageAsStream()
    {
      TestLastMessage(delegate(PopPseudoServer server, PopClient client, string expectedResult) {
        using (var stream = client.DownloadLastMessageAsStream()) {
          FileAssert.AreEqual(new MemoryStream(Encoding.UTF8.GetBytes(expectedResult)),
                              stream);
        }

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 5\r\n");
        Assert.AreEqual(server.DequeueRequest(), "RETR 5\r\n");
      });
    }

    [Test]
    public void TestDownloadMessageAsConverter()
    {
      TestMessage(delegate(PopPseudoServer server, PopClient client, string expectedResult) {
        var ret = client.DownloadMessageAs(3L, delegate(Stream stream) {
          return stream.ReadToEnd().Length;
        });

        Assert.AreEqual(Encoding.UTF8.GetByteCount(expectedResult),
                        ret);

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 3\r\n");
        Assert.AreEqual(server.DequeueRequest(), "RETR 3\r\n");
      });
    }

    [Test]
    public void TestDownloadFirstMessageAsConverter()
    {
      TestFirstMessage(delegate(PopPseudoServer server, PopClient client, string expectedResult) {
        var ret = client.DownloadFirstMessageAs(delegate(Stream stream) {
          return stream.ReadToEnd().Length;
        });

        Assert.AreEqual(Encoding.UTF8.GetByteCount(expectedResult),
                        ret);

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 1\r\n");
        Assert.AreEqual(server.DequeueRequest(), "RETR 1\r\n");
      });
    }

    [Test]
    public void TestDownloadLastMessageAsConverter()
    {
      TestLastMessage(delegate(PopPseudoServer server, PopClient client, string expectedResult) {
        var ret = client.DownloadLastMessageAs(delegate(Stream stream) {
          return stream.ReadToEnd().Length;
        });

        Assert.AreEqual(Encoding.UTF8.GetByteCount(expectedResult),
                        ret);

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 5\r\n");
        Assert.AreEqual(server.DequeueRequest(), "RETR 5\r\n");
      });
    }

    [Test]
    public void TestDownloadMessageAsFunc()
    {
      TestMessage(delegate(PopPseudoServer server, PopClient client, string expectedResult) {
        var ret = client.DownloadMessageAs(3L, delegate(Stream stream, string arg) {
          Assert.AreEqual(arg, "arg");

          return stream.ReadToEnd().Length;
        }, "arg");

        Assert.AreEqual(Encoding.UTF8.GetByteCount(expectedResult),
                        ret);

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 3\r\n");
        Assert.AreEqual(server.DequeueRequest(), "RETR 3\r\n");
      });
    }

    [Test]
    public void TestDownloadFirstMessageAsFunc()
    {
      TestFirstMessage(delegate(PopPseudoServer server, PopClient client, string expectedResult) {
        var ret = client.DownloadFirstMessageAs(delegate(Stream stream, string arg) {
          Assert.AreEqual(arg, "arg");

          return stream.ReadToEnd().Length;
        }, "arg");

        Assert.AreEqual(Encoding.UTF8.GetByteCount(expectedResult),
                        ret);

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 1\r\n");
        Assert.AreEqual(server.DequeueRequest(), "RETR 1\r\n");
      });
    }

    [Test]
    public void TestDownloadLastMessageAsFunc()
    {
      TestLastMessage(delegate(PopPseudoServer server, PopClient client, string expectedResult) {
        var ret = client.DownloadLastMessageAs(delegate(Stream stream, string arg) {
          Assert.AreEqual(arg, "arg");

          return stream.ReadToEnd().Length;
        }, "arg");

        Assert.AreEqual(Encoding.UTF8.GetByteCount(expectedResult),
                        ret);

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 5\r\n");
        Assert.AreEqual(server.DequeueRequest(), "RETR 5\r\n");
      });
    }

    private void TestFile(Action<string, string> action)
    {
      const string expectedFile = "expected.eml";
      const string actualFile = "actual.eml";

      try {
        if (File.Exists(expectedFile))
          File.Delete(expectedFile);
        if (File.Exists(actualFile))
          File.Delete(actualFile);

        action(expectedFile, actualFile);

        FileAssert.AreEqual(expectedFile, actualFile);
      }
      finally {
        if (File.Exists(expectedFile))
          File.Delete(expectedFile);
        if (File.Exists(actualFile))
          File.Delete(actualFile);
      }
    }

    [Test]
    public void TestDownloadMessageToFile()
    {
      TestMessage(delegate(PopPseudoServer server, PopClient client, string expectedResult) {
        TestFile(delegate(string expectedFilePath, string actualFilePath) {
          File.WriteAllText(expectedFilePath, expectedResult, new UTF8Encoding(false));

          client.DownloadMessageToFile(3L, actualFilePath);
        });

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 3\r\n");
        Assert.AreEqual(server.DequeueRequest(), "RETR 3\r\n");
      });
    }

    [Test]
    public void TestDownloadFirstMessageToFile()
    {
      TestFirstMessage(delegate(PopPseudoServer server, PopClient client, string expectedResult) {
        TestFile(delegate(string expectedFilePath, string actualFilePath) {
          File.WriteAllText(expectedFilePath, expectedResult, new UTF8Encoding(false));

          client.DownloadFirstMessageToFile(actualFilePath);
        });

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 1\r\n");
        Assert.AreEqual(server.DequeueRequest(), "RETR 1\r\n");
      });
    }

    [Test]
    public void TestDownloadLastMessageToFile()
    {
       TestLastMessage(delegate(PopPseudoServer server, PopClient client, string expectedResult) {
        TestFile(delegate(string expectedFilePath, string actualFilePath) {
          File.WriteAllText(expectedFilePath, expectedResult, new UTF8Encoding(false));

          client.DownloadLastMessageToFile(actualFilePath);
        });

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 5\r\n");
        Assert.AreEqual(server.DequeueRequest(), "RETR 5\r\n");
      });
    }
  }
}