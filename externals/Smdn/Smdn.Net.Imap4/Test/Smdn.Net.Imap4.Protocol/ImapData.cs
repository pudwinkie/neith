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
        ImapData.CreateTextData(ByteString.CreateImmutable("text")),
      });

      Assert.AreEqual(ImapDataFormat.List, list.Format);
      Assert.IsNotNull(list.List);
      Assert.AreEqual(3, list.List.Length);
    }

    [Test]
    public void TestCreateTextDataFromByteString()
    {
      var str = ByteString.CreateImmutable("string");
      var text = ImapData.CreateTextData(str);

      Assert.AreEqual(ImapDataFormat.Text, text.Format);
      Assert.AreEqual(6, text.GetTextLength());
      Assert.AreEqual(Encoding.ASCII.GetBytes("string"), text.GetTextAsByteArray());
      Assert.AreEqual(str.ToArray(), text.GetTextAsByteArray());
      Assert.AreEqual(ByteString.CreateImmutable("string"), text.GetTextAsByteString());
      Assert.AreSame(str, text.GetTextAsByteString());
      Assert.AreEqual("string", text.GetTextAsString());

      FileAssert.AreEqual(new MemoryStream(str.Segment.Array, str.Segment.Offset, str.Segment.Count, false),
                          text.GetTextAsStream());

      var buffer = new byte[3];

      text.CopyText(buffer, 0, buffer.Length);

      Assert.AreEqual(str.Substring(0, 3).ToArray(), buffer);
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
      Assert.AreEqual(ByteString.CreateImmutable(str), text.GetTextAsByteString());
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
      Assert.AreEqual(0UL, ImapData.CreateTextData(ByteString.CreateImmutable("0")).GetTextAsNumber());
      Assert.AreEqual(1UL, ImapData.CreateTextData(ByteString.CreateImmutable("1")).GetTextAsNumber());
      Assert.AreEqual(5UL, ImapData.CreateTextData(ByteString.CreateImmutable("005")).GetTextAsNumber());
      Assert.AreEqual(123UL, ImapData.CreateTextData(ByteString.CreateImmutable("123")).GetTextAsNumber());

      try {
        ImapData.CreateTextData(ByteString.CreateImmutable("18446744073709551616")).GetTextAsNumber();
        Assert.Fail("OverflowException not thrown");
      }
      catch (OverflowException) {
      }
    }

    [Test]
    public void TestSerializeBinaryTextNil()
    {
      var nil = ImapData.CreateNilData();

      TestUtils.SerializeBinary(nil, delegate(ImapData deserialized) {
        Assert.AreEqual(ImapDataFormat.Nil, deserialized.Format);
        Assert.IsNull(deserialized.List);

        Assert.AreNotSame(deserialized, ImapData.CreateNilData());
      });
    }

    [Test]
    public void TestSerializeBinaryList()
    {
      var list = ImapData.CreateListData(new[] {
        ImapData.CreateNilData(),
        ImapData.CreateNilData(),
        ImapData.CreateTextData(ByteString.CreateImmutable("text")),
      });

      TestUtils.SerializeBinary(list, delegate(ImapData deserialized) {
        Assert.AreEqual(ImapDataFormat.List, deserialized.Format);
        Assert.IsNotNull(deserialized.List);
        Assert.AreEqual(3, deserialized.List.Length);
      });
    }

    [Test]
    public void TestSerializeBinaryTextByteString()
    {
      var str = ByteString.CreateImmutable("string");
      var text = ImapData.CreateTextData(str);

      TestUtils.SerializeBinary(text, delegate(ImapData deserialized) {
        Assert.AreEqual(ImapDataFormat.Text, deserialized.Format);
        Assert.AreEqual(6, deserialized.GetTextLength());
        Assert.AreNotSame(str, deserialized.GetTextAsByteString());
        Assert.AreEqual("string", deserialized.GetTextAsString());

        Assert.IsNull(deserialized.List);
      });
    }

    [Test]
    public void TestSerializeBinaryTextStream()
    {
      var stream = new ChunkedMemoryStream();
      var str = Encoding.ASCII.GetBytes("string");

      stream.Write(str, 0, str.Length);
      stream.Position = 0L;

      var text = ImapData.CreateTextData(stream);

      TestUtils.SerializeBinary(text, delegate(ImapData deserialized) {
        Assert.AreEqual(ImapDataFormat.Text, deserialized.Format);
        Assert.AreEqual(6, deserialized.GetTextLength());
        Assert.AreNotSame(stream, deserialized.GetTextAsStream());
        Assert.AreEqual("string", deserialized.GetTextAsString());

        Assert.IsNull(deserialized.List);
      });
    }
  }
}
