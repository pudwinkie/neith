using System;
using System.IO;
using NUnit.Framework;

namespace Smdn.Net.Pop3.Protocol.Client {
  [TestFixture]
  public class PopCommandSenderTests {
    [SetUp]
    public void Setup()
    {
      baseStream = new MemoryStream();
      sender = new PopCommandSender(new LineOrientedBufferedStream(baseStream));
    }

    [TearDown]
    public void TearDown()
    {
      // nothing to do
    }

    private string SendCommand(PopCommand command)
    {
      baseStream.Seek(0, SeekOrigin.Begin);

      sender.Send(command);

      baseStream.Seek(0, SeekOrigin.Begin);

      var reader = new StreamReader(baseStream, NetworkTransferEncoding.Transfer8Bit);

      return reader.ReadToEnd();
    }

    [Test]
    public void TestSendCommand()
    {
      Assert.AreEqual("RSET\r\n",
                      SendCommand(new PopCommand("RSET")));
    }

    [Test]
    public void TestSendCommandWithArguments()
    {
      Assert.AreEqual("TOP 1 10\r\n",
                      SendCommand(new PopCommand("TOP", new[] {"1", "10"})));
    }

    [Test]
    public void TestSendContinuation()
    {
      Assert.AreEqual("dGVzdAB0ZXN0AHRlc3Q=\r\n",
                      SendCommand(new PopCommand(null, new[] {"dGVzdAB0ZXN0AHRlc3Q="})));
    }

    private MemoryStream baseStream;
    private PopCommandSender sender;
  }
}
