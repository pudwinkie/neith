using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapDateTimeFormatTest {
    [Test]
    public void TestToDateString()
    {
      Assert.AreEqual("1-Feb-1994", ImapDateTimeFormat.ToDateString(new DateTime(1994, 2, 1)));
    }

    [Test]
    public void TestToDateTimeString()
    {
      Assert.AreEqual("\"17-Jul-1996 02:44:25 -0700\"", ImapDateTimeFormat.ToDateTimeString(new DateTimeOffset(1996, 7, 17, 2, 44, 25, TimeSpan.FromHours(-7))));
    }

    [Test]
    public void TestFromDateTimeString()
    {
      Assert.AreEqual(new DateTimeOffset(1996, 7, 17, 2, 44, 25, TimeSpan.FromHours(-7)), ImapDateTimeFormat.FromDateTimeString("17-Jul-1996 02:44:25 -0700"));
    }

    [Test]
    public void TestFromDateTimeStringStartsWithSpace()
    {
      Assert.AreEqual(new DateTimeOffset(2010, 3, 9, 0, 58, 29, TimeSpan.FromHours(-5)),
                      ImapDateTimeFormat.FromDateTimeString(" 9-Mar-2010 00:58:29 -0500"));
    }

    [Test, ExpectedException(typeof(Smdn.Net.Imap4.Protocol.ImapFormatException))]
    public void TestFromDateTimeStringInvalidFormat()
    {
      ImapDateTimeFormat.FromDateTimeString("invalid date and time");
    }
  }
}