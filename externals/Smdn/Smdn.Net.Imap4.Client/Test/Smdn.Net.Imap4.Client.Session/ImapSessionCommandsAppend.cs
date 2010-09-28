using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Smdn.Net.Imap4.Client.Session {
  [TestFixture]
  public class ImapSessionCommandsAppend : ImapSessionTestsBase {
    [Test]
    public void TestAppend()
    {
      using (var session = SelectMailbox()) {
        // APPEND transaction
        server.EnqueueResponse("+ OK continue\r\n");
        server.EnqueueResponse("0004 OK APPEND completed\r\n");
  
        var message = new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\ntest message"),
                                            new ImapMessageFlagList(ImapMessageFlag.Seen));
  
        Assert.IsTrue((bool)session.Append(message, "INBOX"));

        Assert.AreEqual("0004 APPEND \"INBOX\" (\\Seen) {33}\r\n" +
                        "MIME-Version: 1.0\r\n" +
                        "\r\n" +
                        "test message\r\n",
                        server.DequeueAll());
  
        CloseMailbox(session);
      }
    }

    [Test]
    public void TestAppendNonSynchronizedLiterals()
    {
      using (var session = SelectMailbox("LITERAL+")) {
        // APPEND transaction
        server.EnqueueResponse("0004 OK APPEND completed\r\n");
  
        var message = new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\ntest message"),
                                            new ImapMessageFlagList(ImapMessageFlag.Seen));
  
        Assert.IsTrue((bool)session.Append(message, "INBOX"));
  
        Assert.AreEqual("0004 APPEND \"INBOX\" (\\Seen) {33+}\r\n" +
                        "MIME-Version: 1.0\r\n" +
                        "\r\n" +
                        "test message\r\n",
                        server.DequeueAll());
  
        CloseMailbox(session);
      }
    }

    [Test]
    public void TestAppendTryCreate()
    {
      using (var session = SelectMailbox()) {
        // APPEND transaction
        server.EnqueueResponse("0004 NO [TRYCREATE] Mailbox doesn't exist: INBOX.appendto\r\n");
        server.EnqueueResponse("0005 OK CREATE completed\r\n");
        server.EnqueueResponse("+ OK continue\r\n");
        server.EnqueueResponse("0006 OK APPEND completed\r\n");
  
        var message = new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\ntest message"),
                                            new ImapMessageFlagList(new[] {"$label2"}, ImapMessageFlag.Draft));
  
        ImapMailbox created;
  
        Assert.IsTrue((bool)session.Append(message, "INBOX.appendto", out created));
  
        Assert.AreEqual("INBOX.appendto", created.Name);
        Assert.AreEqual(new Uri(uri, "./INBOX.appendto"), created.Url);
        Assert.IsNotNull(created.Flags);
        Assert.IsNotNull(created.ApplicableFlags);
        Assert.IsNotNull(created.PermanentFlags);

        Assert.AreEqual("0004 APPEND \"INBOX.appendto\" (\\Draft $label2) {33}\r\n",
                        server.DequeueRequest());
        Assert.AreEqual("0005 CREATE \"INBOX.appendto\"\r\n",
                        server.DequeueRequest());

        Assert.AreEqual("0006 APPEND \"INBOX.appendto\" (\\Draft $label2) {33}\r\n" +
                        "MIME-Version: 1.0\r\n" +
                        "\r\n" +
                        "test message\r\n",
                        server.DequeueAll());
  
        CloseMailbox(session, "0007");
      }
    }

    [Test]
    public void TestAppendTryCreateNoReferralResponseCode()
    {
      using (var session = SelectMailbox()) {
        // APPEND transaction
        server.EnqueueResponse("0004 NO [REFERRAL IMAP://user;AUTH=*@SERVER2/SHARED/FOO] Remote mailbox. Try SERVER2.\r\n");

        var message = new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\ntest message"),
                                            new ImapMessageFlagList(new[] {"$label2"}, ImapMessageFlag.Draft));

        ImapMailbox created;

        Assert.IsFalse((bool)session.Append(message, "SHARED/FOO", out created));

        Assert.IsNull(created);

        Assert.AreEqual("0004 APPEND \"SHARED/FOO\" (\\Draft $label2) {33}\r\n",
                        server.DequeueRequest());
      }
    }

    [Test]
    public void TestAppendDontTryCreate()
    {
      using (var session = SelectMailbox()) {
        // APPEND transaction
        server.EnqueueResponse("0004 NO [TRYCREATE] Mailbox doesn't exist: INBOX.appendto\r\n");
  
        var message = new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\ntest message"),
                                            new DateTimeOffset(2008, 2, 25, 15, 1, 12, new TimeSpan(+9, 0, 0)));
  
        var internalDate = "\"25-Feb-2008 15:01:12 +0900\"";
  
        Assert.IsFalse((bool)session.Append(message, "INBOX.appendto"));
  
        Assert.AreEqual("0004 APPEND \"INBOX.appendto\" " + internalDate + " {33}\r\n",
                        server.DequeueRequest());
  
        CloseMailbox(session);
      }
    }

    [Test]
    public void TestAppendRemoveNonApplicableFlags()
    {
      using (var session = SelectMailbox()) {
        // APPEND transaction
        server.EnqueueResponse("+ OK continue\r\n");
        server.EnqueueResponse("0004 OK APPEND completed\r\n");
  
        var message = new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\ntest message"),
                                            new ImapMessageFlagList(ImapMessageFlag.Seen,
                                                                    ImapMessageFlag.Recent,
                                                                    ImapMessageFlag.AllowedCreateKeywords));
  
        Assert.IsTrue((bool)session.Append(message, "INBOX"));
  
        Assert.AreEqual("0004 APPEND \"INBOX\" (\\Seen) {33}\r\n" +
                        "MIME-Version: 1.0\r\n" +
                        "\r\n" +
                        "test message\r\n",
                        server.DequeueAll());
  
        CloseMailbox(session);
      }
    }

    [Test]
    public void TestAppendWithAppendUidResponseCode()
    {
      using (var session = SelectMailbox()) {
        // APPEND transaction
        server.EnqueueResponse("+ OK continue\r\n");
        server.EnqueueResponse("0004 OK [APPENDUID 38505 3955] APPEND completed\r\n");
  
        var message = new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\ntest message"),
                                            new ImapMessageFlagList(ImapMessageFlag.Seen));
  
        ImapAppendedUidSet appended;

        Assert.IsTrue((bool)session.Append(message, "INBOX", out appended));

        Assert.AreEqual("0004 APPEND \"INBOX\" (\\Seen) {33}\r\n" +
                        "MIME-Version: 1.0\r\n" +
                        "\r\n" +
                        "test message\r\n",
                        server.DequeueAll());

        Assert.IsNotNull(appended);
        Assert.AreEqual(38505L, appended.UidValidity);
        Assert.AreEqual(3955L, appended.ToNumber());
        Assert.AreEqual("3955", appended.ToString());
        CloseMailbox(session);
      }
    }

    [Test]
    public void TestAppendBeginAppendEndAppend()
    {
      AppendBeginAppendEndAppend(false);
    }

    [Test]
    public void TestAppendBeginAppendEndAppendNonSynchronizedLiterals()
    {
      AppendBeginAppendEndAppend(false);
    }

    private void AppendBeginAppendEndAppend(bool nonSynchronizedLiterals)
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

      using (var session = Authenticate(caps)) {
        var messageBodyStream = new MemoryStream(NetworkTransferEncoding.Transfer8Bit.GetBytes(messageBody));

        var asyncResult = session.BeginAppend(messageBodyStream, null, null, "INBOX");

        Assert.IsNotNull(asyncResult);
        Assert.IsFalse(asyncResult.IsCompleted);
        Assert.IsTrue(session.IsTransactionProceeding);

        server.EnqueueResponse("+ OK continue\r\n");
        server.EnqueueResponse("0002 OK [APPENDUID 38505 3955] APPEND completed\r\n");

        if (!asyncResult.AsyncWaitHandle.WaitOne(1000))
          Assert.Fail("append timed out");

        Assert.IsTrue(asyncResult.IsCompleted);

        ImapAppendedUidSet appendedUid;

        Assert.IsTrue((bool)session.EndAppend(asyncResult, out appendedUid));
        Assert.IsFalse(session.IsTransactionProceeding);

        var expected =
          string.Format("0002 APPEND \"INBOX\" {{{0}}}\r\n", messageBodyStream.Length) +
          messageBody +
          "\r\n";

        Assert.AreEqual(expected, server.DequeueAll());

        Assert.IsNotNull(appendedUid);
        Assert.AreEqual(38505L, appendedUid.UidValidity);
        Assert.AreEqual(3955L, appendedUid.ToNumber());
      }
    }

    [Test]
    public void TestAppendBeginAppendNestedCall()
    {
      using (var session = Authenticate()) {
        var messageBodyStream = new MemoryStream(Encoding.ASCII.GetBytes("message"));

        var asyncResult = session.BeginAppend(messageBodyStream, null, null, "INBOX");

        Assert.IsNotNull(asyncResult);
        Assert.IsFalse(asyncResult.IsCompleted);
        Assert.IsTrue(session.IsTransactionProceeding);

        try {
          session.BeginAppend(messageBodyStream, null, null, "INBOX");
          Assert.Fail("InvalidOperationException not thrown");
        }
        catch (InvalidOperationException) {
        }

        server.EnqueueResponse("+ OK continue\r\n");
        server.EnqueueResponse("0002 OK APPEND completed\r\n");

        if (!asyncResult.AsyncWaitHandle.WaitOne(1000))
          Assert.Fail("append timed out");

        Assert.IsTrue(asyncResult.IsCompleted);

        ImapAppendedUidSet appendedUid;

        Assert.IsTrue((bool)session.EndAppend(asyncResult, out appendedUid));

        Assert.AreEqual("0002 APPEND \"INBOX\" {7}\r\nmessage\r\n",
                                   server.DequeueAll());

        Assert.IsNull(appendedUid);
      }
    }

    [Test]
    public void TestAppendBeginAppendContinuationNo()
    {
      using (var session = Authenticate()) {
        var messageBodyStream = new MemoryStream(Encoding.ASCII.GetBytes("message"));

        var asyncResult = session.BeginAppend(messageBodyStream, null, null, "INBOX");

        Assert.IsNotNull(asyncResult);
        Assert.IsFalse(asyncResult.IsCompleted);
        Assert.IsTrue(session.IsTransactionProceeding);

        server.EnqueueResponse("0002 NO APPEND failed\r\n");

        if (!asyncResult.AsyncWaitHandle.WaitOne(1000))
          Assert.Fail("append timed out");

        Assert.IsTrue(asyncResult.IsCompleted);

        Assert.IsFalse((bool)session.EndAppend(asyncResult));

        Assert.AreEqual("0002 APPEND \"INBOX\" {7}\r\n",
                        server.DequeueAll());
      }
    }

    [Test]
    public void TestAppendEndAppendInvalidAsyncResult()
    {
      using (var session = Authenticate()) {
        try {
          session.EndAppend(null);
          Assert.Fail("ArgumentException not thrown");
        }
        catch (ArgumentException) {
        }
      }
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
      using (var session = SelectMailbox("MULTIAPPEND")) {
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
                                             new ImapMessageFlagList(ImapMessageFlag.Seen));
        var message2 = new ImapAppendMessage(Encoding.ASCII.GetBytes(messageBody2),
                                             new DateTimeOffset(new DateTime(1994, 2, 7, 22, 43, 04), new TimeSpan(-8, 0, 0)),
                                             new ImapMessageFlagList(ImapMessageFlag.Seen));

        ImapAppendedUidSet appended;

        Assert.IsTrue((bool)session.AppendMultiple(new[] {message1, message2}, "saved-messages", out appended));

        var expected =
          string.Format("0004 APPEND \"saved-messages\" (\\Seen) {{{0}}}\r\n", Encoding.ASCII.GetByteCount(messageBody1)) +
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

        CloseMailbox(session);
      }
    }

    [Test]
    public void TestAppendMultipleWithNonSyncedLiteral()
    {
      using (var session = SelectMailbox("MULTIAPPEND", "LITERAL+")) {
        // APPEND transaction
        server.EnqueueResponse("0004 OK APPEND completed\r\n");
  
        var message1 = new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\ntest message1"));
        var message2 = new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\ntest message2"));
        Assert.IsTrue((bool)session.AppendMultiple(new[] {message1, message2}, "saved-messages"));

        Assert.AreEqual("0004 APPEND \"saved-messages\" {34+}\r\n" +
                        "MIME-Version: 1.0\r\n" +
                        "\r\n" +
                        "test message1 {34+}\r\n" +
                        "MIME-Version: 1.0\r\n" +
                        "\r\n" +
                        "test message2\r\n",
                        server.DequeueAll());

        CloseMailbox(session);
      }
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestAppendMultipleIncapable()
    {
      using (var session = SelectMailbox()) {
        session.HandlesIncapableAsException = true;
        // APPEND transaction
        session.AppendMultiple(new[] {
          new ImapAppendMessage(Encoding.ASCII.GetBytes("message")),
          new ImapAppendMessage(Encoding.ASCII.GetBytes("message2")),
        }, "saved-messages");
      }
    }

    [Test]
    public void TestAppendBinary()
    {
      using (var session = SelectMailbox("BINARY")) {
        // APPEND transaction
        server.EnqueueResponse("+ OK continue\r\n");
        server.EnqueueResponse("0004 OK APPEND completed\r\n");

        Assert.IsTrue((bool)session.AppendBinary(new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\n\x00\x01\x02\x03\x04\x05\x06\x07")),
                                                 "INBOX"));

        Assert.AreEqual("0004 APPEND \"INBOX\" ~{29}\r\n" +
                        "MIME-Version: 1.0\r\n" +
                        "\r\n" +
                        "\x00\x01\x02\x03\x04\x05\x06\x07\r\n",
                        server.DequeueAll());

        CloseMailbox(session);
      }
    }

    [Test]
    public void TestAppendBinaryWithNonSynchronizedLiteral()
    {
      using (var session = SelectMailbox("BINARY", "LITERAL+")) {
        // APPEND transaction
        server.EnqueueResponse("0004 OK APPEND completed\r\n");

        Assert.IsTrue((bool)session.AppendBinary(new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\n\x00\x01\x02\x03\x04\x05\x06\x07")),
                                                 "INBOX"));

        Assert.AreEqual("0004 APPEND \"INBOX\" ~{29+}\r\n" +
                        "MIME-Version: 1.0\r\n" +
                        "\r\n" +
                        "\x00\x01\x02\x03\x04\x05\x06\x07\r\n",
                        server.DequeueAll());

        CloseMailbox(session);
      }
    }

    [Test]
    public void TestAppendBinaryMultiple()
    {
      using (var session = SelectMailbox("BINARY", "MULTIAPPEND")) {
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

        Assert.AreEqual("0004 APPEND \"INBOX\" ~{37}\r\n" +
                        "MIME-Version: 1.0\r\n" +
                        "\r\n" +
                        "message1\x00\x01\x02\x03\x04\x05\x06\x07" +
                        " ~{37}\r\n" +
                        "MIME-Version: 1.0\r\n" +
                        "\r\n" +
                        "message2\x00\x01\x02\x03\x04\x05\x06\x07\r\n",
                        server.DequeueAll());

        CloseMailbox(session);
      }
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestAppendBinaryIncapable()
    {
      using (var session = SelectMailbox()) {
        session.HandlesIncapableAsException = true;
        // APPEND transaction
        session.AppendBinary(new ImapAppendMessage(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\n\r\n\x00\x01\x02\x03\x04\x05\x06\x07")),
                             "saved-messages");
      }
    }
  }
}