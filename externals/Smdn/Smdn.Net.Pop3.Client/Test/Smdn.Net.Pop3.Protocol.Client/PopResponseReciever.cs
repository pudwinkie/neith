using System;
using System.IO;
using NUnit.Framework;

namespace Smdn.Net.Pop3.Protocol.Client {
  [TestFixture]
  public class PopResponseRecieverTests {
    [SetUp]
    public void Setup()
    {
      baseStream = new MemoryStream();
      stream = new LineOrientedBufferedStream(baseStream);
      receiver = new PopResponseReceiver(stream);
    }

    [TearDown]
    public void TearDown()
    {
      // nothing to do
    }

    private void WriteResponse(string response)
    {
      var resp = NetworkTransferEncoding.Transfer8Bit.GetBytes(response);

      baseStream.Seek(0, SeekOrigin.Begin);
      baseStream.Write(resp, 0, resp.Length);
      baseStream.Seek(0, SeekOrigin.Begin);
    }

    private PopResponse ReadResponse()
    {
      return receiver.ReceiveResponse();
    }

    [Test]
    public void TestStatusOkResponse()
    {
      WriteResponse("+OK\r\n");

      var response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is PopStatusResponse, "response type");

      var status = response as PopStatusResponse;

      Assert.AreEqual(PopStatusIndicator.Positive, status.Status);
      Assert.IsNull(status.ResponseText.Code);
    }

    [Test]
    public void TestStatusOkResponseWithText()
    {
      WriteResponse("+OK POP3 server ready <1896.697170952@dbc.mtview.ca.us>\r\n");

      var response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is PopStatusResponse, "response type");

      var status = response as PopStatusResponse;

