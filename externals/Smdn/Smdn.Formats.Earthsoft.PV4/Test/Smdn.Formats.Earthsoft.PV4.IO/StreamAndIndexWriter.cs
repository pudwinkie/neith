using System;
using System.IO;
using NUnit.Framework;

namespace Smdn.Formats.Earthsoft.PV4.IO {
  [TestFixture]
  public class StreamAndIndexWriterTests {
    [Test]
    public void TestConstruct()
    {
      const string constructFileName = "test-out";
      const string testStreamFile = constructFileName + ".dv";
      const string testIndexFile = constructFileName + ".dvi";

      try {
        if (File.Exists(testStreamFile))
          File.Delete(testStreamFile);
        if (File.Exists(testIndexFile))
          File.Delete(testIndexFile);

        using (var reader = new StreamFileReader("test-1280x720p-10f.dv")) {
          var header = reader.ReadHeader();

          using (var writer = new StreamAndIndexWriter(header, constructFileName)) {
            writer.Close();
          }
        }

        Assert.IsTrue(File.Exists(testStreamFile));
        Assert.IsTrue(File.Exists(testIndexFile));

        Assert.AreEqual(StreamFileHeaderData.Size, (new FileInfo(testStreamFile)).Length);
        Assert.AreEqual(0L, (new FileInfo(testIndexFile)).Length);
      }
      finally {
        if (File.Exists(testStreamFile))
          File.Delete(testStreamFile);
        if (File.Exists(testIndexFile))
          File.Delete(testIndexFile);
      }
    }

    [Test]
    public void TestWrite()
    {
      const string constructFileName = "test-out";
      const string testStreamFile = constructFileName + ".dv";
      const string testIndexFile = constructFileName + ".dvi";

      try {
        if (File.Exists(testStreamFile))
          File.Delete(testStreamFile);
        if (File.Exists(testIndexFile))
          File.Delete(testIndexFile);

        using (var reader = new StreamFileReader("test-1280x720p-10f.dv")) {
          var header = reader.ReadHeader();

          using (var writer = new StreamAndIndexWriter(header, constructFileName)) {
            for (var frameNumber = 0;; frameNumber++) {
              var frameData = reader.ReadFrameData();

              if (frameData == null)
                break;

              writer.Write(frameData);
            }
          }
        }

        Assert.IsTrue(File.Exists(testStreamFile));
        Assert.IsTrue(File.Exists(testIndexFile));

        FileAssert.AreEqual("test-1280x720p-10f.dvi", testIndexFile);

#if false
        FileAssert.AreEqual("test-1280x720p-10f.dv", testStreamFile);
#else
        using (var expectedDataStream = File.OpenRead("test-1280x720p-10f.dv")) {
          var alignment = new byte[4096 - expectedDataStream.Length % 4096];
          var expectedAlignedDataStream = new Smdn.IO.ExtendStream(expectedDataStream, new byte[0], alignment, true);

          using (var actualDataStream = File.OpenRead(testStreamFile)) {
            FileAssert.AreEqual(expectedAlignedDataStream, actualDataStream);
          }
        }
#endif
      }
      finally {
        if (File.Exists(testStreamFile))
          File.Delete(testStreamFile);
        if (File.Exists(testIndexFile))
          File.Delete(testIndexFile);
      }
    }
  }
}

