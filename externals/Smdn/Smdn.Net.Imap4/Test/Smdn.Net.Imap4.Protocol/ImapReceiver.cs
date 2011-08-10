using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Smdn.Net.Imap4.Protocol {
  [TestFixture]
  public class ImapReceiverTests {
    private class Receiver : ImapReceiver {
      public Receiver(LineOrientedBufferedStream stream)
        : base(stream)
      {
      }

      public ImapData[] ParseNonTerminatedText()
      {
        var line = base.Receive();

        return ParseDataNonTerminatedText(line, 0);
      }

      public ImapData[] Parse()
      {
        var line = base.Receive();

        return ParseData(line, 0, ref context);
      }

      private IParsingContext context = null;
    }

    private MemoryStream stream;
    private Receiver receiver;

    [SetUp]
    public void SetUp()
    {
      stream = new MemoryStream();
      receiver = new Receiver(new LineOrientedBufferedStream(stream));
    }

    private void WriteReceived(string received)
    {
      var r = Encoding.ASCII.GetBytes(received);

      stream.Write(r, 0, r.Length);
      stream.Position = 0L;
    }

    [Test]
    public void TestParseNIL()
    {
      WriteReceived("NIL\r\n");

      var data = receiver.Parse();

      Assert.IsNotNull(data);
      Assert.AreEqual(1, data.Length);
      Assert.AreEqual(ImapDataFormat.Nil, data[0].Format);
    }

    [Test]
    public void TestParseString()
    {
      WriteReceived("INBOX\r\n");

      var data = receiver.Parse();

      Assert.IsNotNull(data);
      Assert.AreEqual(1, data.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[0].Format);
      Assert.AreEqual("INBOX", data[0].GetTextAsString());
    }

    [Test]
    public void TestParseDelimitedBySingleSpace()
    {
      WriteReceived("INBOX NIL ~/Mail/foo\r\n");

      var data = receiver.Parse();

      Assert.IsNotNull(data);
      Assert.AreEqual(3, data.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[0].Format);
      Assert.AreEqual("INBOX", data[0].GetTextAsString());
      Assert.AreEqual(ImapDataFormat.Nil, data[1].Format);
      Assert.AreEqual(ImapDataFormat.Text, data[2].Format);
      Assert.AreEqual("~/Mail/foo", data[2].GetTextAsString());
    }

    [Test]
    public void TestParseDelimitedByMultipleSpace()
    {
      WriteReceived("INBOX   NIL   ~/Mail/foo\r\n");

      var data = receiver.Parse();

      Assert.IsNotNull(data);
      Assert.AreEqual(3, data.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[0].Format);
      Assert.AreEqual("INBOX", data[0].GetTextAsString());
      Assert.AreEqual(ImapDataFormat.Nil, data[1].Format);
      Assert.AreEqual(ImapDataFormat.Text, data[2].Format);
      Assert.AreEqual("~/Mail/foo", data[2].GetTextAsString());
    }

    [Test]
    public void TestParseStringContainsSection()
    {
      WriteReceived("BODY[HEADER.FIELDS (SUBJECT DATE)]\r\n");

      var data = receiver.Parse();

      Assert.IsNotNull(data);
      Assert.AreEqual(1, data.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[0].Format);
      Assert.AreEqual("BODY[HEADER.FIELDS (SUBJECT DATE)]", data[0].GetTextAsString());
    }

    [Test]
    public void TestParseStringStartsWithTildeButNotLiteral8()
    {
      WriteReceived("~/Mail/foo\r\n");

      var data = receiver.Parse();

      Assert.IsNotNull(data);
      Assert.AreEqual(1, data.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[0].Format);
      Assert.AreEqual("~/Mail/foo", data[0].GetTextAsString());
    }

    [Test]
    public void TestParseQuotedString()
    {
      WriteReceived("\"INBOX\"\r\n");

      var data = receiver.Parse();

      Assert.IsNotNull(data);
      Assert.AreEqual(1, data.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[0].Format);
      Assert.AreEqual("INBOX", data[0].GetTextAsString());
    }

    [Test]
    public void TestParseQuotedStringContainsEscapedDQuote()
    {
      WriteReceived("\"INBOX.\\\"QUOTED\\\"\"\r\n");

      var data = receiver.Parse();

      Assert.IsNotNull(data);
      Assert.AreEqual(1, data.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[0].Format);
      Assert.AreEqual("INBOX.\"QUOTED\"", data[0].GetTextAsString());
    }

    [Test]
    public void TestParseQuotedStringContainsEscapedBackSlash()
    {
      WriteReceived("\"INBOX.\\\\(^o^)/\"\r\n");

      var data = receiver.Parse();

      Assert.IsNotNull(data);
      Assert.AreEqual(1, data.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[0].Format);
      Assert.AreEqual("INBOX.\\(^o^)/", data[0].GetTextAsString());
    }

    [Test]
    public void TestParseParenthesizedContainsString()
    {
      WriteReceived("(\\Noselect)\r\n");

      var data = receiver.Parse();

      Assert.IsNotNull(data);
      Assert.AreEqual(1, data.Length);
      Assert.AreEqual(ImapDataFormat.List, data[0].Format);
      Assert.AreEqual(1, data[0].List.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[0].List[0].Format);
      Assert.AreEqual("\\Noselect", data[0].List[0].GetTextAsString());
    }

    [Test]
    public void TestParseParenthesizedContainsQuotedString()
    {
      WriteReceived("(\"INBOX\")\r\n");

      var data = receiver.Parse();

      Assert.IsNotNull(data);
      Assert.AreEqual(1, data.Length);
      Assert.AreEqual(ImapDataFormat.List, data[0].Format);
      Assert.AreEqual(1, data[0].List.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[0].List[0].Format);
      Assert.AreEqual("INBOX", data[0].List[0].GetTextAsString());
    }

    [Test]
    public void TestParseParenthesizedContainsLiteral()
    {
      WriteReceived("({4}\r\n1234)\r\n");

      var data = receiver.Parse();

      Assert.IsNull(data);

      data = receiver.Parse();

      Assert.IsNotNull(data);
      Assert.AreEqual(1, data.Length);
      Assert.AreEqual(ImapDataFormat.List, data[0].Format);
      Assert.AreEqual(1, data[0].List.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[0].List[0].Format);
      Assert.AreEqual("1234", data[0].List[0].GetTextAsString());
    }

    [Test]
    public void TestParseParenthesizedEmpty()
    {
      WriteReceived("()\r\n");

      var data = receiver.Parse();

      Assert.IsNotNull(data);
      Assert.AreEqual(1, data.Length);
      Assert.AreEqual(ImapDataFormat.List, data[0].Format);
      Assert.AreEqual(0, data[0].List.Length);
    }

    [Test]
    public void TestParseParenthesizedTwoOrMore()
    {
      WriteReceived(@"(NIL \Noselect ""Welcome to the \""Mono-devel-list\"" mailing list"")" + "\r\n");

      var data = receiver.Parse();

      Assert.IsNotNull(data);
      Assert.AreEqual(1, data.Length);
      Assert.AreEqual(ImapDataFormat.List, data[0].Format);
      Assert.AreEqual(3, data[0].List.Length);
      Assert.AreEqual(ImapDataFormat.Nil, data[0].List[0].Format);
      Assert.AreEqual(ImapDataFormat.Text, data[0].List[1].Format);
      Assert.AreEqual("\\Noselect", data[0].List[1].GetTextAsString());
      Assert.AreEqual(ImapDataFormat.Text, data[0].List[2].Format);
      Assert.AreEqual(@"Welcome to the ""Mono-devel-list"" mailing list", data[0].List[2].GetTextAsString());
    }

    [Test]
    public void TestParseParenthesizedNested1()
    {
      WriteReceived("((\"\" \"/\")) ((\"~\" \"/\")) ((\"#shared/\" \"/\")(\"#public/\" \"/\")(\"#ftp/\" \"/\")(\"#news.\" \".\"))\r\n");

      var data = receiver.Parse();

      Assert.AreEqual(3, data.Length);
      Assert.AreEqual(ImapDataFormat.List, data[0].Format);
      Assert.AreEqual(ImapDataFormat.List, data[1].Format);
      Assert.AreEqual(ImapDataFormat.List, data[2].Format);

      Assert.AreEqual(1,                   data[0].List.Length, "data[0] length");
      Assert.AreEqual(ImapDataFormat.List, data[0].List[0].Format);
      Assert.AreEqual(2,                   data[0].List[0].List.Length, "data[0].List[0] length");
      Assert.AreEqual(ImapDataFormat.Text, data[0].List[0].List[0].Format);
      Assert.AreEqual("",                  data[0].List[0].List[0].GetTextAsString());
      Assert.AreEqual(ImapDataFormat.Text, data[0].List[0].List[1].Format);
      Assert.AreEqual("/",                 data[0].List[0].List[1].GetTextAsString());

      Assert.AreEqual(1,                   data[1].List.Length, "data[1] length");
      Assert.AreEqual(ImapDataFormat.List, data[1].List[0].Format);
      Assert.AreEqual(2,                   data[1].List[0].List.Length, "data[1].List[0] length");
      Assert.AreEqual(ImapDataFormat.Text, data[1].List[0].List[0].Format);
      Assert.AreEqual("~",                 data[1].List[0].List[0].GetTextAsString());
      Assert.AreEqual(ImapDataFormat.Text, data[1].List[0].List[1].Format);
      Assert.AreEqual("/",                 data[1].List[0].List[1].GetTextAsString());

      Assert.AreEqual(4,                   data[2].List.Length, "data[2] length");
      Assert.AreEqual(ImapDataFormat.List, data[2].List[0].Format);
      Assert.AreEqual(2,                   data[2].List[0].List.Length, "data[2].List[0] length");
      Assert.AreEqual(ImapDataFormat.Text, data[2].List[0].List[0].Format);
      Assert.AreEqual("#shared/",          data[2].List[0].List[0].GetTextAsString());
      Assert.AreEqual(ImapDataFormat.Text, data[2].List[0].List[1].Format);
      Assert.AreEqual("/",                 data[2].List[0].List[1].GetTextAsString());
      Assert.AreEqual(2,                   data[2].List[1].List.Length, "data[2].List[1] length");
      Assert.AreEqual(ImapDataFormat.Text, data[2].List[1].List[0].Format);
      Assert.AreEqual("#public/",          data[2].List[1].List[0].GetTextAsString());
      Assert.AreEqual(ImapDataFormat.Text, data[2].List[1].List[1].Format);
      Assert.AreEqual("/",                 data[2].List[1].List[1].GetTextAsString());
      Assert.AreEqual(2,                   data[2].List[2].List.Length, "data[2].List[2] length");
      Assert.AreEqual(ImapDataFormat.Text, data[2].List[2].List[0].Format);
      Assert.AreEqual("#ftp/",             data[2].List[2].List[0].GetTextAsString());
      Assert.AreEqual(ImapDataFormat.Text, data[2].List[2].List[1].Format);
      Assert.AreEqual("/",                 data[2].List[2].List[1].GetTextAsString());
      Assert.AreEqual(2,                   data[2].List[3].List.Length, "data[2].List[3] length");
      Assert.AreEqual(ImapDataFormat.Text, data[2].List[3].List[0].Format);
      Assert.AreEqual("#news.",            data[2].List[3].List[0].GetTextAsString());
      Assert.AreEqual(ImapDataFormat.Text, data[2].List[3].List[1].Format);
      Assert.AreEqual(".",                 data[2].List[3].List[1].GetTextAsString());
    }

    [Test]
    public void TestParseParenthesizedNested2()
    {
      WriteReceived("(2)(3 6 (4 23)(44 7 96))\r\n");

      var data = receiver.Parse();

      Assert.AreEqual(2, data.Length);
      Assert.AreEqual(ImapDataFormat.List, data[0].Format);
      Assert.AreEqual(ImapDataFormat.List, data[1].Format);

      Assert.AreEqual(1, data[0].List.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[0].List[0].Format);
      Assert.AreEqual(2, data[0].List[0].GetTextAsNumber());

      Assert.AreEqual(4, data[1].List.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[1].List[0].Format);
      Assert.AreEqual(ImapDataFormat.Text, data[1].List[1].Format);
      Assert.AreEqual(ImapDataFormat.List, data[1].List[2].Format);
      Assert.AreEqual(ImapDataFormat.List, data[1].List[3].Format);

      Assert.AreEqual(3,  data[1].List[0].GetTextAsNumber());
      Assert.AreEqual(6,  data[1].List[1].GetTextAsNumber());
      Assert.AreEqual(2,  data[1].List[2].List.Length);
      Assert.AreEqual(4,  data[1].List[2].List[0].GetTextAsNumber());
      Assert.AreEqual(23, data[1].List[2].List[1].GetTextAsNumber());
      Assert.AreEqual(3,  data[1].List[3].List.Length);
      Assert.AreEqual(44, data[1].List[3].List[0].GetTextAsNumber());
      Assert.AreEqual(7,  data[1].List[3].List[1].GetTextAsNumber());
      Assert.AreEqual(96, data[1].List[3].List[2].GetTextAsNumber());
    }

    [Test]
    public void TestParseParenthesizedNested3()
    {
      WriteReceived("(BODY[] {12}\r\ntest message)\r\n");

      var data = receiver.Parse();

      Assert.IsNull(data);

      data = receiver.Parse();

      Assert.IsNotNull(data);
      Assert.AreEqual(1, data.Length);
      Assert.AreEqual(ImapDataFormat.List, data[0].Format);
      Assert.AreEqual(2, data[0].List.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[0].List[0].Format);
      Assert.AreEqual(ImapDataFormat.Text, data[0].List[1].Format);
      Assert.AreEqual("BODY[]", data[0].List[0].GetTextAsString());
      Assert.AreEqual("test message", data[0].List[1].GetTextAsString());
    }

    [Test]
    public void TestParseParenthesizedNestedEmpty()
    {
      WriteReceived("(() ())\r\n");

      var data = receiver.Parse();

      Assert.IsNotNull(data);
      Assert.AreEqual(1, data.Length);
      Assert.AreEqual(ImapDataFormat.List, data[0].Format);
      Assert.AreEqual(2, data[0].List.Length);

      Assert.AreEqual(ImapDataFormat.List, data[0].List[0].Format);
      Assert.AreEqual(0, data[0].List[0].List.Length);

      Assert.AreEqual(ImapDataFormat.List, data[0].List[1].Format);
      Assert.AreEqual(0, data[0].List[1].List.Length);
    }

    [Test]
    public void TestParseLiteral()
    {
      WriteReceived("{8}\r\n12345678\r\n");

      var data = receiver.Parse();

      Assert.IsNull(data);

      data = receiver.Parse();

      Assert.IsNotNull(data);
      Assert.AreEqual(1, data.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[0].Format);
      Assert.AreEqual("12345678", data[0].GetTextAsString());
    }

    [Test]
    public void TestParseLiteralEndWithCRLF()
    {
      WriteReceived("{8}\r\nabcdef\r\n\r\n");

      var data = receiver.Parse();

      Assert.IsNull(data);

      data = receiver.Parse();

      Assert.IsNotNull(data);
      Assert.AreEqual(1, data.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[0].Format);
      Assert.AreEqual("abcdef\r\n", data[0].GetTextAsString());
    }

    [Test]
    public void TestParseLiteralZeroLength()
    {
      WriteReceived("{0}\r\n\r\n");

      var data = receiver.Parse();

      Assert.IsNull(data);

      data = receiver.Parse();

      Assert.IsNotNull(data);
      Assert.AreEqual(1, data.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[0].Format);
      Assert.AreEqual(string.Empty, data[0].GetTextAsString());
    }

    [Test]
    public void TestParseLargeLiteral()
    {
      var literal = new ByteStringBuilder(1 * 1024 * 1024);

      for (var i = 0; i < literal.Capacity; i += 16) {
        literal.Append("0123456789abcdef");
      }

      WriteReceived(string.Format("{{{0}}}\r\n{1}\r\n", literal.Length, literal.ToString()));

      var data = receiver.Parse();

      Assert.IsNull(data);

      data = receiver.Parse();

      Assert.IsNotNull(data);
      Assert.AreEqual(1, data.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[0].Format);

      var stream = data[0].GetTextAsStream();

      Assert.IsNotInstanceOfType(typeof(MemoryStream), stream);

      Assert.AreEqual(literal.ToByteArray(),
                      Smdn.IO.StreamExtensions.ReadToEnd(stream));
    }

    [Test]
    public void TestParseLiteralContainsSpecialChars()
    {
      WriteReceived("{16}\r\n(\"quoted\r\n {8}\")\r\n");

      var data = receiver.Parse();

      Assert.IsNull(data);

      data = receiver.Parse();

      Assert.IsNotNull(data);
      Assert.AreEqual(1, data.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[0].Format);
      Assert.AreEqual("(\"quoted\r\n {8}\")", data[0].GetTextAsString());
    }

    [Test]
    public void TestParseLiteral8()
    {
      WriteReceived("~{8}\r\n\x00\x01\x02\x03\x04\x05\x06\x07\r\n");

      var data = receiver.Parse();

      Assert.IsNull(data);

      data = receiver.Parse();

      Assert.IsNotNull(data);
      Assert.AreEqual(1, data.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[0].Format);
      Assert.AreEqual("\x00\x01\x02\x03\x04\x05\x06\x07", data[0].GetTextAsString());
    }

    [Test]
    public void TestParseLiteralFollowingLiteral()
    {
      WriteReceived("{8}\r\n12345678 {4}\r\n1234 {4}\r\n5678\r\n");

      var data = receiver.Parse();

      Assert.IsNull(data);

      data = receiver.Parse();

      Assert.IsNull(data);

      data = receiver.Parse();

      Assert.IsNull(data);

      data = receiver.Parse();

      Assert.IsNotNull(data);

      Assert.AreEqual(3, data.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[0].Format);
      Assert.AreEqual("12345678", data[0].GetTextAsString());
      Assert.AreEqual(ImapDataFormat.Text, data[1].Format);
      Assert.AreEqual("1234", data[1].GetTextAsString());
      Assert.AreEqual(ImapDataFormat.Text, data[2].Format);
      Assert.AreEqual("5678", data[2].GetTextAsString());
    }

    [Test]
    public void TestParseNonTerminatedText()
    {
      WriteReceived("BADCHARSET (UTF-8 SHIFT-JIS)");

      var data = receiver.ParseNonTerminatedText();

      Assert.IsNotNull(data);

      Assert.AreEqual(2, data.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[0].Format);
      Assert.AreEqual("BADCHARSET", data[0].GetTextAsString());
      Assert.AreEqual(ImapDataFormat.List, data[1].Format);
      Assert.AreEqual(2, data[1].List.Length);
      Assert.AreEqual(ImapDataFormat.Text, data[1].List[0].Format);
      Assert.AreEqual("UTF-8", data[1].List[0].GetTextAsString());
      Assert.AreEqual(ImapDataFormat.Text, data[1].List[1].Format);
      Assert.AreEqual("SHIFT-JIS", data[1].List[1].GetTextAsString());
    }
  }
}
