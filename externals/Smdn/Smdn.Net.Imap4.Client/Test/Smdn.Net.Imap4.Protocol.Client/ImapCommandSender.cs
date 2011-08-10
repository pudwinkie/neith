using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

namespace Smdn.Net.Imap4.Protocol.Client {
  [TestFixture]
  public class ImapCommandSenderTest {
    [SetUp]
    public void Setup()
    {
      baseStream = new MemoryStream();
      sender = new ImapCommandSender(new LineOrientedBufferedStream(baseStream));
    }

    [TearDown]
    public void TearDown()
    {
      // nothing to do
    }

    private string SendCommand(ImapCommand command)
    {
      baseStream.Seek(0, SeekOrigin.Begin);

      sender.Send(command);

      while (sender.CommandContinuing) {
        sender.Send();
      }

      baseStream.Seek(0, SeekOrigin.Begin);

      var reader = new StreamReader(baseStream, NetworkTransferEncoding.Transfer8Bit);

      return reader.ReadToEnd();
    }

    [Test]
    public void TestSendCommand()
    {
      var command = new ImapCommand("login", "0001", "username", "password");

      Assert.AreEqual("0001 login username password\r\n",
                      SendCommand(command));
    }

    [Test]
    public void TestSendCommandNoArguments()
    {
      var command = new ImapCommand("NOOP", "0001");

      Assert.AreEqual("0001 NOOP\r\n",
                      SendCommand(command));
    }

    [Test]
    public void TestSendContinuationRequest()
    {
      var command = new ImapCommand(null, "ignored", "xxxxxxxx");

      Assert.AreEqual("xxxxxxxx\r\n",
                      SendCommand(command));
    }

    [Test]
    public void TestSendCommandContainsSynchronizingLiteral()
    {
      var command = new ImapCommand("SEARCH", "0001", "FROM", new ImapLiteralString("from", Encoding.ASCII, ImapLiteralOptions.Synchronizing));

      Assert.AreEqual("0001 SEARCH FROM {4}\r\nfrom\r\n",
                      SendCommand(command));
    }

    [Test]
    public void TestSendCommandContainsNonSynchronizingLiteral()
    {
      var command = new ImapCommand("SEARCH", "0001", "FROM", new ImapLiteralString("from", Encoding.ASCII, ImapLiteralOptions.NonSynchronizing));

      Assert.AreEqual("0001 SEARCH FROM {4+}\r\nfrom\r\n",
                      SendCommand(command));
    }

    private MemoryStream baseStream;
    private ImapCommandSender sender;
  }
}
