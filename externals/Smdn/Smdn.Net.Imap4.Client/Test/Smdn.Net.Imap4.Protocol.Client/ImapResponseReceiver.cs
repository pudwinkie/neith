using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using Smdn.Net.Imap4.Protocol;

namespace Smdn.Net.Imap4.Protocol.Client {
  [TestFixture]
  public class ImapResponseReceiverTest {
    [SetUp]
    public void Setup()
    {
      baseStream = new MemoryStream();
      stream = new LineOrientedBufferedStream(baseStream);
      receiver = new ImapResponseReceiver(stream);
    }

    [TearDown]
    public void TearDown()
    {
      // nothing to do
    }

    private void WriteResponse(string response)
    {
      var resp = Encoding.UTF8.GetBytes(response);

      baseStream.Seek(0, SeekOrigin.Begin);
      baseStream.Write(resp, 0, resp.Length);
      baseStream.Seek(0, SeekOrigin.Begin);
    }

    private ImapResponse ReadResponse()
    {
      return ReadResponse(2);
    }

    private ImapResponse ReadResponse(int maxTryReceive)
    {
      ImapResponse resp = null;

      for (var act = 0;; act++) {
        if (maxTryReceive < act)
          Assert.Fail("ReadResponse failed");

        resp = receiver.ReceiveResponse();

        if (resp != null)
          return resp;

        if (!receiver.ResponseContinuing)
          break;
      }

      return resp;
    }

    [Test, Ignore("can't test")]
    public void TestDividedStatusResponse()
    {
      ImapResponse response;

      WriteResponse("* OK IMAP4");
      WriteResponse("rev1 Service Ready");
      WriteResponse("\r\n");

      response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is ImapUntaggedStatusResponse, "response type");

      var status = (response as ImapUntaggedStatusResponse);

      Assert.AreEqual(ImapResponseCondition.Ok, status.Condition);
      Assert.AreEqual("IMAP4rev1 Service Ready", status.ResponseText.Text);
    }

    [Test, Ignore("can't test")]
    public void TestDividedDataResponse()
    {
      ImapResponse response;

      WriteResponse("* SEARCH 2 4 5");
      WriteResponse(" 6 9 10 13");
      WriteResponse(" 15\r\n");

      response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is ImapDataResponse, "response type");

      var dataResponse = response as ImapDataResponse;

      Assert.IsTrue(ImapDataResponseType.Search ==  dataResponse.Type);

