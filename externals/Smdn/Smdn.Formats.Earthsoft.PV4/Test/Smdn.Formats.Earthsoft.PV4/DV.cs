using System;
using NUnit.Framework;

using Smdn.Mathematics;
using Smdn.Formats.Earthsoft.PV4.IO;

namespace Smdn.Formats.Earthsoft.PV4 {
  [TestFixture]
  public class DVTests {
    [Test]
    public void TestOpen()
    {
      using (var dv = DV.Open("test-1280x720p-10f.dv")) {
        Assert.AreEqual("test-1280x720p-10f.dv", dv.StreamFile);
        Assert.AreEqual("test-1280x720p-10f.dvi", dv.IndexFile);

        Assert.IsNotNull(dv.Header, "DV.Header");
        Assert.AreEqual(new Fraction(60000, 1001), dv.FrameRate);
        Assert.AreEqual(FrameScanning.Progressive, dv.FrameScanning);
        Assert.AreEqual(1280, dv.PixelsHorizontal);
        Assert.AreEqual(720, dv.PixelsVertical);
        Assert.AreEqual(new Fraction(16, 9), dv.DisplayAspectRatio);
        Assert.AreEqual(new Fraction(48000, 1), dv.AudioSamplingRate);
        Assert.AreEqual(10, dv.FrameCount);
        Assert.IsNotNull(dv.Reader);
      }
    }

    [Test]
    public void TestClose()
    {
      using (var dv = DV.Open("test-1280x720p-10f.dv")) {
        var reader = dv.Reader;

        dv.Close();

        try {
          Assert.IsNotNull(dv.GetFrame(0));
          Assert.Fail("ObjectDisposedException not thrown from DV");
        }
        catch (ObjectDisposedException) {
        }

        try {
          Assert.AreNotEqual(-1, reader.ReadByte());
          Assert.Fail("ObjectDisposedException not thrown from reader");
        }
        catch (ObjectDisposedException) {
        }
      }
    }

    [Test]
    public void TestGetFrame()
    {
      using (var dv = DV.Open("test-1280x720p-10f.dv")) {
        var frameData = dv.GetFrame(0);

        Assert.IsNotNull(frameData);
        StreamFileReaderTests.TestAudioData(frameData.Audio, true, 0);
        StreamFileReaderTests.TestVideoData(frameData.Video, true, 0);

        Assert.AreEqual(0x0321 * 4, frameData.Audio.Block.Count, "frame0 Audio.Block.Count");
        Assert.AreEqual(27296,  frameData.Video.Block0.Count, "frame0 Video.Block0.Count");
        Assert.AreEqual(27360,  frameData.Video.Block1.Count, "frame0 Video.Block1.Count");
        Assert.AreEqual(0,      frameData.Video.Block2.Count, "frame0 Video.Block2.Count");
        Assert.AreEqual(0,      frameData.Video.Block3.Count, "frame0 Video.Block3.Count");
      }
    }

    [Test]
    public void TestGetFrameBuffered()
    {
      using (var dv = DV.Open("test-1280x720p-10f.dv")) {
        StreamFileFrameData buffer = null;

        dv.GetFrame(0, ref buffer);

        Assert.IsNotNull(buffer);
        StreamFileReaderTests.TestAudioData(buffer.Audio, true, 0);
        StreamFileReaderTests.TestVideoData(buffer.Video, true, 0);

        var frame0 = buffer;

        dv.GetFrame(1, ref buffer);

        Assert.IsNotNull(buffer);
        StreamFileReaderTests.TestAudioData(buffer.Audio, true, 1);
        StreamFileReaderTests.TestVideoData(buffer.Video, true, 1);

        Assert.AreSame(frame0, buffer);
      }
    }

    [Test]
    public void TestGetAudio()
    {
      using (var dv = DV.Open("test-1280x720p-10f.dv")) {
        var frameData = dv.GetAudio(0);

        Assert.IsNotNull(frameData);
        StreamFileReaderTests.TestAudioData(frameData.Audio, true, 0);
        StreamFileReaderTests.TestVideoData(frameData.Video, false, 0);

        Assert.AreEqual(0x0321 * 4, frameData.Audio.Block.Count, "frame0 Audio.Block.Count");
      }
    }

    [Test]
    public void TestGetVideo()
    {
      using (var dv = DV.Open("test-1280x720p-10f.dv")) {
        var frameData = dv.GetVideo(0);

        Assert.IsNotNull(frameData);
        StreamFileReaderTests.TestAudioData(frameData.Audio, false, 0);
        StreamFileReaderTests.TestVideoData(frameData.Video, true, 0);

        Assert.AreEqual(27296,  frameData.Video.Block0.Count, "frame0 Video.Block0.Count");
        Assert.AreEqual(27360,  frameData.Video.Block1.Count, "frame0 Video.Block1.Count");
        Assert.AreEqual(0,      frameData.Video.Block2.Count, "frame0 Video.Block2.Count");
        Assert.AreEqual(0,      frameData.Video.Block3.Count, "frame0 Video.Block3.Count");
      }
    }
  }
}
