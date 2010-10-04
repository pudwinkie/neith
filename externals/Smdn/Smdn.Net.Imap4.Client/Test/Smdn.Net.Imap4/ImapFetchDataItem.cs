using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapFetchDataItemTest {
    [Test]
    public void TestCombine()
    {
      var fetch1 = ImapFetchDataItem.Envelope;
      var fetch2 = ImapFetchDataItem.Uid;

      Assert.AreEqual("(ENVELOPE UID)", ImapFetchDataItem.Combine(fetch1, fetch2).ToString());
      Assert.AreEqual("(ENVELOPE UID)", (fetch1 + fetch2).ToString());
      Assert.AreEqual("(ENVELOPE UID)", fetch1.CombineWith(fetch2).ToString());

      Assert.AreEqual("(UID ENVELOPE)", (fetch2 + fetch1).ToString());
    }

    [Test]
    public void TestCombineWithMacro()
    {
      Assert.AreEqual("(FLAGS INTERNALDATE RFC822.SIZE UID)", (ImapFetchDataItem.Fast + ImapFetchDataItem.Uid).ToString());
    }

    [Test]
    public void TestBody()
    {
      Assert.AreEqual("(BODY[])", ImapFetchDataItem.BodyText().ToString());
      Assert.AreEqual("(BODY.PEEK[])", ImapFetchDataItem.BodyPeek().ToString());
      Assert.AreEqual("(BODY[])", ImapFetchDataItem.BodyText(false).ToString());
      Assert.AreEqual("(BODY.PEEK[])", ImapFetchDataItem.BodyText(true).ToString());
      Assert.AreEqual("(BODY[heADEr])", ImapFetchDataItem.BodyText("heADEr").ToString());
      Assert.AreEqual("(BODY.PEEK[]<1000.2000>)", ImapFetchDataItem.BodyPeek(1000L, 2000L).ToString());
      Assert.AreEqual("(BODY.PEEK[]<1000.2000>)", ImapFetchDataItem.BodyPeek(new ImapPartialRange(1000L, 2000L)).ToString());
      Assert.AreEqual("(BODY[HEADER (subject date)]<1000.2000>)", ImapFetchDataItem.BodyText("HEADER (subject date)", 1000, 2000).ToString());
      Assert.AreEqual("(BODY.PEEK[HEADER (from to)]<0.1000>)", ImapFetchDataItem.BodyPeek("HEADER (from to)", 0, 1000).ToString());
    }

    [Test]
    public void TestBodyInvalidArgument()
    {
      for (var i = 0; i < 6; i++) {
        try {
          switch (i) {
            case 0: ImapFetchDataItem.BodyPeek(new ImapPartialRange(1000L)); break;
            case 1: ImapFetchDataItem.BodyText(new ImapPartialRange(1000L)); break;
            case 2: ImapFetchDataItem.BodyPeek(1000L, 0L); break;
            case 3: ImapFetchDataItem.BodyText(1000L, 0L); break;
            case 4: ImapFetchDataItem.BodyPeek(-1L, 1L); break;
            case 5: ImapFetchDataItem.BodyText(-1L, 1L); break;
          }

          Assert.Fail("ArgumentException not thrown (#{0})", i);
        }
        catch (ArgumentException) {
        }
      }
    }

    [Test]
    public void TestBinary()
    {
      Assert.AreEqual("(BINARY[1.1])", ImapFetchDataItem.Binary("1.1").ToString());
      Assert.AreEqual("(BINARY.PEEK[1.1])", ImapFetchDataItem.BinaryPeek("1.1").ToString());
      Assert.AreEqual("(BINARY[1.1])", ImapFetchDataItem.Binary(false, "1.1").ToString());
      Assert.AreEqual("(BINARY.PEEK[1.1])", ImapFetchDataItem.Binary(true, "1.1").ToString());
      Assert.AreEqual("(BINARY[heADEr])", ImapFetchDataItem.Binary("heADEr").ToString());
      Assert.AreEqual("(BINARY.PEEK[1.1.TEXT]<0.1024>)", ImapFetchDataItem.BinaryPeek("1.1.TEXT", 0L, 1024L).ToString());
      Assert.AreEqual("(BINARY.PEEK[1.1.TEXT]<0.1024>)", ImapFetchDataItem.BinaryPeek("1.1.TEXT", new ImapPartialRange(0L, 1024L)).ToString());
      Assert.AreEqual("(BINARY.PEEK[1.1.TEXT]<1000.2000>)", ImapFetchDataItem.BinaryPeek("1.1.TEXT", 1000L, 2000L).ToString());
      Assert.AreEqual("(BINARY.PEEK[1.1.TEXT]<1000.2000>)", ImapFetchDataItem.BinaryPeek("1.1.TEXT", new ImapPartialRange(1000L, 2000L)).ToString());
    }

    [Test]
    public void TestBinaryInvalidArgument()
    {
      for (var i = 0; i < 6; i++) {
        try {
          switch (i) {
            case 0: ImapFetchDataItem.BinaryPeek("1.1", new ImapPartialRange(1000L)); break;
            case 1: ImapFetchDataItem.BinaryPeek("1.1", new ImapPartialRange(1000L)); break;
            case 2: ImapFetchDataItem.BinaryPeek("1.1", 1000L, 0L); break;
            case 3: ImapFetchDataItem.BinaryPeek("1.1", 1000L, 0L); break;
            case 4: ImapFetchDataItem.BinaryPeek("1.1", -1L, 1L); break;
            case 5: ImapFetchDataItem.BinaryPeek("1.1", -1L, 1L); break;
          }

          Assert.Fail("ArgumentException not thrown (#{0})", i);
        }
        catch (ArgumentException) {
        }
      }
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestBinarySectionNull()
    {
      ImapFetchDataItem.Binary(null);
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestBinarySectionEmpty()
    {
      ImapFetchDataItem.Binary(string.Empty);
    }

    [Test]
    public void TestCombineWithContainsRequiredCapability()
    {
      var dataItem1 = ImapFetchDataItem.ModSeq;
      var dataItem2 = ImapFetchDataItem.InternalDate;

      var combined = dataItem1 + dataItem2;

      Assert.AreEqual("(MODSEQ INTERNALDATE)", combined.ToString());
      Assert.AreEqual(ImapCapability.CondStore, (combined as IImapExtension).RequiredCapability);
    }
  }
}