      Assert.AreEqual(PopStatusIndicator.Positive, status.Status);
      Assert.AreEqual("POP3 server ready <1896.697170952@dbc.mtview.ca.us>", status.Text);
      Assert.IsNull(status.ResponseText.Code);
    }

    [Test]
    public void TestStatusNoResponse()
    {
      WriteResponse("-ERR\r\n");

      var response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is PopStatusResponse, "response type");

      var status = response as PopStatusResponse;

      Assert.AreEqual(PopStatusIndicator.Negative, status.Status);
      Assert.IsNull(status.ResponseText.Code);
    }

    [Test]
    public void TestStatusNoResponseWithText()
    {
      WriteResponse("-ERR sorry, no mailbox for frated here\r\n");

      var response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is PopStatusResponse, "response type");

      var status = response as PopStatusResponse;

      Assert.AreEqual(PopStatusIndicator.Negative, status.Status);
      Assert.IsNull(status.ResponseText.Code);
      Assert.AreEqual("sorry, no mailbox for frated here", status.Text);
    }

    [Test]
    public void TestStatusNoResponseWithResponseCode()
    {
      WriteResponse("-ERR [IN-USE] Do you have another POP session running?\r\n");

      var response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is PopStatusResponse, "response type");

      var status = response as PopStatusResponse;

      Assert.AreEqual(PopStatusIndicator.Negative, status.Status);
      Assert.IsNotNull(status.ResponseText.Code);
      Assert.AreEqual(PopResponseCode.InUse, status.ResponseText.Code);
      Assert.AreEqual("Do you have another POP session running?", status.Text);
    }

    [Test]
    public void TestStatusNoResponseWithResponseCodeOnly()
    {
      WriteResponse("-ERR [IN-USE]\r\n");

      var response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is PopStatusResponse, "response type");

      var status = response as PopStatusResponse;

      Assert.AreEqual(PopStatusIndicator.Negative, status.Status);
      Assert.IsNotNull(status.ResponseText.Code);
      Assert.AreEqual(PopResponseCode.InUse, status.ResponseText.Code);
      Assert.IsEmpty(status.Text);
    }

    [Test]
    public void TestStatusResponseWithResponseCodeAndTextWithoutSpace()
    {
      WriteResponse("-ERR [IN-USE]text\r\n");

      var response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is PopStatusResponse, "response type");

      var status = response as PopStatusResponse;

      Assert.AreEqual(PopStatusIndicator.Negative, status.Status);
      Assert.IsNotNull(status.ResponseText.Code);
      Assert.AreEqual(PopResponseCode.InUse, status.ResponseText.Code);
      Assert.AreEqual("text", status.Text);
    }

    [Test]
    public void TestCommandContinuationRequest()
    {
      WriteResponse("+\r\n");

      var response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is PopContinuationRequest, "response type");
    }

    [Test]
    public void TestCommandContinuationRequestWithBase64Text()
    {
      WriteResponse("+ TlRMTVNTUAACAAAAAAAAAAAAAAABAoAAfvE6HR6OGggAAAAAAAAAABQAFAAwAAAAAwAMAGgAYQB5AGEAdABlAAAAAAA=\r\n");

      var response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is PopContinuationRequest, "response type");

      var continuation = response as PopContinuationRequest;

      Assert.AreEqual("TlRMTVNTUAACAAAAAAAAAAAAAAABAoAAfvE6HR6OGggAAAAAAAAAABQAFAAwAAAAAwAMAGgAYQB5AGEAdABlAAAAAAA=", continuation.Base64Text.ToString());
    }

    [Test]
    public void TestTerminationResponse()
    {
      WriteResponse(".\r\n");

      var response = ReadResponse();

      Assert.IsNotNull(response);
      Assert.IsTrue(response is PopTerminationResponse, "response type");
    }

    [Test]
    public void TestHandleAsMultiline()
    {
      var lines = new[] {
        "+OK 134 octets",
        "MIME-Version: 1.0",
        "Content-Type: text/plain",
        "",
        "...byte-stuffed",
        "+OK response style line",
        "-ERR response style line2",
        "+ continuation request style line",
        "+",
        "..",
        "end of message",
        ".",
        string.Empty,
      };

      WriteResponse(string.Join("\r\n", lines));

      receiver.HandleAsMultiline = false;

      var expectedResponseTypes = new[] {
        typeof(PopStatusResponse),
        typeof(PopFollowingResponse),
        typeof(PopFollowingResponse),
        typeof(PopFollowingResponse),
        typeof(PopFollowingResponse),
        typeof(PopFollowingResponse),
        typeof(PopFollowingResponse),
        typeof(PopFollowingResponse),
        typeof(PopFollowingResponse),
        typeof(PopFollowingResponse),
        typeof(PopFollowingResponse),
        typeof(PopTerminationResponse),
      };

      for (var i = 0; i < expectedResponseTypes.Length; i++) {
        var response = ReadResponse();

        Assert.IsNotNull(response);
        Assert.IsInstanceOfType(expectedResponseTypes[i], response, "response #{0}", i);

        if (response is PopFollowingResponse) {
          var expected = lines[i];

          if (expected.StartsWith("."))
            expected = expected.Substring(1);

          Assert.AreEqual((response as PopFollowingResponse).Text.ToString(), expected);
        }

        if (i == 0)
          receiver.HandleAsMultiline = true;
      }
    }

    [Test]
    public void TestReceiveLine()
    {
      var lines = new[] {
        "+OK 134 octets",
        "MIME-Version: 1.0",
        "Content-Type: text/plain",
        "",
        "...byte-stuffed",
        "+OK response style line",
        "-ERR response style line2",
        "+ continuation request style line",
        "+",
        "..",
        "end of message",
        ".",
        string.Empty,
      };

      WriteResponse(string.Join("\r\n", lines));

      for (var i = 0; i < lines.Length; i++) {
        var expected = lines[i] + "\r\n";

        if (expected.StartsWith("."))
          expected = expected.Substring(1);

        var lineStream = new MemoryStream();
        var ret = receiver.ReceiveLine(lineStream);

        lineStream.Close();

        var line = lineStream.ToArray();

        if (i == lines.Length - 2) {
          Assert.IsFalse(ret);
          break;
        }
        else {
          Assert.IsTrue(ret);
          Assert.AreEqual(expected, NetworkTransferEncoding.Transfer7Bit.GetString(line));
        }
      }
    }

    private PopResponseReceiver receiver;
    private MemoryStream baseStream;
    private LineOrientedBufferedStream stream;
  }
}