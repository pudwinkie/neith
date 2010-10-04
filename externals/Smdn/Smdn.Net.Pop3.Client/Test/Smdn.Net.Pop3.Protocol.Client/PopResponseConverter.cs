using System;
using System.IO;
using NUnit.Framework;

namespace Smdn.Net.Pop3.Protocol.Client {
  [TestFixture]
  public class PopResponseConverterTests {
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

    private TResponse GetSingleResponse<TResponse>(string response) where TResponse : PopResponse
    {
      var resp = NetworkTransferEncoding.Transfer8Bit.GetBytes(response);

      baseStream.Seek(0, SeekOrigin.Begin);
      baseStream.Write(resp, 0, resp.Length);
      baseStream.Seek(0, SeekOrigin.Begin);

      var r = receiver.ReceiveResponse();

      Assert.IsInstanceOfType(typeof(TResponse), r, "response type");

      return r as TResponse;
    }

    [Test]
    public void TestFromStat()
    {
      var response = GetSingleResponse<PopStatusResponse>("+OK 2 320\r\n");

      var dropListing = PopResponseConverter.FromStat(response);

      Assert.AreEqual(2L, dropListing.MessageCount);
      Assert.AreEqual(320L, dropListing.SizeInOctets);
    }

    [Test]
    public void TestFromCapa()
    {
      var response = GetSingleResponse<PopFollowingResponse>("SASL CRAM-MD5 KERBEROS_V4\r\n");

      var capa = PopResponseConverter.FromCapa(response);

      Assert.AreEqual("SASL", capa.Tag);
      Assert.IsTrue(capa.ContainsAllArguments("CRAM-MD5", "KERBEROS_V4"));
    }

    [Test]
    public void TestFromListSingleLine()
    {
      var response = GetSingleResponse<PopStatusResponse>("+OK 2 200\r\n");

      var scanListing = PopResponseConverter.FromList(response);

      Assert.AreEqual(2L, scanListing.MessageNumber);
      Assert.AreEqual(200L, scanListing.SizeInOctets);
    }

    [Test]
    public void TestFromListMultiLine()
    {
      var response = GetSingleResponse<PopFollowingResponse>("1 120\r\n");

      var scanListing = PopResponseConverter.FromList(response);

      Assert.AreEqual(1L, scanListing.MessageNumber);
      Assert.AreEqual(120L, scanListing.SizeInOctets);
    }

    [Test]
    public void TestFromUidlSingleLine()
    {
      var response = GetSingleResponse<PopStatusResponse>("+OK 2 QhdPYR:00WBw1Ph7x7\r\n");

      var uidListing = PopResponseConverter.FromUidl(response);

      Assert.AreEqual(2L, uidListing.MessageNumber);
      Assert.AreEqual("QhdPYR:00WBw1Ph7x7", uidListing.UniqueId);
    }

    [Test]
    public void TestFromUidlMultiLine()
    {
      var response = GetSingleResponse<PopFollowingResponse>("1 whqtswO00WBw418f9t5JxYwZ\r\n");

      var uidListing = PopResponseConverter.FromUidl(response);

      Assert.AreEqual(1L, uidListing.MessageNumber);
      Assert.AreEqual("whqtswO00WBw418f9t5JxYwZ", uidListing.UniqueId);
    }

    [Test]
    public void TestFromGreetingBanner()
    {
      var response = GetSingleResponse<PopStatusResponse>("+OK POP3 server ready <1896.697170952@dbc.mtview.ca.us>\r\n");

      var timestamp = PopResponseConverter.FromGreetingBanner(response);

      Assert.AreEqual("<1896.697170952@dbc.mtview.ca.us>", timestamp);

      response = GetSingleResponse<PopStatusResponse>("+OK POP3 server ready\r\n");

      timestamp = PopResponseConverter.FromGreetingBanner(response);

      Assert.IsNull(timestamp);
    }


    private PopResponseReceiver receiver;
    private MemoryStream baseStream;
    private LineOrientedBufferedStream stream;
  }
}
