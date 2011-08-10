using System;
using System.Text;
using System.IO;
using NUnit.Framework;

namespace Smdn.Formats.Mime {
  [TestFixture]
  public class DecoderTest {

    [SetUp]
    public void Setup()
    {
      // nothing to do
    }

    [TearDown]
    public void TearDown()
    {
      // nothing to do
    }

    [Test]
    public void TestDecodeContentTypeNotExist()
    {
      var mime = MimeMessage.LoadMessage(@"MIME-Version: 1.0
Subject: transfer encoding not exist

decode as text/plain; charset=us-ascii");

      Assert.AreEqual("decode as text/plain; charset=us-ascii", mime.ReadContentAsText());
    }

    [Test]
    public void TestDecodeContentTypeNonText()
    {
      var mime = MimeMessage.LoadMessage(@"MIME-Version: 1.0
Content-Type: application/octet-stream
Subject: transfer encoding not exist

decode as binary stream");

      var text = Encoding.ASCII.GetBytes("decode as binary stream");

      Assert.AreEqual(MimeType.ApplicationOctetStream, mime.MimeType);
      Assert.AreEqual(text, mime.ReadContentAsBinary());
    }

    [Test]
    public void TestDecodeTransferEncoding7BitMessage()
    {
      var mime = MimeMessage.LoadMessage(@"MIME-Version: 1.0
Content-Type: text/plain; charset=us-ascii
Content-Transfer-Encoding: 7bit
Subject: 7bit transfer encoding

7bit transfer encoding");

      Assert.AreEqual(MimeType.TextPlain, mime.MimeType);
      Assert.AreEqual(Charsets.ASCII, mime.Charset);
      Assert.AreEqual(ContentTransferEncodingMethod.SevenBit, mime.TransferEncoding);
      Assert.AreEqual("7bit", mime.Headers["Content-Transfer-Encoding"].Value);
      Assert.AreEqual("7bit transfer encoding", mime.ReadContentAsText());
    }

    [Test]
    public void TestDecodeTransferEncoding8BitMessage()
    {
      var mime = MimeMessage.LoadMessage(@"MIME-Version: 1.0
Content-Type: text/plain; charset=iso-8859-1
Content-Transfer-Encoding: 8bit
Subject: 8bit transfer encoding

" + "8bit transfer encoding \u0080 \u00cf \u00ff\r\n");

      Assert.AreEqual(MimeType.TextPlain, mime.MimeType);
      Assert.AreEqual(Charsets.Latin1, mime.Charset);
      Assert.AreEqual(ContentTransferEncodingMethod.EightBit, mime.TransferEncoding);
      Assert.AreEqual("8bit", mime.Headers["Content-Transfer-Encoding"].Value);
      Assert.AreEqual("8bit transfer encoding \u0080 \u00cf \u00ff\r\n", mime.ReadContentAsText());
    }

    [Test]
    public void TestDecodeTransferEncodingBinaryMessage()
    {
      var message = new MemoryStream(0x200);

      var header = latin1Encoding.GetBytes(@"MIME-Version: 1.0
Content-Type: application/octet-stream
Content-Transfer-Encoding: binary
Subject: binary transfer encoding

");

      message.Write(header, 0, header.Length);

      var bodyStream = new MemoryStream(); 

      for (var i = 0; i < 0x100; i++) {
        bodyStream.WriteByte((byte)i);
      }

      var body = bodyStream.ToArray();

      message.Write(body, 0, body.Length);

      var mime = MimeMessage.Load(new MemoryStream(message.ToArray()));

      Assert.AreEqual(MimeType.ApplicationOctetStream, mime.MimeType);
      Assert.AreEqual(ContentTransferEncodingMethod.Binary, mime.TransferEncoding);
      Assert.AreEqual("binary", mime.Headers["Content-Transfer-Encoding"].Value);

      Assert.AreEqual(body, mime.ReadContentAsBinary());
    }

    [Test]
    public void TestDecodeTransferEncodingBase64Message()
    {
      var mime = MimeMessage.LoadMessage(@"MIME-Version: 1.0
Content-Type: text/plain; charset=shift_jis
Content-Transfer-Encoding: base64
Subject: base64 transfer encoding

ir+OmmFiY4KpgsgxMjODSoNpDQqCUIJRglKCgYKCgoMNCg==");

      Assert.AreEqual(MimeType.TextPlain, mime.MimeType);
      Assert.AreEqual(Charsets.ShiftJIS, mime.Charset);
      Assert.AreEqual(ContentTransferEncodingMethod.Base64, mime.TransferEncoding);
      Assert.AreEqual("base64", mime.Headers["Content-Transfer-Encoding"].Value);
      Assert.AreEqual("漢字abcかな123カナ\r\n１２３ａｂｃ\r\n", mime.ReadContentAsText());
    }

    [Test]
    public void TestDecodeTransferEncodingQuotedPrintableMessage()
    {
      var mime = MimeMessage.LoadMessage(@"MIME-Version: 1.0
Content-Type: text/plain; charset=euc-jp
Content-Transfer-Encoding: quoted-printable
Subject: quoted printable transfer encoding

=B4=C1=BB=FAabc=A4=AB=A4=CA123=A5=AB=A5=CA=0D=0A=A3=B1=A3=B2=A3=B3=A3=E1=A3=
=E2=A3=E3=0D=0A");

      Assert.AreEqual(MimeType.TextPlain, mime.MimeType);
      Assert.AreEqual(Encoding.GetEncoding("euc-jp"), mime.Charset);
      Assert.AreEqual(ContentTransferEncodingMethod.QuotedPrintable, mime.TransferEncoding);
      Assert.AreEqual("quoted-printable", mime.Headers["Content-Transfer-Encoding"].Value);
      Assert.AreEqual("漢字abcかな123カナ\r\n１２３ａｂｃ\r\n", mime.ReadContentAsText());
    }

    [Test]
    public void TestDecodeTransferEncodingUnsupported()
    {
      var mime = MimeMessage.LoadMessage(@"MIME-Version: 1.0
Content-Type: text/plain; charset=euc-jp
Content-Transfer-Encoding: unsupported
Subject: unsupported transfer encoding

unsupported transfer encoding");

      Assert.AreEqual(MimeType.TextPlain, mime.MimeType);
      Assert.AreEqual(Encoding.GetEncoding("euc-jp"), mime.Charset);
      Assert.AreEqual(ContentTransferEncodingMethod.Unknown, mime.TransferEncoding);
      Assert.AreEqual("unsupported", mime.Headers["Content-Transfer-Encoding"].Value);

      try {
        mime.ReadContentAsText();

        Assert.Fail("NotSupportedException not thrown");
      }
      catch (NotSupportedException) {
      }
    }

    [Test]
    public void TestDecodeContentCharsetContainsLeadingWhitespaces()
    {
      var mime = MimeMessage.LoadMessage(@"MIME-Version: 1.0
Content-Type: text/plain; charset= iso-2022-jp
Subject: test

content");

      Assert.AreEqual(MimeType.TextPlain, mime.MimeType);
      Assert.AreEqual(Charsets.JIS, mime.Charset);
      Assert.AreEqual("content", mime.ReadContentAsText());
    }

    [Test]
    public void TestDecodeContentCharsetContainsTrailingWhitespaces()
    {
      var mime = MimeMessage.LoadMessage(@"MIME-Version: 1.0
Content-Type: text/plain; charset=iso-2022-jp 
Subject: test

content");

      Assert.AreEqual(MimeType.TextPlain, mime.MimeType);
      Assert.AreEqual(Charsets.JIS, mime.Charset);
      Assert.AreEqual("content", mime.ReadContentAsText());
    }

    [Test]
    public void TestDecodeContentCharsetQuoted()
    {
      var mime = MimeMessage.LoadMessage(@"MIME-Version: 1.0
Content-Type: text/plain; charset=""iso-2022-jp""
Subject: test

content");

      Assert.AreEqual(MimeType.TextPlain, mime.MimeType);
      Assert.AreEqual(Charsets.JIS, mime.Charset);
      Assert.AreEqual("content", mime.ReadContentAsText());
    }

    [Test]
    public void TestDecodeContentCharsetQuotedContainsWhitespaces1()
    {
      var mime = MimeMessage.LoadMessage(@"MIME-Version: 1.0
Content-Type: text/plain; charset="" iso-2022-jp ""
Subject: test

content");

      Assert.AreEqual(MimeType.TextPlain, mime.MimeType);
      Assert.AreEqual(Charsets.JIS, mime.Charset);
      Assert.AreEqual("content", mime.ReadContentAsText());
    }

    [Test]
    public void TestDecodeContentCharsetQuotedContainsWhitespaces2()
    {
      var mime = MimeMessage.LoadMessage(@"MIME-Version: 1.0
Content-Type: text/plain; charset=""shift jis""
Subject: test

content");

      Assert.AreEqual(MimeType.TextPlain, mime.MimeType);
      Assert.AreEqual(Charsets.ShiftJIS, mime.Charset);
      Assert.AreEqual("content", mime.ReadContentAsText());
    }

    [Test]
    public void TestDecodeContentCharsetIllegal1()
    {
      var mime = MimeMessage.LoadMessage(@"MIME-Version: 1.0
Content-Type: text/plain; charset=shift-jis
Subject: test

content");

      Assert.AreEqual(MimeType.TextPlain, mime.MimeType);
      Assert.AreEqual(Charsets.ShiftJIS, mime.Charset);
      Assert.AreEqual("content", mime.ReadContentAsText());
    }

    [Test]
    public void TestDecodeContentCharsetIllegal2()
    {
      var mime = MimeMessage.LoadMessage(@"MIME-Version: 1.0
Content-Type: text/plain; charset=euc_jp
Subject: test

content");

      Assert.AreEqual(MimeType.TextPlain, mime.MimeType);
      Assert.AreEqual(Encoding.GetEncoding("euc-jp"), mime.Charset);
      Assert.AreEqual("content", mime.ReadContentAsText());
    }

    [Test]
    public void TestDecodeContentCharsetIllegal3()
    {
      var mime = MimeMessage.LoadMessage(@"MIME-Version: 1.0
Content-Type: text/plain; charset=iso_2022-jp
Subject: test

content");

      Assert.AreEqual(MimeType.TextPlain, mime.MimeType);
      Assert.AreEqual(Charsets.JIS, mime.Charset);
      Assert.AreEqual("content", mime.ReadContentAsText());
    }

    private Encoding latin1Encoding = Encoding.GetEncoding("latin1");
  }
}