      Assert.AreEqual(8, dataResponse.Data.Length);
      //Assert.AreEqual("* SEARCH 2 4 5 6 9 10 13 15\r\n", dataResponse.RawResponse);
      Assert.AreEqual(ImapDataFormat.Text, dataResponse.Data[0].Format);
      Assert.AreEqual("2", dataResponse.Data[0].GetTextAsString());
      Assert.AreEqual(ImapDataFormat.Text, dataResponse.Data[7].Format);
      Assert.AreEqual("15", dataResponse.Data[7].GetTextAsString());
    }

    [Test, Ignore("can't test")]
    public void TestDividedDataResponseAtLastCrLf()
    {
      ImapResponse response;

      WriteResponse("* 12 FETCH (BODY[HEADER] {342}\r\n" +
                                "Date: Wed, 17 Jul 1996 02:23:25 -0700 (PDT)\r\n" + 
                                "From: Terry Gray <gray@cac.washington.edu>\r\n" + 
                                "Subject: IMAP4rev1 WG mtg summary and minutes\r\n" + 
                                "To: imap@cac.washington.edu\r\n" + 
                                "cc: minutes@CNRI.Reston.VA.US, John Klensin <KLENSIN@MIT.EDU>\r\n" + 
                                "Message-Id: <B27397-0100000@cac.washington.edu>\r\n" + 
                                "MIME-Version: 1.0\r\n" + 
                                 "Content-Type: TEXT/PLAIN; CHARSET=US-ASCII\r\n" + 
                                "\r\n" +
                                ")");
      WriteResponse("\r\n" +
                                "* 13 FETCH (BODY[HEADER] {342}\r\n" +
                                "Date: Wed, 17 Jul 1996 02:23:25 -0700 (PDT)\r\n" + 
                                "From: Terry Gray <gray@cac.washington.edu>\r\n" + 
                                "Subject: IMAP4rev1 WG mtg summary and minutes\r\n" + 
                                "To: imap@cac.washington.edu\r\n" + 
                                "cc: minutes@CNRI.Reston.VA.US, John Klensin <KLENSIN@MIT.EDU>\r\n" + 
                                "Message-Id: <B27397-0100000@cac.washington.edu>\r\n" + 
                                 "MIME-Version: 1.0\r\n" + 
                                "Content-Type: TEXT/PLAIN; CHARSET=US-ASCII\r\n" + 
                                "\r\n" +
                                ")\r\n");

      response = ReadResponse();

      Assert.IsTrue(response is ImapDataResponse, "response0 type");

      ImapDataResponse data = response as ImapDataResponse;

      Assert.IsTrue(ImapDataResponseType.Fetch == data.Type);

      Assert.AreEqual(2, data.Data.Length);
      Assert.AreEqual(ImapDataFormat.Text, data.Data[0].Format);
      Assert.AreEqual("12", data.Data[0].GetTextAsString());
      Assert.AreEqual(ImapDataFormat.List, data.Data[1].Format);

      response = ReadResponse();

      Assert.IsTrue(response is ImapDataResponse, "response1 type");

      data = response as ImapDataResponse;

      Assert.IsTrue(ImapDataResponseType.Fetch == data.Type);

      Assert.AreEqual(2, data.Data.Length);
      Assert.AreEqual(ImapDataFormat.Text, data.Data[0].Format);
      Assert.AreEqual("13", data.Data[0].GetTextAsString());
      Assert.AreEqual(ImapDataFormat.List, data.Data[1].Format);
    }

    [Test, Ignore("can't test")]
    public void TestDividedQuotedDataResponse()
    {
      ImapResponse response;

      WriteResponse("* LIST \".");
      WriteResponse("\" \"INB");
      WriteResponse("OX\"\r\n");

      response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is ImapDataResponse, "response type");

      var dataResponse = response as ImapDataResponse;

      Assert.IsTrue(ImapDataResponseType.List ==  dataResponse.Type);

      Assert.AreEqual(ImapDataFormat.Text, dataResponse.Data[0].Format);
      Assert.AreEqual(".", dataResponse.Data[0].GetTextAsString());

      Assert.AreEqual(ImapDataFormat.Text, dataResponse.Data[1].Format);
      Assert.AreEqual("INBOX", dataResponse.Data[1].GetTextAsString());
    }

    [Test, Ignore("can't test")]
    public void TestDividedParenthesizedDataResponse()
    {
      ImapResponse response;

      WriteResponse("* NAMESPACE");
      WriteResponse(" (");
      WriteResponse("(\"INBOX.\" ");
      WriteResponse("\".\")) NIL NIL\r\n");

      response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is ImapDataResponse, "response type");

      var dataResponse = response as ImapDataResponse;

      Assert.IsTrue(ImapDataResponseType.Namespace ==  dataResponse.Type);

      var data = (response as ImapDataResponse).Data;

      Assert.AreEqual(ImapDataFormat.List, data[0].Format);
      Assert.AreEqual(ImapDataFormat.Nil, data[1].Format);
      Assert.AreEqual(ImapDataFormat.Nil, data[2].Format);
    }

    [Test, Ignore("can't test")]
    public void TestDividedLiteralDataResponse()
    {
      ImapResponse response;

      WriteResponse("* 12 FETCH (BODY[HEADER] {342}\r\n" +
                                 "Date: Wed, 17 Jul 1996 02:23:25 -0700 (PDT)\r\n" + 
                                 "From: Terry Gray <gray@cac.washington.edu>\r\n" + 
                                 "Subject: IMAP4rev1 WG");
      WriteResponse(" mtg summary and minutes\r\n" + 
                                 "To: imap@cac.washington.edu\r\n" + 
                                 "cc: minutes@CNRI.Reston.VA.US, John Klensin <KLENSIN@MIT.EDU>\r\n" + 
                                 "Message-Id: <B27397-0100000@cac.washington.edu>\r\n" + 
                                 "MIME-Version: 1.0\r");
      WriteResponse("\nContent-Type: TEXT/PLAIN; CHARSET=US-ASCII\r\n" + 
                                 "\r\n");
      WriteResponse(")\r\n");

      response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is ImapDataResponse, "response type");

      var dataResponse = response as ImapDataResponse;

      Assert.IsTrue(ImapDataResponseType.Fetch ==  dataResponse.Type);

      Assert.AreEqual(2, dataResponse.Data.Length);
      Assert.AreEqual(2, dataResponse.Data[1].List.Length);
      Assert.AreEqual(ImapDataFormat.Text, dataResponse.Data[1].List[0].Format);
      Assert.AreEqual(ImapDataFormat.Text, dataResponse.Data[1].List[1].Format);
      Assert.AreEqual(342, dataResponse.Data[1].List[1].GetTextLength());
    }

    [Test]
    public void TestDataResponseLiteralString()
    {
      WriteResponse("* 12 FETCH (BODY[HEADER] {342}\r\n" +
                                 "Date: Wed, 17 Jul 1996 02:23:25 -0700 (PDT)\r\n" + 
                                 "From: Terry Gray <gray@cac.washington.edu>\r\n" + 
                                 "Subject: IMAP4rev1 WG mtg summary and minutes\r\n" + 
                                 "To: imap@cac.washington.edu\r\n" + 
                                 "cc: minutes@CNRI.Reston.VA.US, John Klensin <KLENSIN@MIT.EDU>\r\n" + 
                                 "Message-Id: <B27397-0100000@cac.washington.edu>\r\n" + 
                                 "MIME-Version: 1.0\r\n" + 
                                 "Content-Type: TEXT/PLAIN; CHARSET=US-ASCII\r\n" + 
                                 "\r\n" +
                                 ")\r\n");

      var response = ReadResponse(11);

      Assert.IsNotNull(response);
      Assert.IsTrue(response is ImapDataResponse, "response type");

      var dataResponse = (response as ImapDataResponse);

      Assert.IsTrue(ImapDataResponseType.Fetch == dataResponse.Type);

      Assert.AreEqual(2, dataResponse.Data.Length);
      Assert.AreEqual(ImapDataFormat.Text, dataResponse.Data[0].Format);
      Assert.AreEqual(ImapDataFormat.List, dataResponse.Data[1].Format);

      Assert.AreEqual("12", dataResponse.Data[0].GetTextAsString());

      Assert.AreEqual(2, dataResponse.Data[1].List.Length);
      Assert.AreEqual(ImapDataFormat.Text, dataResponse.Data[1].List[0].Format);
      Assert.AreEqual(ImapDataFormat.Text, dataResponse.Data[1].List[1].Format);
      Assert.AreEqual(342, dataResponse.Data[1].List[1].GetTextLength());
    }

    [Test]
    public void TestDataResponseLiteral8String()
    {
      WriteResponse("* 12 FETCH (BINARY[1.1] ~{32}\r\n" +
                    "\x00\x01\x02\x03\x04\x05\x06\x07\x08\x09\x0a\x0b\x0c\x0d\x0e\x0f" +
                    "\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f" +
                    ")\r\n");

      var response = ReadResponse(1);

      Assert.IsNotNull(response);
      Assert.IsTrue(response is ImapDataResponse, "response type");

      var dataResponse = (response as ImapDataResponse);

      Assert.IsTrue(ImapDataResponseType.Fetch == dataResponse.Type);

      Assert.AreEqual(2, dataResponse.Data.Length);
      Assert.AreEqual(ImapDataFormat.Text, dataResponse.Data[0].Format);
      Assert.AreEqual(ImapDataFormat.List, dataResponse.Data[1].Format);

      Assert.AreEqual("12", dataResponse.Data[0].GetTextAsString());

      Assert.AreEqual(2, dataResponse.Data[1].List.Length);
      Assert.AreEqual(ImapDataFormat.Text, dataResponse.Data[1].List[0].Format);
      Assert.AreEqual(ImapDataFormat.Text, dataResponse.Data[1].List[1].Format);
      Assert.AreEqual(32, dataResponse.Data[1].List[1].GetTextLength());
    }

    [Test]
    public void TestDataResponseMailboxDataWhichContainsNoSp()
    {
      WriteResponse("* SEARCH\r\n0006 OK Search completed.\r\n");

      var response = ReadResponse();

      Assert.IsTrue(response is ImapDataResponse, "responses[0] type");

      var dataResponse = response as ImapDataResponse;

      Assert.IsTrue(ImapDataResponseType.Search == dataResponse.Type);

      var data = dataResponse.Data;

      Assert.AreEqual(0, data.Length);


      response = ReadResponse();

      Assert.IsTrue(response is ImapTaggedStatusResponse, "responses[1] type");

      var status = response as ImapTaggedStatusResponse;

      Assert.AreEqual(ImapResponseCondition.Ok, status.Condition);
      Assert.AreEqual("Search completed.", status.ResponseText.Text);
    }

    [Test]
    public void TestDataResponseContainsSpecialCharacters()
    {
      // http://tools.ietf.org/html/rfc2683
      // RFC 2683 - IMAP4 Implementation Recommendations
      // 3.4.2. Special Characters

      WriteResponse("* LIST () \"\" INBOX\r\n" + 
                    "* LIST () \"\\\\\" TEST\r\n" + 
                    "* LIST () \"\\\\\" {12}\r\n" + 
                    "\"My\" mailbox\r\n" + 
                    "* LIST () \"\\\\\" {17}\r\n" + 
                    "\"My\" mailbox\\Junk\r\n");

      ImapResponse response;
      ImapMailboxList list;

      // 1
      response = ReadResponse();

      Assert.IsInstanceOfType(typeof(ImapDataResponse), response, "response type #1");

      list = ImapDataResponseConverter.FromList(response as ImapDataResponse);

      Assert.AreEqual(string.Empty, list.HierarchyDelimiter, "LIST response #1 delimiter");
      Assert.AreEqual("INBOX", list.Name, "LIST response #1 name");

      // 2
      response = ReadResponse();

      Assert.IsInstanceOfType(typeof(ImapDataResponse), response, "response type #2");

      list = ImapDataResponseConverter.FromList(response as ImapDataResponse);

      Assert.AreEqual("\\", list.HierarchyDelimiter, "LIST response #2 delimiter");
      Assert.AreEqual("TEST", list.Name, "LIST response #2 name");

      // 3
      response = ReadResponse();

      Assert.IsInstanceOfType(typeof(ImapDataResponse), response, "response type #3");

      list = ImapDataResponseConverter.FromList(response as ImapDataResponse);

      Assert.AreEqual("\\", list.HierarchyDelimiter, "LIST response #3 delimiter");
      Assert.AreEqual(@"""My"" mailbox", list.Name, "LIST response #3 name");

      // 4
      response = ReadResponse();

      Assert.IsInstanceOfType(typeof(ImapDataResponse), response, "response type #4");

      list = ImapDataResponseConverter.FromList(response as ImapDataResponse);

      Assert.AreEqual("\\", list.HierarchyDelimiter, "LIST response #4 delimiter");
      Assert.AreEqual(@"""My"" mailbox\Junk", list.Name, "LIST response #4 name");
    }

    [Test]
    public void TestStatusOkResponse()
    {
      WriteResponse("* OK IMAP4rev1 server ready\r\n");

      var response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is ImapUntaggedStatusResponse, "response type");

      var status = response as ImapUntaggedStatusResponse;

      Assert.AreEqual(ImapResponseCondition.Ok, status.Condition);
      Assert.AreEqual("IMAP4rev1 server ready", status.ResponseText.Text);
      Assert.IsNull(status.ResponseText.Code);
    }

    [Test]
    public void TestStatusNoResponse()
    {
      WriteResponse("A223 NO COPY failed: disk is full\r\n");

      var response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is ImapTaggedStatusResponse, "response type");

      var status = response as ImapTaggedStatusResponse;

      Assert.AreEqual(ImapResponseCondition.No, status.Condition);
      Assert.AreEqual("COPY failed: disk is full", status.ResponseText.Text);
      Assert.AreEqual("A223", status.Tag);
      Assert.IsNull(status.ResponseText.Code);
    }

    [Test]
    public void TestStatusBadResponse()
    {
      WriteResponse("* BAD Disk crash, attempting salvage to a new disk!\r\n");

      var response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is ImapUntaggedStatusResponse, "response type");

      var status = response as ImapUntaggedStatusResponse;

      Assert.AreEqual(ImapResponseCondition.Bad, status.Condition);
      Assert.AreEqual("Disk crash, attempting salvage to a new disk!", status.ResponseText.Text);
      Assert.IsNull(status.ResponseText.Code);
    }

    [Test]
    public void TestStatusPreAuthResponse()
    {
      WriteResponse("* PREAUTH IMAP4rev1 server logged in as Smith\r\n");

      var response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is ImapUntaggedStatusResponse, "response type");

      var status = response as ImapUntaggedStatusResponse;

      Assert.AreEqual(ImapResponseCondition.PreAuth, status.Condition);
      Assert.AreEqual("IMAP4rev1 server logged in as Smith", status.ResponseText.Text);
      Assert.IsNull(status.ResponseText.Code);
    }

    [Test]
    public void TestStatusByeResponse()
    {
      WriteResponse("* BYE Autologout; idle for too long\r\n");

      var response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is ImapUntaggedStatusResponse, "response type");

      var status = response as ImapUntaggedStatusResponse;

      Assert.AreEqual(ImapResponseCondition.Bye, status.Condition);
      Assert.AreEqual("Autologout; idle for too long", status.ResponseText.Text);
      Assert.IsNull(status.ResponseText.Code);
    }

    [Test]
    public void TestStatusResponseWithStringDataStatusCode()
    {
      WriteResponse("* OK [UNSEEN 17] Message 17 is the first unseen message\r\n");

      var response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is ImapUntaggedStatusResponse, "response type");

      var status = response as ImapUntaggedStatusResponse;

      Assert.AreEqual(ImapResponseCondition.Ok, status.Condition);
      Assert.AreEqual("Message 17 is the first unseen message", status.ResponseText.Text);

      // response status code
      Assert.IsNotNull(status.ResponseText.Code);
      Assert.IsTrue(status.ResponseText.Code == ImapResponseCode.Unseen);
      Assert.AreEqual(1, status.ResponseText.Arguments.Length);
      Assert.AreEqual(ImapDataFormat.Text, status.ResponseText.Arguments[0].Format);
      Assert.AreEqual("17", status.ResponseText.Arguments[0].GetTextAsString());
    }

    [Test]
    public void TestStatusResponseWithParenthesizedDataStatusCode()
    {
      WriteResponse("* OK [PERMANENTFLAGS (\\Deleted \\Seen \\*)] Limited\r\n");

      var response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is ImapUntaggedStatusResponse, "response type");

      var status = response as ImapUntaggedStatusResponse;

      Assert.AreEqual(ImapResponseCondition.Ok, status.Condition);

      // response status code
      Assert.IsNotNull(status.ResponseText.Code);
      Assert.IsTrue(status.ResponseText.Code == ImapResponseCode.PermanentFlags);
      Assert.AreEqual(1, status.ResponseText.Arguments.Length);
      Assert.AreEqual(ImapDataFormat.List, status.ResponseText.Arguments[0].Format);
      Assert.AreEqual(3, status.ResponseText.Arguments[0].List.Length);
      Assert.AreEqual("\\Deleted",  status.ResponseText.Arguments[0].List[0].GetTextAsString());
      Assert.AreEqual("\\Seen",     status.ResponseText.Arguments[0].List[1].GetTextAsString());
      Assert.AreEqual("\\*",        status.ResponseText.Arguments[0].List[2].GetTextAsString());
    }

    [Test]
    public void TestCommandContinuationRequest()
    {
      WriteResponse("+ Ready for additional command text\r\n");

      var response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is ImapCommandContinuationRequest, "response type");

      var continuation = response as ImapCommandContinuationRequest;

      Assert.AreEqual("Ready for additional command text", continuation.Text);
    }

    [Test]
    public void TestReceiveResponseAsUTF8()
    {
      receiver.ReceiveResponseAsUTF8 = true;

      WriteResponse("A001 OK LANGUAGE-Befehl ausgeführt\r\n");

      var response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.AreEqual("LANGUAGE-Befehl ausgeführt", (response as ImapTaggedStatusResponse).ResponseText.Text);
    }

    private ImapResponseReceiver receiver;
    private MemoryStream baseStream;
    private LineOrientedBufferedStream stream;
  }
}