using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Smdn.Net.Imap4.Protocol {
  [TestFixture]
  public class ImapSenderTests {
    private class Sender : ImapSender {
      public int UnsentCount {
        get { return base.UnsentFragments; }
      }

      public Sender(LineOrientedBufferedStream stream)
        : base(stream)
      {
      }

      public void Enqueue(params ImapString[] strings)
      {
        base.Enqueue(strings);
      }
    }

    private Sender sender;
    private MemoryStream stream;

    [SetUp]
    public void SetUp()
    {
      stream = new MemoryStream();
      sender = new Sender(new LineOrientedBufferedStream(stream));
    }

    private string GetSent()
    {
      var pos = stream.Position;
      var buffer = new byte[stream.Length];

      try {
        stream.Position = 0L;

        var len = stream.Read(buffer, 0, buffer.Length);

        return NetworkTransferEncoding.Transfer8Bit.GetString(buffer, 0, len);
      }
      finally {
        stream.Position = pos;
      }
    }

    [Test]
    public void TestSendString()
    {
      sender.Enqueue(new ImapString("string"), "NIL\r\n");
      sender.Send();

      Assert.AreEqual(0, sender.UnsentCount);
      Assert.AreEqual("string NIL\r\n",
                      GetSent());
    }

    [Test]
    public void TestSendInsertSpace()
    {
      sender.Enqueue("NIL\r\n");
      sender.Send();

      Assert.AreEqual(0, sender.UnsentCount);
      Assert.AreEqual("NIL\r\n",
                      GetSent());

      stream.SetLength(0L);

      sender.Enqueue("NIL", "NIL\r\n");
      sender.Send();

      Assert.AreEqual(0, sender.UnsentCount);
      Assert.AreEqual("NIL NIL\r\n",
                      GetSent());

      stream.SetLength(0L);

      sender.Enqueue(new ImapParenthesizedString("NIL", "NIL"), "NIL\r\n");
      sender.Send();

      Assert.AreEqual(0, sender.UnsentCount);
      Assert.AreEqual("(NIL NIL) NIL\r\n",
                      GetSent());

      stream.SetLength(0L);

      sender.Enqueue("NIL", new ImapParenthesizedString("NIL", "NIL"), "NIL\r\n");
      sender.Send();

      Assert.AreEqual(0, sender.UnsentCount);
      Assert.AreEqual("NIL (NIL NIL) NIL\r\n",
                      GetSent());

      stream.SetLength(0L);

      sender.Enqueue(new ImapParenthesizedString(new ImapParenthesizedString("NIL", "NIL"),
                                                 new ImapParenthesizedString(new ImapString[0]),
                                                 new ImapParenthesizedString("NIL")),
                     "NIL\r\n");
      sender.Send();

      Assert.AreEqual(0, sender.UnsentCount);
      Assert.AreEqual("((NIL NIL) () (NIL)) NIL\r\n",
                      GetSent());
    }

    [Test]
    public void TestSendQuotedString()
    {
      sender.Enqueue(new ImapQuotedString("quoted"), "NIL\r\n");
      sender.Send();

      Assert.AreEqual(0, sender.UnsentCount);
      Assert.AreEqual("\"quoted\" NIL\r\n",
                      GetSent());
    }

    [Test]
    public void TestSendQuotedStringContainsDQuote()
    {
      sender.Enqueue(new ImapQuotedString("\"quoted\""), "NIL\r\n");
      sender.Send();

      Assert.AreEqual(0, sender.UnsentCount);
      Assert.AreEqual("\"\\\"quoted\\\"\" NIL\r\n",
                      GetSent());
    }

    [Test]
    public void TestSendQuotedStringContainsBackSlash()
    {
      sender.Enqueue(new ImapQuotedString("\\quoted\\"), "NIL\r\n");
      sender.Send();

      Assert.AreEqual(0, sender.UnsentCount);
      Assert.AreEqual("\"\\\\quoted\\\\\" NIL\r\n",
                      GetSent());
    }

    [Test]
    public void TestSendQuotedStringContainsDQuoteAndBackSlash()
    {
      sender.Enqueue(new ImapQuotedString("\"\\Sent\""), "NIL\r\n");
      sender.Send();

      Assert.AreEqual(0, sender.UnsentCount);
      Assert.AreEqual("\"\\\"\\\\Sent\\\"\" NIL\r\n",
                      GetSent());
    }

    [Test]
    public void TestSendStringList()
    {
      sender.Enqueue(new ImapStringList("0000", "NOOP"), "NIL\r\n");
      sender.Send();

      Assert.AreEqual(0, sender.UnsentCount);
      Assert.AreEqual("0000 NOOP NIL\r\n",
                      GetSent());
    }

    [Test]
    public void TestSendParenthesizedString()
    {
      sender.Enqueue(new ImapParenthesizedString("\\Seen", "\\Flagged"), "NIL\r\n");
      sender.Send();

      Assert.AreEqual(0, sender.UnsentCount);
      Assert.AreEqual("(\\Seen \\Flagged) NIL\r\n",
                      GetSent());
    }

    [Test]
    public void TestSendParenthesizedStringEmpty()
    {
      sender.Enqueue(new ImapParenthesizedString(new ImapString[0]), "NIL\r\n");
      sender.Send();

      Assert.AreEqual(0, sender.UnsentCount);
      Assert.AreEqual("() NIL\r\n",
                      GetSent());
    }

    [Test]
    public void TestSendNestedParenthesizedString()
    {
      sender.Enqueue(new ImapParenthesizedString("OR",
                                                 new ImapParenthesizedString("UNFLAGGED", "UNSEEN"),
                                                 new ImapParenthesizedString("UNFLAGGED", "DRAFT")),
                     "NIL\r\n");
      sender.Send();

      Assert.AreEqual(0, sender.UnsentCount);
      Assert.AreEqual("(OR (UNFLAGGED UNSEEN) (UNFLAGGED DRAFT)) NIL\r\n",
                      GetSent());
    }

    [Test]
    public void TestSendLiteralStringSynchronizing()
    {
      sender.Enqueue(new ImapLiteralString("literal", Encoding.ASCII, ImapLiteralOptions.Synchronizing),
                     "NIL\r\n");
      sender.Send();

      Assert.Greater(sender.UnsentCount, 0);
      Assert.AreEqual("{7}\r\n",
                      GetSent());

      sender.Send();

      Assert.AreEqual(0, sender.UnsentCount);
      Assert.AreEqual("{7}\r\nliteral NIL\r\n",
                      GetSent());
    }

    [Test]
    public void TestSendLiteralStringNonSynchronizing()
    {
      sender.Enqueue(new ImapLiteralString("literal", Encoding.ASCII, ImapLiteralOptions.NonSynchronizing),
                     "NIL\r\n");
      sender.Send();

      Assert.AreEqual(0, sender.UnsentCount);
      Assert.AreEqual("{7+}\r\nliteral NIL\r\n",
                      GetSent());
    }

    [Test]
    public void TestSendLiteralStringLiteral8Synchronizing()
    {
      sender.Enqueue(new ImapLiteralString("literal\0", Encoding.ASCII, ImapLiteralOptions.Literal8 | ImapLiteralOptions.Synchronizing),
                     "NIL\r\n");
      sender.Send();

      Assert.Greater(sender.UnsentCount, 0);
      Assert.AreEqual("~{8}\r\n",
                      GetSent());

      sender.Send();

      Assert.AreEqual(0, sender.UnsentCount);
      Assert.AreEqual("~{8}\r\nliteral\0 NIL\r\n",
                      GetSent());
    }

    [Test]
    public void TestSendLiteralStringLiteral8NonSynchronizing()
    {
      sender.Enqueue(new ImapLiteralString("literal\0", Encoding.ASCII, ImapLiteralOptions.Literal8 | ImapLiteralOptions.NonSynchronizing),
                     "NIL\r\n");
      sender.Send();

      Assert.AreEqual(0, sender.UnsentCount);
      Assert.AreEqual("~{8+}\r\nliteral\0 NIL\r\n",
                      GetSent());
    }

    [Test]
    public void TestSendLiteralStreamSynchronizing()
    {
      var s = new MemoryStream(new byte[] {0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08}, false);

      sender.Enqueue(new ImapLiteralStream(s, ImapLiteralOptions.Synchronizing),
                     "NIL\r\n");
      sender.Send();

      Assert.Greater(sender.UnsentCount, 0);
      Assert.AreEqual("{8}\r\n",
                      GetSent());

      sender.Send();

      Assert.AreEqual(0, sender.UnsentCount);
      Assert.AreEqual("{8}\r\n\x01\x02\x03\x04\x05\x06\x07\x08 NIL\r\n",
                      GetSent());
    }

    [Test]
    public void TestSendLiteralStreamNonSynchronizing()
    {
      var s = new MemoryStream(new byte[] {0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08}, false);

      sender.Enqueue(new ImapLiteralStream(s, ImapLiteralOptions.NonSynchronizing),
                     "NIL\r\n");
      sender.Send();

      Assert.AreEqual(0, sender.UnsentCount);
      Assert.AreEqual("{8+}\r\n\x01\x02\x03\x04\x05\x06\x07\x08 NIL\r\n",
                      GetSent());
    }

    [Test]
    public void TestSendLiteralStreamLiteral8Synchronizing()
    {
      var s = new MemoryStream(new byte[] {0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07}, false);

      sender.Enqueue(new ImapLiteralStream(s, ImapLiteralOptions.Literal8 | ImapLiteralOptions.Synchronizing),
                     "NIL\r\n");
      sender.Send();

      Assert.Greater(sender.UnsentCount, 0);
      Assert.AreEqual("~{8}\r\n",
                      GetSent());

      sender.Send();

      Assert.AreEqual(0, sender.UnsentCount);
      Assert.AreEqual("~{8}\r\n\x00\x01\x02\x03\x04\x05\x06\x07 NIL\r\n",
                      GetSent());
    }

    [Test]
    public void TestSendLiteralStreamLiteral8NonSynchronizing()
    {
      var s = new MemoryStream(new byte[] {0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07}, false);

      sender.Enqueue(new ImapLiteralStream(s, ImapLiteralOptions.Literal8 | ImapLiteralOptions.NonSynchronizing),
                     "NIL\r\n");
      sender.Send();

      Assert.AreEqual(0, sender.UnsentCount);
      Assert.AreEqual("~{8+}\r\n\x00\x01\x02\x03\x04\x05\x06\x07 NIL\r\n",
                      GetSent());
    }
  }
}
