using System;
using System.IO;
using System.Text;
using NUnit.Framework;

using Smdn.IO;

namespace Smdn.Net.Imap4.Protocol {
  [TestFixture]
  public class ImapDataTests {
    [Test]
    public void TestNil()
    {
      var nil = ImapData.CreateNilData();

      Assert.AreEqual(ImapDataFormat.Nil, nil.Format);

      Assert.AreSame(nil, ImapData.CreateNilData());
    }

    [Test]
    public void TestList()
    {
      var list = ImapData.CreateListData(new[] {
        ImapData.CreateNilData(),
        ImapData.CreateNilData(),
        ImapData.CreateTextData(new ByteString("text")),
      });

      Assert.AreEqual(ImapDataFormat.List, list.Format);
      Assert.IsNotNull(list.List);
      Assert.AreEqual(3, list.List.Length);
    }

    [Test]
    public void TestCreateTextDataFromByteString()
    {
      var str = new ByteString("string");
      var text = ImapData.CreateTextData(str);

      Assert.AreEqual(ImapDataFormat.Text, text.Format);
      Assert.AreEqual(6, text.GetTextLength());
      Assert.AreEqual(Encoding.ASCII.GetBytes("string"), text.GetTextAsByteArray());
      Assert.AreSame(str.ByteArray, text.GetTextAsByteArray());
      Assert.AreEqual(new ByteString("string"), text.GetTextAsByteString());
      Assert.AreSame(str, text.GetTextAsByteString());
      Assert.AreEqual("string", text.GetTextAsString());

      FileAssert.AreEqual(new MemoryStream(str.ByteArray, false), text.GetTextAsStream());

      var buffer = new byte[3];

      text.CopyText(buffer, 0, buffer.Length);

      Assert.AreEqual(str.ByteArray.Slice(0, 3), buffer);
    }

    [Test]
    public void TestCreateTextDataFromStream()
    {
      var stream = new ChunkedMemoryStream();
      var str = Encoding.ASCII.GetBytes("string");

      stream.Write(str, 0, str.Length);
      stream.Position = 0L;

      var text = ImapData.CreateTextData(stream);

      Assert.AreEqual(ImapDataFormat.Text, text.Format);
      Assert.AreEqual(6, text.GetTextLength());
      Assert.AreEqual(Encoding.ASCII.GetBytes("string"), text.GetTextAsByteArray());
      Assert.AreEqual(new ByteString(str), text.GetTextAsByteString());
      Assert.AreEqual("string", text.GetTextAsString());

      Assert.AreSame(stream, text.GetTextAsStream());

      FileAssert.AreEqual(new MemoryStream(str, false), text.GetTextAsStream());

      var buffer = new byte[3];

      text.CopyText(buffer, 0, buffer.Length);

      Assert.AreEqual(str.Slice(0, 3), buffer);
    }

    [Test]
    public void TestGetTextAsNumber()
    {
      Assert.AreEqual(0UL, ImapData.CreateTextData(new ByteString("0")).GetTextAsNumber());
      Assert.AreEqual(1UL, ImapData.CreateTextData(new ByteString("1")).GetTextAsNumber());
      Assert.AreEqual(5UL, ImapData.CreateTextData(new ByteString("005")).GetTextAsNumber());
      Assert.AreEqual(123UL, ImapData.CreateTextData(new ByteString("123")).GetTextAsNumber());

      try {
        ImapData.CreateTextData(new ByteString("18446744073709551616")).GetTextAsNumber();
        Assert.Fail("OverflowException not thrown");
      }
      catch (OverflowException) {
      }
    }
  }
}
