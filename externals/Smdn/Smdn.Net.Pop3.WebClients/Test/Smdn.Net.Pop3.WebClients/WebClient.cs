using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using NUnit.Framework;

using PopPseudoServer = Smdn.Net.Pop3.Client.Session.PopPseudoServer;

namespace Smdn.Net.Pop3.WebClients {
  [TestFixture]
  public class WebClientTests {
    private static readonly string message = @"MIME-Version: 1.0
From: from
To: to
Subect: subject
Date: Sat, 16 Jan 2010 00:41:20 +0900

1st line
2nd line
3rd line
".Replace("\r\n", "\n").Replace("\n", "\r\n");

    [SetUp]
    public void Setup()
    {
      PopWebRequestCreator.RegisterPrefix();
    }

    private PopPseudoServer InitializeServer()
    {
      var server = new PopPseudoServer();

      server.Start();

      // greeting
      server.EnqueueResponse("+OK\r\n");
      // CAPA
      server.EnqueueResponse("+OK\r\n" +
                             ".\r\n");
      // USER
      server.EnqueueResponse("+OK\r\n");
      // PASS
      server.EnqueueResponse("+OK\r\n");
      // RETR
      server.EnqueueResponse("+OK\r\n" +
                             message +
                             ".\r\n");
      // QUIT
      server.EnqueueResponse("+OK\r\n");

      return server;
    }

    [Test]
    public void TestOpenRead()
    {
      using (var server = InitializeServer()) {
        using (var client = new System.Net.WebClient()) {
          using (var stream = client.OpenRead(new Uri(string.Format("pop://{0}/;MSG=1", server.HostPort)))) {
            FileAssert.AreEqual(new MemoryStream(Encoding.ASCII.GetBytes(message)), stream);
          }
        }
      }
    }

    [Test]
    public void TestOpenReadAsync()
    {
      if (!Runtime.IsRunningOnMono)
        Assert.Ignore("supported only on Mono");

      using (var server = InitializeServer()) {
        using (var client = new System.Net.WebClient()) {
          using (var waitHandle = new AutoResetEvent(false)) {
            client.OpenReadCompleted += delegate(object sender, OpenReadCompletedEventArgs e) {
              try {
                Assert.IsNull(e.Error);

                FileAssert.AreEqual(new MemoryStream(Encoding.ASCII.GetBytes(message)), e.Result);
              }
              finally {
                waitHandle.Set();
              }
            };

            client.OpenReadAsync(new Uri(string.Format("pop://{0}/;MSG=1", server.HostPort)));

            if (!waitHandle.WaitOne(5000))
              Assert.Fail("timed out");
          }
        }
      }
    }

    [Test]
    public void TestDownloadData()
    {
      using (var server = InitializeServer()) {
        using (var client = new System.Net.WebClient()) {
          Assert.AreEqual(Encoding.ASCII.GetBytes(message),
                          client.DownloadData(new Uri(string.Format("pop://{0}/;MSG=1", server.HostPort))));
        }
      }
    }

    [Test]
    public void TestDownloadDataAsync()
    {
      if (!Runtime.IsRunningOnMono)
        Assert.Ignore("supported only on Mono");

      using (var server = InitializeServer()) {
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

            client.DownloadDataAsync(new Uri(string.Format("pop://{0}/;MSG=1", server.HostPort)));

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

        using (var server = InitializeServer()) {
          using (var client = new System.Net.WebClient()) {
            client.DownloadFile(new Uri(string.Format("pop://{0}/;MSG=1", server.HostPort)), filename);

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

        using (var server = InitializeServer()) {
          using (var client = new System.Net.WebClient()) {
            using (var waitHandle = new AutoResetEvent(false)) {
              client.DownloadFileCompleted += delegate {
                waitHandle.Set();
              };

              client.DownloadFileAsync(new Uri(string.Format("pop://{0}/;MSG=1", server.HostPort)), filename);

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
      using (var server = InitializeServer()) {
        using (var client = new System.Net.WebClient()) {
          Assert.AreEqual(message,
                          client.DownloadString(new Uri(string.Format("pop://{0}/;MSG=1", server.HostPort))));
        }
      }
    }

    [Test]
    public void TestDownloadStringAsync()
    {
      if (!Runtime.IsRunningOnMono)
        Assert.Ignore("supported only on Mono");

      using (var server = InitializeServer()) {
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

            client.DownloadStringAsync(new Uri(string.Format("pop://{0}/;MSG=1", server.HostPort)));

            if (!waitHandle.WaitOne(5000))
              Assert.Fail("timed out");
          }
        }
      }
    }

    [Test]
    public void TestOpenWrite()
    {
      using (var server = InitializeServer()) {
        using (var client = new System.Net.WebClient()) {
          try {
            client.OpenWrite(new Uri(string.Format("pop://{0}/", server.HostPort)), PopWebRequestMethods.List);
            Assert.Fail("WebException not thrown");
          }
          catch (WebException) {
          }

          try {
            client.OpenWrite(new Uri(string.Format("pop://{0}/;MSG=1", server.HostPort)), PopWebRequestMethods.Retr);
            Assert.Fail("WebException not thrown");
          }
          catch (WebException) {
          }
        }
      }
    }
    [Test]
    public void TestUploadData()
    {
      using (var server = InitializeServer()) {
        using (var client = new System.Net.WebClient()) {
          try {
            client.UploadData(string.Format("pop://{0}/", server.HostPort), PopWebRequestMethods.List, Encoding.ASCII.GetBytes(message));
            Assert.Fail("WebException not thrown");
          }
          catch (WebException) {
          }

          try {
            client.UploadData(string.Format("pop://{0}/;MSG=1", server.HostPort), PopWebRequestMethods.Retr, Encoding.ASCII.GetBytes(message));
            Assert.Fail("WebException not thrown");
          }
          catch (WebException) {
          }
        }
      }
    }

    [Test]
    public void TestUploadString()
    {
      using (var server = InitializeServer()) {
        using (var client = new System.Net.WebClient()) {
          try {
            client.UploadString(string.Format("pop://{0}/", server.HostPort), PopWebRequestMethods.List, message);
            Assert.Fail("WebException not thrown");
          }
          catch (WebException) {
          }

          try {
            client.UploadString(string.Format("pop://{0}/;MSG=1", server.HostPort), PopWebRequestMethods.Retr, message);
            Assert.Fail("WebException not thrown");
          }
          catch (WebException) {
          }
        }
      }
    }
  }
}
