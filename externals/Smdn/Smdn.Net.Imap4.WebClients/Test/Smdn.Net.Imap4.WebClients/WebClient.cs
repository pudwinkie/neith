using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using NUnit.Framework;

using Smdn.IO;

namespace Smdn.Net.Imap4.WebClients {
  [TestFixture]
  public class WebClientTests {
    private static readonly string message = @"MIME-Version: 1.0
From: from
To: to
Subject: subject
Date: Sat, 16 Jan 2010 00:41:20 +0900

1st line
2nd line
3rd line
".Replace("\r\n", "\n").Replace("\n", "\r\n");

    [SetUp]
    public void Setup()
    {
      ImapWebRequestCreator.RegisterPrefix();
    }

    private ImapPseudoServer InitializeFetchServer()
    {
      var server = new ImapPseudoServer();

      server.Start();

      // greeting
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1] ready\r\n");
      // LOGIN
      server.EnqueueResponse("0000 OK done\r\n");
      // CAPABILITY
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1]\r\n" +
                             "0001 OK done\r\n");
      // SELECT
      server.EnqueueResponse(@"* FLAGS (\Answered \Flagged \Deleted \Seen \Draft)\r\n" +
                             @"* OK [PERMANENTFLAGS (\Answered \Flagged \Deleted \Seen \Draft \*)] flags\r\n" +
                             "* 1 EXISTS\r\n" +
                             "* 0 RECENT\r\n" +
                             "* OK [UIDVALIDITY 1259315738] UID valid\r\n" +
                             "* OK [UIDNEXT 2] next UID\r\n" +
                             "0002 OK [READ-WRITE] done\r\n");
      // FETCH
      server.EnqueueResponse(string.Format(@"* 1 FETCH (RFC822.SIZE {0} BODYSTRUCTURE (""text"" ""plain"" (""charset"" ""utf8"") NIL NIL ""7bit"" 999 999 NIL NIL NIL NIL) BODY[]<0> {{{0}}}" + "\r\n",
                                           Encoding.ASCII.GetByteCount(message)) +
                             message + ")\r\n" +
                             "0003 OK done\r\n");

      return server;
    }

    private ImapPseudoServer InitializeAppendServer()
    {
      var server = new ImapPseudoServer();

      server.Start();

      // greeting
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1] ready\r\n");
      // LOGIN
      server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                             "0000 OK done\r\n");
      // APPEND
      server.EnqueueResponse("+ continue\r\n");
      server.EnqueueResponse(string.Empty);
      server.EnqueueResponse("0001 OK done\r\n");

      return server;
    }

    [Test]
    public void TestOpenRead()
    {
      using (var server = InitializeFetchServer()) {
        using (var client = new System.Net.WebClient()) {
          using (var stream = client.OpenRead(new Uri(string.Format("imap://{0}/INBOX/;UID=1", server.HostPort)))) {
            // stream does not support get_Length and Seek
            // FileAssert.AreEqual(new MemoryStream(Encoding.ASCII.GetBytes(message)), stream);

            var actualStream = new MemoryStream();

            stream.CopyTo(actualStream);

            FileAssert.AreEqual(new MemoryStream(Encoding.ASCII.GetBytes(message)), actualStream);
          }
        }
      }
    }

    [Test]
    public void TestOpenReadAsync()
    {
      if (!Runtime.IsRunningOnMono)
        Assert.Ignore("supported only on Mono");

      using (var server = InitializeFetchServer()) {
        using (var client = new System.Net.WebClient()) {
          using (var waitHandle = new AutoResetEvent(false)) {
            client.OpenReadCompleted += delegate(object sender, OpenReadCompletedEventArgs e) {
              try {
                Assert.IsNull(e.Error);

                // stream does not support get_Length and Seek
                // FileAssert.AreEqual(new MemoryStream(Encoding.ASCII.GetBytes(message)), e.Result);

                var actualStream = new MemoryStream();

                e.Result.CopyTo(actualStream);

                FileAssert.AreEqual(new MemoryStream(Encoding.ASCII.GetBytes(message)), actualStream);
              }
              finally {
                waitHandle.Set();
              }
            };

            client.OpenReadAsync(new Uri(string.Format("imap://{0}/INBOX/;UID=1", server.HostPort)));

            if (!waitHandle.WaitOne(5000))
              Assert.Fail("timed out");
          }
        }
      }
    }

    [Test]
    public void TestDownloadData()
    {
      using (var server = InitializeFetchServer()) {
        using (var client = new System.Net.WebClient()) {
          Assert.AreEqual(Encoding.ASCII.GetBytes(message),
                          client.DownloadData(new Uri(string.Format("imap://{0}/INBOX/;UID=1", server.HostPort))));
        }
      }
    }

    [Test]
    public void TestDownloadDataAsync()
    {
      if (!Runtime.IsRunningOnMono)
        Assert.Ignore("supported only on Mono");

      using (var server = InitializeFetchServer()) {
        using (var client = new System.Net.WebClient()) {
          using (var waitHandle = new AutoResetEvent(false)) {
            client.DownloadDataCompleted += delegate(object sender, DownloadDataCompletedEventArgs e) {
              try {
                Assert.IsNull(e.Error);
                Assert.AreEqual(Encoding.ASCII.GetBytes(message), e.Result);
              }
              finally {
                waitHandle.Set();
              }
            };

            client.DownloadDataAsync(new Uri(string.Format("imap://{0}/INBOX/;UID=1", server.HostPort)));

            if (!waitHandle.WaitOne(5000))
              Assert.Fail("timed out");
          }
        }
      }
    }

    [Test]
    public void TestDownloadFile()
    {
      const string filename = "downloaded.txt";

      try {
        if (File.Exists(filename))
          File.Delete(filename);

        using (var server = InitializeFetchServer()) {
          using (var client = new System.Net.WebClient()) {
            client.DownloadFile(new Uri(string.Format("imap://{0}/INBOX/;UID=1", server.HostPort)), filename);

            Assert.IsTrue(File.Exists(filename));

            using (var actual = File.OpenRead(filename)) {
              FileAssert.AreEqual(new MemoryStream(Encoding.ASCII.GetBytes(message)),
                                  actual);
            }
          }
        }
      }
      finally {
        if (File.Exists(filename))
          File.Delete(filename);
      }
    }

    [Test]
    public void TestDownloadFileAsync()
    {
      if (!Runtime.IsRunningOnMono)
        Assert.Ignore("supported only on Mono");

      const string filename = "downloaded.txt";

      try {
        if (File.Exists(filename))
          File.Delete(filename);

        using (var server = InitializeFetchServer()) {
          using (var client = new System.Net.WebClient()) {
            using (var waitHandle = new AutoResetEvent(false)) {
              client.DownloadFileCompleted += delegate {
                waitHandle.Set();
              };

              client.DownloadFileAsync(new Uri(string.Format("imap://{0}/INBOX/;UID=1", server.HostPort)), filename);

              if (!waitHandle.WaitOne(5000))
                Assert.Fail("timed out");
            }
          }
        }

        Assert.IsTrue(File.Exists(filename));

        using (var actual = File.OpenRead(filename)) {
          FileAssert.AreEqual(new MemoryStream(Encoding.ASCII.GetBytes(message)),
                              actual);
        }
      }
      finally {
        if (File.Exists(filename))
          File.Delete(filename);
      }
    }

    [Test]
    public void TestDownloadString()
    {
      using (var server = InitializeFetchServer()) {
        using (var client = new System.Net.WebClient()) {
          Assert.AreEqual(message,
                          client.DownloadString(new Uri(string.Format("imap://{0}/INBOX/;UID=1", server.HostPort))));
        }
      }
    }

    [Test]
    public void TestDownloadStringAsync()
    {
      if (!Runtime.IsRunningOnMono)
        Assert.Ignore("supported only on Mono");

      using (var server = InitializeFetchServer()) {
        using (var client = new System.Net.WebClient()) {
          using (var waitHandle = new AutoResetEvent(false)) {
            client.DownloadStringCompleted += delegate(object sender, DownloadStringCompletedEventArgs e) {
              try {
                Assert.IsNull(e.Error);
                Assert.AreEqual(message, e.Result);
              }
              finally {
                waitHandle.Set();
              }
            };

            client.DownloadStringAsync(new Uri(string.Format("imap://{0}/INBOX/;UID=1", server.HostPort)));

            if (!waitHandle.WaitOne(5000))
              Assert.Fail("timed out");
          }
        }
      }
    }

    [Test]
    public void TestUploadData()
    {
      using (var server = InitializeAppendServer()) {
        using (var client = new System.Net.WebClient()) {
          client.UploadData(string.Format("imap://{0}/INBOX", server.HostPort), ImapWebRequestMethods.Append, Encoding.ASCII.GetBytes(message));
        }

        AssertAppendRequest(server);
      }
    }

    [Test]
    public void TestUploadDataAsync()
    {
      if (!Runtime.IsRunningOnMono)
        Assert.Ignore("supported only on Mono");

      using (var server = InitializeAppendServer()) {
        using (var client = new System.Net.WebClient()) {
          using (var waitHandle = new AutoResetEvent(false)) {
            client.UploadDataCompleted += delegate(object sender, UploadDataCompletedEventArgs e) {
              try {
                Assert.IsNull(e.Error);
                Assert.AreEqual(new byte[0], e.Result);
              }
              finally {
                waitHandle.Set();
              }
            };

            client.UploadDataAsync(new Uri(string.Format("imap://{0}/INBOX", server.HostPort)), ImapWebRequestMethods.Append, Encoding.ASCII.GetBytes(message));

            if (!waitHandle.WaitOne(5000))
              Assert.Fail("timed out");

            AssertAppendRequest(server);
          }
        }
      }
    }

    [Test]
    public void TestUploadString()
    {
      using (var server = InitializeAppendServer()) {
        using (var client = new System.Net.WebClient()) {
          client.UploadString(string.Format("imap://{0}/INBOX", server.HostPort), ImapWebRequestMethods.Append, message);
        }

        AssertAppendRequest(server);
      }
    }

    [Test]
    public void TestUploadStringAsync()
    {
      if (!Runtime.IsRunningOnMono)
        Assert.Ignore("supported only on Mono");

      using (var server = InitializeAppendServer()) {
        using (var client = new System.Net.WebClient()) {
          using (var waitHandle = new AutoResetEvent(false)) {
            client.UploadStringCompleted += delegate(object sender, UploadStringCompletedEventArgs e) {
              try {
                Assert.IsNull(e.Error);
                Assert.AreEqual(string.Empty, e.Result);
              }
              finally {
                waitHandle.Set();
              }
            };

            client.UploadStringAsync(new Uri(string.Format("imap://{0}/INBOX", server.HostPort)), ImapWebRequestMethods.Append, message);

            if (!waitHandle.WaitOne(5000))
              Assert.Fail("timed out");

            AssertAppendRequest(server);
          }
        }
      }
    }

    private void AssertAppendRequest(ImapPseudoServer server)
    {
      server.DequeueRequest(); // LOGIN

      var appendRequest = server.DequeueRequest();

      StringAssert.StartsWith("0001 APPEND \"INBOX\"", appendRequest);
      StringAssert.EndsWith(string.Format(" {{{0}}}\r\n", Encoding.ASCII.GetByteCount(message)), appendRequest);

      StringAssert.StartsWith(message, server.DequeueAll());
    }

    [Test]
    public void TestOpenWriteWithInvalidMethod()
    {
      using (var server = InitializeFetchServer()) {
        using (var client = new System.Net.WebClient()) {
          try {
            client.OpenWrite(new Uri(string.Format("imap://{0}/", server.HostPort)), ImapWebRequestMethods.Lsub);
            Assert.Fail("WebException not thrown");
          }
          catch (WebException ex) {
            Assert.IsInstanceOfType(typeof(NotSupportedException), ex.InnerException);
          }

          try {
            client.OpenWrite(new Uri(string.Format("imap://{0}/INBOX", server.HostPort)), ImapWebRequestMethods.Subscribe);
            Assert.Fail("WebException not thrown");
          }
          catch (WebException ex) {
            Assert.IsInstanceOfType(typeof(ProtocolViolationException), ex.InnerException);
          }

          try {
            client.OpenWrite(new Uri(string.Format("imap://{0}/INBOX/;UID=1", server.HostPort)), ImapWebRequestMethods.Fetch);
            Assert.Fail("WebException not thrown");
          }
          catch (WebException ex) {
            Assert.IsInstanceOfType(typeof(NotSupportedException), ex.InnerException);
          }

          try {
            client.OpenWrite(new Uri(string.Format("imap://{0}/INBOX?UID 1", server.HostPort)), ImapWebRequestMethods.Search);
            Assert.Fail("WebException not thrown");
          }
          catch (WebException ex) {
            Assert.IsInstanceOfType(typeof(NotSupportedException), ex.InnerException);
          }
        }
      }
    }

    [Test]
    public void TestUploadDataWithInvalidMethod()
    {
      using (var server = InitializeFetchServer()) {
        using (var client = new System.Net.WebClient()) {
          try {
            client.UploadData(string.Format("imap://{0}/", server.HostPort), ImapWebRequestMethods.Lsub, Encoding.ASCII.GetBytes(message));
            Assert.Fail("WebException not thrown");
          }
          catch (WebException ex) {
            Assert.IsInstanceOfType(typeof(NotSupportedException), ex.InnerException);
          }

          try {
            client.UploadData(string.Format("imap://{0}/INBOX", server.HostPort), ImapWebRequestMethods.Subscribe, Encoding.ASCII.GetBytes(message));
            Assert.Fail("WebException not thrown");
          }
          catch (WebException ex) {
            Assert.IsInstanceOfType(typeof(ProtocolViolationException), ex.InnerException);
          }

          try {
            client.UploadData(string.Format("imap://{0}/INBOX/;UID=1", server.HostPort), ImapWebRequestMethods.Fetch, Encoding.ASCII.GetBytes(message));
            Assert.Fail("WebException not thrown");
          }
          catch (WebException ex) {
            Assert.IsInstanceOfType(typeof(NotSupportedException), ex.InnerException);
          }

          try {
            client.UploadData(string.Format("imap://{0}/INBOX?UID 1", server.HostPort), ImapWebRequestMethods.Search, Encoding.ASCII.GetBytes(message));
            Assert.Fail("WebException not thrown");
          }
          catch (WebException ex) {
            Assert.IsInstanceOfType(typeof(NotSupportedException), ex.InnerException);
          }
        }
      }
    }

    [Test]
    public void TestUploadStringWithInvalidMethod()
    {
      using (var server = InitializeFetchServer()) {
        using (var client = new System.Net.WebClient()) {
          try {
            client.UploadString(string.Format("imap://{0}/", server.HostPort), ImapWebRequestMethods.Lsub, message);
            Assert.Fail("WebException not thrown");
          }
          catch (WebException ex) {
            Assert.IsInstanceOfType(typeof(NotSupportedException), ex.InnerException);
          }

          try {
            client.UploadString(string.Format("imap://{0}/INBOX", server.HostPort), ImapWebRequestMethods.Subscribe, message);
            Assert.Fail("WebException not thrown");
          }
          catch (WebException ex) {
            Assert.IsInstanceOfType(typeof(ProtocolViolationException), ex.InnerException);
          }

          try {
            client.UploadString(string.Format("imap://{0}/INBOX/;UID=1", server.HostPort), ImapWebRequestMethods.Fetch, message);
            Assert.Fail("WebException not thrown");
          }
          catch (WebException ex) {
            Assert.IsInstanceOfType(typeof(NotSupportedException), ex.InnerException);
          }

          try {
            client.UploadString(string.Format("imap://{0}/INBOX?UID 1", server.HostPort), ImapWebRequestMethods.Search, message);
            Assert.Fail("WebException not thrown");
          }
          catch (WebException ex) {
            Assert.IsInstanceOfType(typeof(NotSupportedException), ex.InnerException);
          }
        }
      }
    }
  }
}
