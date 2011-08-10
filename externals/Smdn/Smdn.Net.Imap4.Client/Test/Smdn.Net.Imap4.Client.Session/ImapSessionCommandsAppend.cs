using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Smdn.Net.Imap4.Client.Session {
  [TestFixture]
  public class ImapSessionCommandsAppendTests : ImapSessionTestsBase {
    [Test]
    public void TestAppend()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        // APPEND transaction
        server.EnqueueResponse("+ OK continue\r\n");
        server.EnqueueResponse("0004 OK APPEND completed\r\n");

        var message = new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\ntest message"),
                                            new ImapMessageFlagSet(ImapMessageFlag.Seen));

        Assert.IsTrue((bool)session.Append(message, "INBOX"));

        Assert.AreEqual("0004 APPEND INBOX (\\Seen) {33}\r\n" +
                        "MIME-Version: 1.0\r\n" +
                        "\r\n" +
                        "test message\r\n",
                        server.DequeueAll());

        return 1;
      });
    }

    [Test]
    public void TestAppendNonSynchronizedLiterals()
    {
      SelectMailbox(new[] {"LITERAL+"}, delegate(ImapSession session, ImapPseudoServer server) {
        // APPEND transaction
        server.EnqueueResponse("0004 OK APPEND completed\r\n");

        var message = new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\ntest message"),
                                            new ImapMessageFlagSet(ImapMessageFlag.Seen));

        Assert.IsTrue((bool)session.Append(message, "INBOX"));

        Assert.AreEqual("0004 APPEND INBOX (\\Seen) {33+}\r\n" +
                        "MIME-Version: 1.0\r\n" +
                        "\r\n" +
                        "test message\r\n",
                        server.DequeueAll());

        return 1;
      });
    }

    [Test]
    public void TestAppendTryCreate()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server, Uri expectedAuthority) {
        // APPEND transaction
        server.EnqueueResponse("0004 NO [TRYCREATE] Mailbox doesn't exist: INBOX.appendto\r\n");
        server.EnqueueResponse("0005 OK CREATE completed\r\n");
        server.EnqueueResponse("+ OK continue\r\n");
        server.EnqueueResponse("0006 OK APPEND completed\r\n");

        var message = new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\ntest message"),
                                            new ImapMessageFlagSet(new[] {"$label2"}, ImapMessageFlag.Draft));

        ImapMailbox created;

        Assert.IsTrue((bool)session.Append(message, "INBOX.appendto", out created));

        Assert.AreEqual("INBOX.appendto", created.Name);
        Assert.AreEqual(new Uri(expectedAuthority, "./INBOX.appendto"), created.Url);
        Assert.IsNotNull(created.Flags);
        Assert.IsNotNull(created.ApplicableFlags);
        Assert.IsNotNull(created.PermanentFlags);

        Assert.AreEqual("0004 APPEND INBOX.appendto (\\Draft $label2) {33}\r\n",
                        server.DequeueRequest());
        Assert.AreEqual("0005 CREATE INBOX.appendto\r\n",
                        server.DequeueRequest());

        Assert.AreEqual("0006 APPEND INBOX.appendto (\\Draft $label2) {33}\r\n" +
                        "MIME-Version: 1.0\r\n" +
                        "\r\n" +
                        "test message\r\n",
                        server.DequeueAll());

        return 3;
      });
    }

    [Test]
    public void TestAppendTryCreateNoReferralResponseCode()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        // APPEND transaction
        server.EnqueueResponse("0004 NO [REFERRAL IMAP://user;AUTH=*@SERVER2/SHARED/FOO] Remote mailbox. Try SERVER2.\r\n");

        var message = new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\ntest message"),
                                            new ImapMessageFlagSet(new[] {"$label2"}, ImapMessageFlag.Draft));

        ImapMailbox created;

        Assert.IsFalse((bool)session.Append(message, "SHARED/FOO", out created));

        Assert.IsNull(created);

        Assert.AreEqual("0004 APPEND SHARED/FOO (\\Draft $label2) {33}\r\n",
                        server.DequeueRequest());

        return 1;
      });
    }

    [Test]
    public void TestAppendDontTryCreate()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        // APPEND transaction
        server.EnqueueResponse("0004 NO [TRYCREATE] Mailbox doesn't exist: INBOX.appendto\r\n");

        var message = new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\ntest message"),
                                            new DateTimeOffset(2008, 2, 25, 15, 1, 12, new TimeSpan(+9, 0, 0)));

        var internalDate = "\"25-Feb-2008 15:01:12 +0900\"";

        Assert.IsFalse((bool)session.Append(message, "INBOX.appendto"));

        Assert.AreEqual("0004 APPEND INBOX.appendto " + internalDate + " {33}\r\n",
                        server.DequeueRequest());

        return 1;
      });
    }

    [Test]
    public void TestAppendRemoveNonApplicableFlags()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        // APPEND transaction
        server.EnqueueResponse("+ OK continue\r\n");
        server.EnqueueResponse("0004 OK APPEND completed\r\n");

        var message = new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\ntest message"),
                                            new ImapMessageFlagSet(ImapMessageFlag.Seen,
                                                                    ImapMessageFlag.Recent,
                                                                    ImapMessageFlag.AllowedCreateKeywords));

        Assert.IsTrue((bool)session.Append(message, "INBOX"));

        Assert.AreEqual("0004 APPEND INBOX (\\Seen) {33}\r\n" +
                        "MIME-Version: 1.0\r\n" +
                        "\r\n" +
                        "test message\r\n",
                        server.DequeueAll());

        return 1;
      });
    }

    [Test]
    public void TestAppendWithAppendUidResponseCode()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        // APPEND transaction
        server.EnqueueResponse("+ OK continue\r\n");
        server.EnqueueResponse("0004 OK [APPENDUID 38505 3955] APPEND completed\r\n");

        var message = new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\ntest message"),
                                            new ImapMessageFlagSet(ImapMessageFlag.Seen));

        ImapAppendedUidSet appended;

        Assert.IsTrue((bool)session.Append(message, "INBOX", out appended));

        Assert.AreEqual("0004 APPEND INBOX (\\Seen) {33}\r\n" +
                        "MIME-Version: 1.0\r\n" +
                        "\r\n" +
                        "test message\r\n",
                        server.DequeueAll());

        Assert.IsNotNull(appended);
        Assert.AreEqual(38505L, appended.UidValidity);
        Assert.AreEqual(3955L, appended.ToNumber());
        Assert.AreEqual("3955", appended.ToString());

        return 1;
      });
    }

    [Test]
    public void TestPrepareAppend()
    {
      PrepareAppend(false, false);
    }

    [Test]
    public void TestPrepareAppendLengthSpecified()
    {
      PrepareAppend(false, true);
    }

    [Test]
    public void TestPrepareAppendNonSynchronizedLiterals()
    {
      PrepareAppend(true, false);
    }

    [Test]
    public void TestPrepareAppendLengthSpecifiedNonSynchronizedLiterals()
    {
      PrepareAppend(true, true);
    }

    private void PrepareAppend(bool nonSynchronizedLiterals, bool specifyLength)
    {
      var messageBody = @"Date: Mon, 7 Feb 1994 21:52:25 -0800 (PST)
From: Fred Foobar <foobar@Blurdybloop.example.COM>
Subject: afternoon meeting
To: mooch@owatagu.example.net
Message-Id: <B27397-0100000@Blurdybloop.example.COM>
MIME-Version: 1.0
Content-Type: TEXT/PLAIN; CHARSET=US-ASCII

Hello Joe, do you think we can meet at 3:30 tomorrow?
".Replace("\r\n", "\n").Replace("\n", "\r\n");

      var caps = nonSynchronizedLiterals
        ? new[] {"IMAP4REV1", "LITERAL+"}
        : new[] {"IMAP4REV1"};

      Authenticate(caps, delegate(ImapSession session, ImapPseudoServer server) {
        var messageBodyBytes = NetworkTransferEncoding.Transfer7Bit.GetBytes(messageBody);
        var length = specifyLength ? (long?)messageBodyBytes.Length : null;
        var appendContext = session.PrepareAppend(length, null,  null, "INBOX");

        Assert.IsNotNull(appendContext);
        Assert.IsNotNull(appendContext.WriteStream);
        Assert.IsTrue(session.IsTransactionProceeding);

        server.EnqueueResponse("+ OK continue\r\n");
        server.EnqueueResponse(string.Empty);
        server.EnqueueResponse("0002 OK [APPENDUID 38505 3955] APPEND completed\r\n");

        appendContext.WriteStream.Write(messageBodyBytes,
                                        0,
                                        messageBodyBytes.Length);

        System.Threading.Thread.Sleep(250);

        if (specifyLength)
          Assert.IsFalse(session.IsTransactionProceeding);
        else
          Assert.IsTrue(session.IsTransactionProceeding);

        ImapAppendedUidSet appendedUid;

        Assert.IsTrue((bool)appendContext.GetResult(out appendedUid));
        Assert.IsFalse(session.IsTransactionProceeding);

        var expected =
          string.Format("0002 APPEND INBOX {{{0}}}\r\n", messageBodyBytes.Length) +
          messageBody +
          "\r\n";

        Assert.AreEqual(expected, server.DequeueAll());

        Assert.IsNotNull(appendedUid);
        Assert.AreEqual(38505L, appendedUid.UidValidity);
        Assert.AreEqual(3955L, appendedUid.ToNumber());
      });
    }

    [Test]
    public void TestPrepareAppendAppendContextWriteStream()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        var appendContext = session.PrepareAppend(null, null,  null, "INBOX");

        Assert.IsNotNull(appendContext);
        Assert.IsNotNull(appendContext.WriteStream);

        var writeStream = appendContext.WriteStream;

        try {
          writeStream.Position = 0L;
          Assert.Fail("NotSupportedException not thrown (set_Position)");
        }
        catch (NotSupportedException) {
        }

        try {
          Assert.Fail("NotSupportedException not thrown (get_Position; {0})",
                      writeStream.Position);
        }
        catch (NotSupportedException) {
        }

        try {
          Assert.Fail("NotSupportedException not thrown (get_Length; {0})",
                      writeStream.Length);
        }
        catch (NotSupportedException) {
        }

        try {
          writeStream.Seek(0L, SeekOrigin.Begin);
          Assert.Fail("NotSupportedException not thrown (Seek)");
        }
        catch (NotSupportedException) {
        }

        try {
          var buffer = new byte[8];

          writeStream.Read(buffer, 0, buffer.Length);

          Assert.Fail("NotSupportedException not thrown (Read)");
        }
        catch (NotSupportedException) {
        }

        Assert.IsTrue(session.IsTransactionProceeding);

        server.EnqueueResponse("+ OK continue\r\n");
        server.EnqueueResponse(string.Empty);
        server.EnqueueResponse("0002 OK APPEND completed\r\n");

        writeStream.Close();

        Assert.IsNull(appendContext.WriteStream);

        System.Threading.Thread.Sleep(250);

        Assert.IsTrue(session.IsTransactionProceeding);

        try {
          writeStream.WriteByte(0);
          Assert.Fail("ObjectDisposedException not thrown");
        }
        catch (ObjectDisposedException) {
        }

        writeStream.Close(); // call Close() again

        ImapAppendedUidSet appendedUid;

        Assert.IsTrue((bool)appendContext.GetResult(out appendedUid));
        Assert.IsFalse(session.IsTransactionProceeding);

        Assert.AreEqual("0002 APPEND INBOX {0}\r\n\r\n", server.DequeueAll());

        Assert.IsNull(appendedUid);
      });
    }

    [Test]
    public void TestPrepareAppendAppendContextWriteStreamBufferUnderrun()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        session.SendTimeout = 500;
        session.ReceiveTimeout = 500;

        var appendContext = session.PrepareAppend(null, null, null, "INBOX");

        Assert.IsNotNull(appendContext);
        Assert.IsNotNull(appendContext.WriteStream);
        Assert.IsTrue(session.IsTransactionProceeding);

        server.EnqueueResponse("+ OK continue\r\n");

        System.Threading.Thread.Sleep(250);

        Assert.IsTrue(session.IsTransactionProceeding);

        ImapAppendedUidSet appendedUid;

        try {
          appendContext.GetResult(out appendedUid);
          Assert.Fail("TimeoutException not thrown");
        }
        catch (TimeoutException) {
        }

        Assert.IsFalse(session.IsTransactionProceeding);
      });
    }

    [Test]
    public void TestPrepareAppendAppendContextWriteStreamWriteOverSpecifiedLength()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        var messageBody = "message";
        var messageBodyBytes = NetworkTransferEncoding.Transfer7Bit.GetBytes(messageBody);

        var appendContext = session.PrepareAppend(messageBodyBytes.Length, null, null, "INBOX");

        Assert.IsNotNull(appendContext);
        Assert.IsNotNull(appendContext.WriteStream);
        Assert.IsTrue(session.IsTransactionProceeding);

        server.EnqueueResponse("+ OK continue\r\n");

        appendContext.WriteStream.Write(messageBodyBytes,
                                        0,
                                        messageBodyBytes.Length);

        // extra data
        appendContext.WriteStream.WriteByte(0x40);
        appendContext.WriteStream.WriteByte(0x40);
        appendContext.WriteStream.WriteByte(0x40);
        appendContext.WriteStream.WriteByte(0x40);

        server.EnqueueResponse(string.Empty);
        server.EnqueueResponse("0002 OK APPEND completed\r\n");

        Assert.IsTrue(session.IsTransactionProceeding);

        ImapAppendedUidSet appendedUid;

        Assert.IsTrue((bool)appendContext.GetResult(out appendedUid));
        Assert.IsFalse(session.IsTransactionProceeding);

        var expected =
          string.Format("0002 APPEND INBOX {{{0}}}\r\n", messageBodyBytes.Length) +
          messageBody +
          "\r\n";

        Assert.AreEqual(expected, server.DequeueAll());
      });
    }

    [Test]
    public void TestPrepareAppendAppendContextWriteStreamWriteShortageSpecifiedLength()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        const int shortage = 8;

        var messageBody = "message";
        var messageBodyBytes = NetworkTransferEncoding.Transfer7Bit.GetBytes(messageBody);

        session.SendTimeout = 500;
        session.ReceiveTimeout = 500;

        var appendContext = session.PrepareAppend(messageBodyBytes.Length + shortage, null, null, "INBOX");

        Assert.IsNotNull(appendContext);
        Assert.IsNotNull(appendContext.WriteStream);
        Assert.IsTrue(session.IsTransactionProceeding);

        server.EnqueueResponse("+ OK continue\r\n");

        appendContext.WriteStream.Write(messageBodyBytes,
                                        0,
                                        messageBodyBytes.Length);

        ImapAppendedUidSet appendedUid;

        try {
          appendContext.GetResult(out appendedUid);
          Assert.Fail("TimeoutException not thrown");
        }
        catch (TimeoutException) {
        }
      });
    }

    [Test]
    public void TestPrepareAppendNestedCall()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        var appendContext = session.PrepareAppend(null, null, null, "INBOX");

        Assert.IsNotNull(appendContext);
        Assert.IsNotNull(appendContext.WriteStream);
        Assert.IsTrue(session.IsTransactionProceeding);

        try {
          session.PrepareAppend(null, null, null, "INBOX");
          Assert.Fail("InvalidOperationException not thrown");
        }
        catch (InvalidOperationException) {
        }

        server.EnqueueResponse("+ OK continue\r\n");
        server.EnqueueResponse("0002 OK APPEND completed\r\n");

        var messageBodyBytes = NetworkTransferEncoding.Transfer7Bit.GetBytes("message");

        appendContext.WriteStream.Write(messageBodyBytes,
                                        0,
                                        messageBodyBytes.Length);

        System.Threading.Thread.Sleep(250);

        Assert.IsTrue(session.IsTransactionProceeding);

        ImapAppendedUidSet appendedUid;

        Assert.IsTrue((bool)appendContext.GetResult(out appendedUid));
        Assert.IsFalse(session.IsTransactionProceeding);

        Assert.AreEqual("0002 APPEND INBOX {7}\r\nmessage\r\n",
                                   server.DequeueAll());

        Assert.IsNull(appendedUid);
      });
    }

    [Test]
    public void TestPrepareAppendGetResultAlreadyFinished()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        var appendContext = session.PrepareAppend(null, null, null, "INBOX");

        Assert.IsNotNull(appendContext);
        Assert.IsNotNull(appendContext.WriteStream);
        Assert.IsTrue(session.IsTransactionProceeding);

        server.EnqueueResponse("+ OK continue\r\n");
        server.EnqueueResponse("0002 OK APPEND completed\r\n");

        appendContext.WriteStream.WriteByte(0x40);
        appendContext.WriteStream.Close();

        System.Threading.Thread.Sleep(250);

        Assert.IsFalse(session.IsTransactionProceeding);

        ImapAppendedUidSet appendedUid;

        Assert.IsTrue((bool)appendContext.GetResult(out appendedUid));
        Assert.IsFalse(session.IsTransactionProceeding);

        Assert.AreEqual("0002 APPEND INBOX {1}\r\n@\r\n",
                                   server.DequeueAll());

        Assert.IsNull(appendedUid);

        try {
          appendContext.GetResult(out appendedUid);
          Assert.Fail("InvalidOperationException not thrown");
        }
        catch (InvalidOperationException) {
        }
      });
    }

    [Test]
    public void TestPrepareAppendContinuationNo1()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        var appendContext = session.PrepareAppend(null, null, null, "INBOX");

        Assert.IsNotNull(appendContext);
        Assert.IsNotNull(appendContext.WriteStream);
        Assert.IsTrue(session.IsTransactionProceeding);

        server.EnqueueResponse("0002 NO APPEND failed\r\n");

        System.Threading.Thread.Sleep(250);

        Assert.IsTrue(session.IsTransactionProceeding);

        var messageBodyBytes = NetworkTransferEncoding.Transfer7Bit.GetBytes("message");

        appendContext.WriteStream.Write(messageBodyBytes,
                                        0,
                                        messageBodyBytes.Length);

        System.Threading.Thread.Sleep(250);

        Assert.IsTrue(session.IsTransactionProceeding);

        ImapAppendedUidSet appendedUid;

        Assert.IsFalse((bool)appendContext.GetResult(out appendedUid));
        Assert.IsFalse(session.IsTransactionProceeding);

        Assert.AreEqual("0002 APPEND INBOX {7}\r\n",
                        server.DequeueAll());
      });
    }

    [Test]
    public void TestPrepareAppendContinuationNo2()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        var appendContext = session.PrepareAppend(null, null, null, "INBOX");

        Assert.IsNotNull(appendContext);
        Assert.IsNotNull(appendContext.WriteStream);
        Assert.IsTrue(session.IsTransactionProceeding);

        server.EnqueueResponse("0002 NO APPEND failed\r\n");

        System.Threading.Thread.Sleep(250);

        Assert.IsTrue(session.IsTransactionProceeding);

        appendContext.WriteStream.Close();

        System.Threading.Thread.Sleep(250);

        Assert.IsFalse(session.IsTransactionProceeding);

        ImapAppendedUidSet appendedUid;

        Assert.IsFalse((bool)appendContext.GetResult(out appendedUid));
        Assert.IsFalse(session.IsTransactionProceeding);

        Assert.AreEqual("0002 APPEND INBOX {0}\r\n",
                        server.DequeueAll());
      });
    }

    [Test]
    public void TestAppendMultiple()
    {
      AppendMultiple(false);
    }

    [Test]
    public void TestAppendMultipleWithAppendUidResponseCode()
    {
      AppendMultiple(true);
    }

    private void AppendMultiple(bool respondAppendUid)
    {
      SelectMailbox(new[] {"MULTIAPPEND"}, delegate(ImapSession session, ImapPseudoServer server) {
        // APPEND transaction
        server.EnqueueResponse("+ Ready for literal data\r\n");
        server.EnqueueResponse(string.Empty);
        server.EnqueueResponse("+ Ready for literal data\r\n");
        server.EnqueueResponse(string.Empty);

        if (respondAppendUid)
          server.EnqueueResponse("0004 OK [APPENDUID 38505 3955:3956] APPEND completed\r\n");
        else
          server.EnqueueResponse("0004 OK APPEND completed\r\n");

        var messageBody1 = @"Date: Mon, 7 Feb 1994 21:52:25 -0800 (PST)
From: Fred Foobar <foobar@Blurdybloop.example.COM>
Subject: afternoon meeting
To: mooch@owatagu.example.net
Message-Id: <B27397-0100000@Blurdybloop.example.COM>
MIME-Version: 1.0
Content-Type: TEXT/PLAIN; CHARSET=US-ASCII

Hello Joe, do you think we can meet at 3:30 tomorrow?
".Replace("\r\n", "\n").Replace("\n", "\r\n");
        var messageBody2 = @"Date: Mon, 7 Feb 1994 22:43:04 -0800 (PST)
From: Joe Mooch <mooch@OWaTaGu.example.net>
Subject: Re: afternoon meeting
To: foobar@blurdybloop.example.com
Message-Id: <a0434793874930@OWaTaGu.example.net>
MIME-Version: 1.0
Content-Type: TEXT/PLAIN; CHARSET=US-ASCII

3:30 is fine with me.
".Replace("\r\n", "\n").Replace("\n", "\r\n");

        var message1 = new ImapAppendMessage(Encoding.ASCII.GetBytes(messageBody1),
                                             new ImapMessageFlagSet(ImapMessageFlag.Seen));
        var message2 = new ImapAppendMessage(Encoding.ASCII.GetBytes(messageBody2),
                                             new DateTimeOffset(new DateTime(1994, 2, 7, 22, 43, 04), new TimeSpan(-8, 0, 0)),
                                             new ImapMessageFlagSet(ImapMessageFlag.Seen));

        ImapAppendedUidSet appended;

        Assert.IsTrue((bool)session.AppendMultiple(new[] {message1, message2}, "saved-messages", out appended));

        var expected =
          string.Format("0004 APPEND saved-messages (\\Seen) {{{0}}}\r\n", Encoding.ASCII.GetByteCount(messageBody1)) +
          messageBody1 +
          string.Format(" (\\Seen) \"07-Feb-1994 22:43:04 -0800\" {{{0}}}\r\n", Encoding.ASCII.GetByteCount(messageBody2)) +
          messageBody2 +
          "\r\n";

        Assert.AreEqual(expected, server.DequeueAll());

        if (respondAppendUid) {
          Assert.IsNotNull(appended);
          Assert.AreEqual(38505L, appended.UidValidity);

          var appendedUids = appended.ToArray();

          Assert.AreEqual(2, appendedUids.Length);
          Assert.AreEqual(3955L, appendedUids[0]);
          Assert.AreEqual(3956L, appendedUids[1]);
        }

        return 1;
      });
    }

    [Test]
    public void TestAppendMultipleWithNonSyncedLiteral()
    {
      SelectMailbox(new[] {"MULTIAPPEND", "LITERAL+"}, delegate(ImapSession session, ImapPseudoServer server) {
        // APPEND transaction
        server.EnqueueResponse("0004 OK APPEND completed\r\n");

        var message1 = new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\ntest message1"));
        var message2 = new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\ntest message2"));
        Assert.IsTrue((bool)session.AppendMultiple(new[] {message1, message2}, "saved-messages"));

        Assert.AreEqual("0004 APPEND saved-messages {34+}\r\n" +
                        "MIME-Version: 1.0\r\n" +
                        "\r\n" +
                        "test message1 {34+}\r\n" +
                        "MIME-Version: 1.0\r\n" +
                        "\r\n" +
                        "test message2\r\n",
                        server.DequeueAll());

        return 1;
      });
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestAppendMultipleIncapable()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        session.HandlesIncapableAsException = true;
        // APPEND transaction
        session.AppendMultiple(new[] {
          new ImapAppendMessage(Encoding.ASCII.GetBytes("message")),
          new ImapAppendMessage(Encoding.ASCII.GetBytes("message2")),
        }, "saved-messages");

        return -1;
      });
    }

    [Test]
    public void TestAppendBinary()
    {
      SelectMailbox(new[] {"BINARY"}, delegate(ImapSession session, ImapPseudoServer server) {
        // APPEND transaction
        server.EnqueueResponse("+ OK continue\r\n");
        server.EnqueueResponse("0004 OK APPEND completed\r\n");

        Assert.IsTrue((bool)session.AppendBinary(new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\n\x00\x01\x02\x03\x04\x05\x06\x07")),
                                                 "INBOX"));

        Assert.AreEqual("0004 APPEND INBOX ~{29}\r\n" +
                        "MIME-Version: 1.0\r\n" +
                        "\r\n" +
                        "\x00\x01\x02\x03\x04\x05\x06\x07\r\n",
                        server.DequeueAll());

        return 1;
      });
    }

    [Test]
    public void TestAppendBinaryWithNonSynchronizedLiteral()
    {
      SelectMailbox(new[] {"BINARY", "LITERAL+"}, delegate(ImapSession session, ImapPseudoServer server) {
        // APPEND transaction
        server.EnqueueResponse("0004 OK APPEND completed\r\n");

        Assert.IsTrue((bool)session.AppendBinary(new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\n\x00\x01\x02\x03\x04\x05\x06\x07")),
                                                 "INBOX"));

        Assert.AreEqual(("0004 APPEND INBOX ~{29+}\r\n" +
                        "MIME-Version: 1.0\r\n" +
                        "\r\n" +
                        "\x00\x01\x02\x03\x04\x05\x06\x07\r\n").Replace("\x00", @"\0"),
                        server.DequeueAll().Replace("\x00", @"\0"));

        return 1;
      });
    }

    [Test]
    public void TestAppendBinaryMultiple()
    {
      SelectMailbox(new[] {"BINARY", "MULTIAPPEND"}, delegate(ImapSession session, ImapPseudoServer server) {
        // APPEND transaction
        server.EnqueueResponse("+ OK continue\r\n");
        server.EnqueueResponse("+ OK continue\r\n");
        server.EnqueueResponse("0004 OK APPEND completed\r\n");

        var messages = new[] {
          new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\nmessage1\x00\x01\x02\x03\x04\x05\x06\x07")),
          new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\nmessage2\x00\x01\x02\x03\x04\x05\x06\x07")),
        };

        Assert.IsTrue((bool)session.AppendBinaryMultiple(messages,
                                                         "INBOX"));

        Assert.AreEqual(("0004 APPEND INBOX ~{37}\r\n" +
                        "MIME-Version: 1.0\r\n" +
                        "\r\n" +
                        "message1\x00\x01\x02\x03\x04\x05\x06\x07" +
                        " ~{37}\r\n" +
                        "MIME-Version: 1.0\r\n" +
                        "\r\n" +
                        "message2\x00\x01\x02\x03\x04\x05\x06\x07\r\n").Replace("\x00", @"\0"),
                        server.DequeueAll().Replace("\x00", @"\0"));

        return 1;
      });
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestAppendBinaryIncapable()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        session.HandlesIncapableAsException = true;
        // APPEND transaction
        session.AppendBinary(new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\n\x00\x01\x02\x03\x04\x05\x06\x07")),
                             "saved-messages");

        return -1;
      });
    }
  }
}