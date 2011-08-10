using NUnit.Framework;
using System;
using System.IO;
using System.Net;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif

namespace Smdn.Formats.ApacheLog {
  [TestFixture]
  public class ApacheLogReaderTests {
    [Test]
    public void TestReadEntry()
    {
      var log = @"127.0.0.1 smdn.invisiblefulmoon.net - [14/Oct/2009:06:38:38 +0900] ""GET /misc/forum/ HTTP/1.0"" 200 2905 ""http://smdn.invisiblefulmoon.net/"" ""Mozilla/5.0 (X11; U; Linux i686; ja; rv:1.9.0.14) Gecko/2009090216 Ubuntu/9.04 (jaunty) Firefox/3.0.14""";

      using (var reader = new ApacheLogReader(new StringReader(log))) {
        var entry = reader.ReadEntry();

        Assert.IsNotNull(entry);
        Assert.AreEqual("127.0.0.1", entry.RemoteHost);
        Assert.AreEqual("smdn.invisiblefulmoon.net", entry.RemoteLogName);
        Assert.AreEqual(null, entry.RemoteUser);
        Assert.AreEqual(new DateTime(2009, 10, 14, 06, 38, 38), entry.RequestedTime.DateTime);
        Assert.AreEqual(new TimeSpan(9, 0, 0), entry.RequestedTime.Offset);
        Assert.AreEqual("GET /misc/forum/ HTTP/1.0", entry.RequestLine);
        Assert.AreEqual(HttpStatusCode.OK, entry.StatusCode);
        Assert.IsNotNull(entry.ResponseLengthCLF);
        Assert.AreEqual(2905L, entry.ResponseLengthCLF.Value);
        Assert.IsNotNull(entry.RequestHeaders[HttpRequestHeader.Referer]);
        Assert.AreEqual("http://smdn.invisiblefulmoon.net/", entry.Referer);
        Assert.IsNotNull(entry.RequestHeaders[HttpRequestHeader.UserAgent]);
        Assert.AreEqual("Mozilla/5.0 (X11; U; Linux i686; ja; rv:1.9.0.14) Gecko/2009090216 Ubuntu/9.04 (jaunty) Firefox/3.0.14", entry.UserAgent);

        entry = reader.ReadEntry();

        Assert.IsNull(entry);
      }
    }

    [Test]
    public void TestReadAllEntries()
    {
      var log = @"127.0.0.1 smdn.invisiblefulmoon.net - [14/Oct/2009:06:38:38 +0900] ""GET /misc/forum/ HTTP/1.0"" 200 2905 ""http://smdn.invisiblefulmoon.net/"" ""Mozilla/5.0 (X11; U; Linux i686; ja; rv:1.9.0.14) Gecko/2009090216 Ubuntu/9.04 (jaunty) Firefox/3.0.14""
127.0.0.1 smdn.invisiblefulmoon.net - [14/Oct/2009:06:38:38 +0900] ""GET /misc/forum/ HTTP/1.0"" 200 2905 ""http://smdn.invisiblefulmoon.net/"" ""Mozilla/5.0 (X11; U; Linux i686; ja; rv:1.9.0.14) Gecko/2009090216 Ubuntu/9.04 (jaunty) Firefox/3.0.14""";

      using (var reader = new ApacheLogReader(new StringReader(log))) {
        var entries = reader.ReadAllEntries();

        Assert.IsNotNull(entries);
        Assert.AreEqual(2, entries.Count());
      }
    }
  }
}
