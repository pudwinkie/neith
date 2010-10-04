using System;
using System.Text;
using NUnit.Framework;

namespace Smdn.Formats.Mime {
  [TestFixture]
  public class ParserTest {
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

    [Test/*, ExpectedException(typeof(NotSupportedException))*/]
    public void TestUnsupportedMimeVersion()
    {
      MimeMessage.LoadMessage(@"MIME-Version: 1.1
Content-Type: text/plain; charset=""iso-2022-jp""
Content-Transfer-Encoding: quoted-printable
");
    }

    [Test]
    public void TestMimeVersionNotExist()
    {
      var mime = MimeMessage.LoadMessage(@"Content-Type: text/plain
From: from@example.com
To: to@example.com");

      Assert.AreEqual(MimeType.TextPlain, mime.MimeType);
      Assert.IsTrue(mime.Headers.Contains("Content-Type"));
      Assert.AreEqual("text/plain", mime.Headers["Content-Type"].Value);
      Assert.IsTrue(mime.Headers.Contains("From"));
      Assert.AreEqual("from@example.com", mime.Headers["From"].Value);
      Assert.IsTrue(mime.Headers.Contains("To"));
      Assert.AreEqual("to@example.com", mime.Headers["To"].Value);
    }

    [Test/*, ExpectedException(typeof(NotSupportedException))*/]
    public void TestMimeVersionNotSupported()
    {
      MimeMessage.LoadMessage(@"MIME-Version: 1.1\r\n");
    }

    [Test]
    // TODO: [ExpectedException(typeof(NotSupportedException))]
    public void TestMimeVersionInvalid()
    {
      MimeMessage.LoadMessage("MIME-Version: \"1.0 (comment)\"\r\n");
    }

    [Test]
    public void TestMimeVersionWithComment()
    {
      MimeMessage.LoadMessage("MIME-Version: 1.0 (comment)\r\n");
    }

    [Test]
    public void TestMimeVersionQuoted()
    {
      MimeMessage.LoadMessage("MIME-Version: \"1.0\"\r\n");
    }

    [Test]
    public void TestIgnoreInvalidHeader()
    {
      var mime = MimeMessage.LoadMessage(@"MIME-Version 1.1
MIME-Version: 1.0");

      Assert.IsTrue(mime.Headers.Contains("MIME-Version"));
      Assert.AreEqual("1.0", mime.Headers["MIME-Version"].Value);
    }

    [Test]
    public void TestHeaderDelimiting()
    {
      var message =
        "Content-Type\t\t\t:\t\t\ttext/plain\r\n" +
        "From:       from@example.com\r\n" + 
        "To:to@example.com\r\n" +
        "Subject\t  : \tsubject\r\n";

      var mime = MimeMessage.LoadMessage(message);

      Assert.AreEqual(MimeType.TextPlain, mime.MimeType);
      Assert.IsTrue(mime.Headers.Contains("Content-Type"));
      Assert.AreEqual("text/plain", mime.Headers["Content-Type"].Value);
      Assert.IsTrue(mime.Headers.Contains("From"));
      Assert.AreEqual("from@example.com", mime.Headers["From"].Value);
      Assert.IsTrue(mime.Headers.Contains("To"));
      Assert.AreEqual("to@example.com", mime.Headers["To"].Value);
      Assert.IsTrue(mime.Headers.Contains("Subject"));
      Assert.AreEqual("subject", mime.Headers["Subject"].Value);
    }

    [Test]
    public void TestCaseInvaliantComparison()
    {
      var message =
        "content-type: text/plain\r\n" +
        "FROM: from@example.com\r\n" + 
        "tO: to@example.com\r\n";

      var mime = MimeMessage.LoadMessage(message);

      Assert.AreEqual(MimeType.TextPlain, mime.MimeType);
      Assert.IsTrue(mime.Headers.Contains("CONTENT-TYPE"));
      Assert.AreEqual("text/plain", mime.Headers["Content-Type"].Value);
      Assert.IsTrue(mime.Headers.Contains("from"));
      Assert.AreEqual("from@example.com", mime.Headers["FROM"].Value);
      Assert.IsTrue(mime.Headers.Contains("to"));
      Assert.AreEqual("to@example.com", mime.Headers["TO"].Value);
    }

    [Test]
    public void TestMultilineHeader()
    {
      var message = 
        "MIME-Version: 1.0\r\n" +
        "Subject: line1\r\n" +
        " line2\r\n" +
        "\tline3\r\n" +
        "   \tline4\r\n";

      var mime = MimeMessage.LoadMessage(message);

      Assert.IsTrue(mime.Headers.Contains("Subject"));
      Assert.AreEqual("line1 line2 line3 line4", mime.Headers["Subject"].Value);
    }

    [Test]
    public void TestSinglePartTextBody()
    {
      var mime = MimeMessage.LoadMessage(@"Content-Type: text/plain
Subject: single part

single-part text body");

      Assert.AreEqual(MimeType.TextPlain, mime.MimeType);
      Assert.IsTrue(mime.Headers.Contains("Content-Type"));
      Assert.AreEqual("text/plain", mime.Headers["Content-Type"].Value);
      Assert.IsTrue(mime.Headers.Contains("Subject"));
      Assert.AreEqual("single part", mime.Headers["Subject"].Value);
      Assert.AreEqual("single-part text body", mime.ReadContentAsText());
    }

    [Test]
    public void TestMultiPartTextBody()
    {
      var mime = MimeMessage.LoadMessage(@"Content-Type: multipart/alternative;
  boundary=""==boundary==""
Subject: multi part

multipart message body

--==boundary==
Content-Type: text/plain

first part, text/plain

--==boundary==
Content-Type: text/html

<html><body><p>second part, text/html</p></body></html>
--==boundary==--");

      Assert.AreEqual(MimeType.MultipartAlternative, mime.MimeType);
      Assert.AreEqual("==boundary==", mime.Boundary);
      Assert.IsTrue(mime.Headers.Contains("Content-Type"));
      Assert.AreEqual("multipart/alternative; boundary=\"==boundary==\"",
                      mime.Headers["Content-Type"].Value);
      Assert.IsTrue(mime.Headers.Contains("Subject"));
      Assert.AreEqual("multi part", mime.Headers["Subject"].Value);
      Assert.AreEqual("multipart message body\n", mime.ReadContentAsText().Replace("\r\n", "\n"));

      Assert.AreEqual(2, mime.SubParts.Count);

      Assert.AreEqual(MimeType.TextPlain, mime.SubParts[0].MimeType);
      Assert.IsTrue(mime.SubParts[0].Headers.Contains("Content-Type"));
      Assert.AreEqual("text/plain", mime.SubParts[0].Headers["Content-Type"].Value);
      Assert.AreEqual("first part, text/plain\n", mime.SubParts[0].ReadContentAsText().Replace("\r\n", "\n"));

      Assert.AreEqual(new MimeType("text", "html"), mime.SubParts[1].MimeType);
      Assert.IsTrue(mime.SubParts[1].Headers.Contains("Content-Type"));
      Assert.AreEqual("text/html", mime.SubParts[1].Headers["Content-Type"].Value);
      Assert.AreEqual("<html><body><p>second part, text/html</p></body></html>",
                      mime.SubParts[1].ReadContentAsText());
    }

    [Test]
    public void TestMultiPartWithNoHeader()
    {
      var mime = MimeMessage.LoadMessage(@"Return-Path: <>
Date: Fri, 21 Dec 2007 07:04:35 +0900
MIME-Version: 1.0
Content-Type: multipart/report; report-type=delivery-status;
  boundary=""lBKM1GFo005500.1198188275/xxx.xxxxxx.xxx""
Subject: Warning: could not send message for past 4 hours
Auto-Submitted: auto-generated (warning-timeout)

This is a MIME-encapsulated message

--lBKM1GFo005500.1198188275/xxx.xxxxxx.xxx

    **********************************************
    **      THIS IS A WARNING MESSAGE ONLY      **
    **  YOU DO NOT NEED TO RESEND YOUR MESSAGE  **
    **********************************************

The original message was received at Fri, 21 Dec 2007 02:06:24 +0900
from xxx.xxxxxx.xxx [127.0.0.1]

   ----- Transcript of session follows -----
451 xxx.xxxxxx.xxx: Name server timeout
Warning: message still undelivered after 4 hours
Will keep trying until message is 5 days old
451 xxx.xxxxxx.xxx: Name server timeout

--lBKM1GFo005500.1198188275/xxx.xxxxxx.xxx
Content-Type: message/delivery-status

Reporting-MTA: dns; xxx.xxxxxx.xxx
Arrival-Date: Fri, 21 Dec 2007 02:06:24 +0900

Final-Recipient: RFC822; root@xxx.xxxxxx.xxx
X-Actual-Recipient: RFC822; xxxxxx@xxx.xxxxxx.xxx
Action: delayed
Status: 4.4.3
Last-Attempt-Date: Fri, 21 Dec 2007 07:04:35 +0900
Will-Retry-Until: Wed, 26 Dec 2007 02:06:24 +0900

--lBKM1GFo005500.1198188275/xxx.xxxxxx.xxx
Content-Type: message/rfc822

Return-Path: <root@xxx.xxxxxx.xxx>
Date: Fri, 21 Dec 2007 02:02:15 +0900
Content-Type: text/plain; charset=UTF-8
Auto-Submitted: auto-generated
X-Cron-Env: <MAILTO=root,postmaster,webmaster,clamav>
X-Cron-Env: <SHELL=/bin/sh>
X-Cron-Env: <HOME=/root>
X-Cron-Env: <PATH=/usr/bin:/bin>
X-Cron-Env: <LOGNAME=root>
X-Cron-Env: <USER=root>

ERROR: Can't query current.cvd.clamav.net
ERROR: Can't get information about database.clamav.net: Temporary DNS error
ERROR: No servers could be reached. Giving up
ERROR: Can't query current.cvd.clamav.net
ERROR: Can't get information about database.clamav.net: Temporary DNS error
ERROR: No servers could be reached. Giving up
ERROR: Can't query current.cvd.clamav.net
ERROR: Can't get information about database.clamav.net: Temporary DNS error
ERROR: No servers could be reached. Giving up
ERROR: Update failed. Your network may be down or none of the mirrors listed in freshclam.conf is working.

--lBKM1GFo005500.1198188275/xxx.xxxxxx.xxx--
");

      Assert.AreEqual(new MimeType("multipart", "report"), mime.MimeType);
      Assert.AreEqual("lBKM1GFo005500.1198188275/xxx.xxxxxx.xxx", mime.Boundary);
      Assert.IsTrue(mime.Headers.Contains("Content-Type"));
      Assert.AreEqual("multipart/report; report-type=delivery-status; boundary=\"lBKM1GFo005500.1198188275/xxx.xxxxxx.xxx\"",
                      mime.Headers["Content-Type"].Value);

      Assert.AreEqual(3, mime.SubParts.Count);

      Assert.AreEqual(0, mime.SubParts[0].Headers.Count);
      Assert.IsTrue(mime.SubParts[0].ReadContentAsText().StartsWith("    ***"));

      Assert.AreEqual(new MimeType("message", "delivery-status"), mime.SubParts[1].MimeType);
      Assert.AreEqual("message/delivery-status", mime.SubParts[1].Headers["Content-Type"].Value);
      Assert.AreEqual(new MimeType("message", "rfc822"), mime.SubParts[2].MimeType);
      Assert.AreEqual("message/rfc822", mime.SubParts[2].Headers["Content-Type"].Value);
    }
  }
}