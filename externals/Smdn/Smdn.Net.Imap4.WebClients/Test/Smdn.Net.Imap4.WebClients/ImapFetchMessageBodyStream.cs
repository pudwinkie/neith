using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using NUnit.Framework;

namespace Smdn.Net.Imap4.WebClients {
  [TestFixture]
  public class ImapFetchMessageBodyStreamTests {
    [SetUp]
    public void Setup()
    {
      ImapWebRequestCreator.RegisterPrefix();
    }

    private void Fetch(string requestUri, int fetchBlockSize, string[] responses, Action<ImapPseudoServer, Stream> responseStreamAction)
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("imap://{0}/INBOX{1}", server.HostPort, requestUri)) as ImapWebRequest;

        request.KeepAlive = false;
        request.Timeout = 1000;
        request.Method = "FETCH";
        request.FetchBlockSize = fetchBlockSize;
        request.AllowCreateMailbox = true;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");
        // SELECT
        server.EnqueueResponse("0002 OK done\r\n");

        var tag = 3;

        foreach (var resp in responses) {
          server.EnqueueResponse(resp.Replace("${TAG}", string.Format("{0:x4}", tag++)));
        }

        using (var response = request.GetResponse() as ImapWebResponse) {
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          server.DequeueRequest(); // CAPABILITY
          server.DequeueRequest(); // LOGIN
          server.DequeueRequest(); // SELECT

          using (var stream = response.GetResponseStream()) {
            responseStreamAction(server, stream);

            // CLOSE
            server.EnqueueResponse(string.Format("{0:x4} OK done\r\n", tag++));
            // LOGOUT
            server.EnqueueResponse("* BYE logging out\r\n" + 
                                   string.Format("{0:x4} OK done\r\n", tag++));
          }
        }
      }
    }

    private string ToString(byte[] buffer, int length)
    {
      return ByteString.CreateImmutable(buffer, 0, length).ToString();
    }

    [Test]
    public void TestReadFullFetch()
    {
      var responses = new[] {
        "* FETCH 1 (RFC822.SIZE 40 BODYSTRUCTURE (\"text\" \"plain\" (\"charset\" \"us-ascii\") NIL NIL \"7bit\" 1024 5 NIL NIL NIL NIL) BODY[]<0> {16}\r\n" +
        "0123456789abcdef)\r\n" +
        "${TAG} OK done\r\n",

        "* FETCH 1 (BODY[]<16> {16}\r\n" +
        "0123456789abcdef)\r\n" +
        "${TAG} OK done\r\n",

        "* FETCH 1 (BODY[]<32> {8}\r\n" +
        "01234567)\r\n" +
        "${TAG} OK done\r\n",
      };

      Fetch("/;UID=1", 16, responses, delegate(ImapPseudoServer server, Stream stream) {
        StringAssert.EndsWith("UID FETCH 1 (FLAGS INTERNALDATE RFC822.SIZE ENVELOPE BODY.PEEK[]<0.16>)\r\n",
                              server.DequeueRequest());

        Assert.AreEqual(40L, stream.Length);
        Assert.AreEqual(0L, stream.Position);

        var buffer = new byte[16];

        Assert.AreEqual(6, stream.Read(buffer, 0, 6));
        Assert.AreEqual("012345", ToString(buffer, 6));
        Assert.AreEqual(6L, stream.Position);

        Assert.AreEqual(6, stream.Read(buffer, 0, 6));
        Assert.AreEqual("6789ab", ToString(buffer, 6));
        Assert.AreEqual(12L, stream.Position);

        Assert.AreEqual(6, stream.Read(buffer, 0, 6));
        Assert.AreEqual("cdef01", ToString(buffer, 6));
        Assert.AreEqual(18L, stream.Position);

        StringAssert.EndsWith("UID FETCH 1 (BODY.PEEK[]<16.16>)\r\n",
                              server.DequeueRequest());

        Assert.AreEqual(0, stream.Read(buffer, 0, 0));
        Assert.AreEqual(18L, stream.Position);

        Assert.AreEqual(6, stream.Read(buffer, 0, 6));
        Assert.AreEqual("234567", ToString(buffer, 6));
        Assert.AreEqual(24L, stream.Position);

        Assert.AreEqual(6, stream.Read(buffer, 0, 6));
        Assert.AreEqual("89abcd", ToString(buffer, 6));
        Assert.AreEqual(30L, stream.Position);

        Assert.AreEqual(6, stream.Read(buffer, 0, 6));
        Assert.AreEqual("ef0123", ToString(buffer, 6));
        Assert.AreEqual(36L, stream.Position);

        StringAssert.EndsWith("UID FETCH 1 (BODY.PEEK[]<32.16>)\r\n",
                              server.DequeueRequest());

        Assert.AreEqual(4, stream.Read(buffer, 0, 6));
        Assert.AreEqual("4567", ToString(buffer, 4));
        Assert.AreEqual(40L, stream.Position);

        Assert.AreEqual(0, stream.Read(buffer, 0, 6));
        Assert.AreEqual(40L, stream.Position);
      });
    }

    [Test]
    public void TestReadPartialFetchSectionSpecified()
    {
      var responses = new[] {
        "* FETCH 1 (BODY[]<0> {16}\r\n" +
        "0123456789abcdef)\r\n" +
        "${TAG} OK done\r\n",

        "* FETCH 1 (BODY[]<16> {16}\r\n" +
        "0123456789abcdef)\r\n" +
        "${TAG} OK done\r\n",

        "* FETCH 1 (BODY[]<32> {8}\r\n" +
        "01234567)\r\n" +
        "${TAG} OK done\r\n",
      };

      Fetch("/;UID=1/;SECTION=HEADER", 16, responses, delegate(ImapPseudoServer server, Stream stream) {
        StringAssert.EndsWith("UID FETCH 1 (BODY.PEEK[HEADER]<0.16>)\r\n",
                              server.DequeueRequest());

        Assert.AreEqual(0L, stream.Length);
        Assert.AreEqual(0L, stream.Position);

        var buffer = new byte[16];

        Assert.AreEqual(16, stream.Read(buffer, 0, buffer.Length));
        Assert.AreEqual("0123456789abcdef", ToString(buffer, 16));
        Assert.AreEqual(16L, stream.Position);

        Assert.AreEqual(16, stream.Read(buffer, 0, buffer.Length));
        Assert.AreEqual("0123456789abcdef", ToString(buffer, 16));
        Assert.AreEqual(32L, stream.Position);

        StringAssert.EndsWith("UID FETCH 1 (BODY.PEEK[HEADER]<16.16>)\r\n",
                              server.DequeueRequest());

        Assert.AreEqual(0, stream.Read(buffer, 0, 0));
        Assert.AreEqual(32L, stream.Position);

        StringAssert.EndsWith("UID FETCH 1 (BODY.PEEK[HEADER]<32.16>)\r\n",
                              server.DequeueRequest());

        Assert.AreEqual(8, stream.Read(buffer, 0, buffer.Length));
        Assert.AreEqual("01234567", ToString(buffer, 8));
        Assert.AreEqual(40L, stream.Position);

        Assert.AreEqual(0, stream.Read(buffer, 0, 6));
        Assert.AreEqual(40L, stream.Position);
      });
    }

    [Test]
    public void TestReadPartialFetchStartSpecified()
    {
      var responses = new[] {
        "* FETCH 1 (BODY[]<128> {16}\r\n" +
        "0123456789abcdef)\r\n" +
        "${TAG} OK done\r\n",

        "* FETCH 1 (BODY[]<144> {16}\r\n" +
        "0123456789abcdef)\r\n" +
        "${TAG} OK done\r\n",

        "* FETCH 1 (BODY[]<160> {8}\r\n" +
        "01234567)\r\n" +
        "${TAG} OK done\r\n",
      };

      Fetch("/;UID=1/;PARTIAL=128", 16, responses, delegate(ImapPseudoServer server, Stream stream) {
        StringAssert.EndsWith("UID FETCH 1 (BODY.PEEK[]<128.16>)\r\n",
                              server.DequeueRequest());

        Assert.AreEqual(0L, stream.Length);
        Assert.AreEqual(0L, stream.Position);

        var buffer = new byte[20];

        Assert.AreEqual(20, stream.Read(buffer, 0, buffer.Length));
        Assert.AreEqual("0123456789abcdef0123", ToString(buffer, 20));
        Assert.AreEqual(20L, stream.Position);

        StringAssert.EndsWith("UID FETCH 1 (BODY.PEEK[]<144.16>)\r\n",
                              server.DequeueRequest());

        Assert.AreEqual(0, stream.Read(buffer, 0, 0));
        Assert.AreEqual(20L, stream.Position);

        Assert.AreEqual(20, stream.Read(buffer, 0, buffer.Length));
        Assert.AreEqual("456789abcdef01234567", ToString(buffer, 20));
        Assert.AreEqual(40L, stream.Position);

        StringAssert.EndsWith("UID FETCH 1 (BODY.PEEK[]<160.16>)\r\n",
                              server.DequeueRequest());

        Assert.AreEqual(0, stream.Read(buffer, 0, 0));
        Assert.AreEqual(40L, stream.Position);
      });
    }

    [Test]
    public void TestReadPartialFetchRangeSpecified()
    {
      var responses = new[] {
        "* FETCH 1 (BODY[]<128> {16}\r\n" +
        "0123456789abcdef)\r\n" +
        "${TAG} OK done\r\n",

        "* FETCH 1 (BODY[]<144> {16}\r\n" +
        "0123456789abcdef)\r\n" +
        "${TAG} OK done\r\n",

        "* FETCH 1 (BODY[]<160> {8}\r\n" +
        "01234567)\r\n" +
        "${TAG} OK done\r\n",
      };

      Fetch("/;UID=1/;PARTIAL=128.40", 16, responses, delegate(ImapPseudoServer server, Stream stream) {
        StringAssert.EndsWith("UID FETCH 1 (BODY.PEEK[]<128.16>)\r\n",
                              server.DequeueRequest());

        Assert.AreEqual(0L, stream.Length);
        Assert.AreEqual(0L, stream.Position);

        var buffer = new byte[20];

        Assert.AreEqual(20, stream.Read(buffer, 0, buffer.Length));
        Assert.AreEqual("0123456789abcdef0123", ToString(buffer, 20));
        Assert.AreEqual(20L, stream.Position);

        StringAssert.EndsWith("UID FETCH 1 (BODY.PEEK[]<144.16>)\r\n",
                              server.DequeueRequest());

        Assert.AreEqual(0, stream.Read(buffer, 0, 0));
        Assert.AreEqual(20L, stream.Position);

        Assert.AreEqual(20, stream.Read(buffer, 0, buffer.Length));
        Assert.AreEqual("456789abcdef01234567", ToString(buffer, 20));
        Assert.AreEqual(40L, stream.Position);

        StringAssert.EndsWith("UID FETCH 1 (BODY.PEEK[]<160.8>)\r\n",
                              server.DequeueRequest());

        Assert.AreEqual(0, stream.Read(buffer, 0, 0));
        Assert.AreEqual(40L, stream.Position);
      });
    }

    [Test]
    public void TestReadLessThanFetchBlockSize()
    {
      var responses = new[] {
        "* FETCH 1 (BODY[]<0> {16}\r\n" +
        "0123456789abcdef)\r\n" +
        "${TAG} OK done\r\n",
      };

      Fetch("/;UID=1/;SECTION=HEADER", 10240, responses, delegate(ImapPseudoServer server, Stream stream) {
        StringAssert.EndsWith("UID FETCH 1 (BODY.PEEK[HEADER]<0.10240>)\r\n",
                              server.DequeueRequest());

        Assert.AreEqual(0L, stream.Length);
        Assert.AreEqual(0L, stream.Position);

        var buffer = new byte[32];

        Assert.AreEqual(16, stream.Read(buffer, 0, buffer.Length));
        Assert.AreEqual("0123456789abcdef", ToString(buffer, 16));
        Assert.AreEqual(16L, stream.Position);

        Assert.AreEqual(0, stream.Read(buffer, 0, 0));
        Assert.AreEqual(16L, stream.Position);
      });
    }

    [Test]
    public void TestReadViaStreamReader()
    {
      var message = @"Date: Wed, 17 Jul 1996 02:23:25 -0700 (PDT)
From: Terry Gray <gray@cac.washington.edu>
Subject: IMAP4rev1 WG mtg summary and minutes
To: imap@cac.washington.edu
cc: minutes@CNRI.Reston.VA.US, John Klensin <KLENSIN@MIT.EDU>
Message-Id: <B27397-0100000@cac.washington.edu>
MIME-Version: 1.0
Content-Type: TEXT/PLAIN; CHARSET=US-ASCII
".Replace("\r\n", "\n").Replace("\n", "\r\n");

      var blockSize = 48;
      var start = 0;
      var responses = new List<string>();

      for (;;) {
        var substr = message.Substring(start, Math.Min(message.Length - start, blockSize));

        responses.Add(string.Format("* FETCH 1 (BODY[]<{0}> {{{1}}}\r\n", start, substr.Length) +
                      string.Format("{0})\r\n", substr) +
                      "${TAG} OK done\r\n");

        start += substr.Length;

        if (start == message.Length)
          break;
      }

      Fetch("/;UID=1/;SECTION=HEADER", blockSize, responses.ToArray(), delegate(ImapPseudoServer server, Stream stream) {
        var reader = new StreamReader(stream, Encoding.ASCII);

        Assert.AreEqual(message, reader.ReadToEnd());
      });
    }
  }
}