using System;
using System.IO;
using NUnit.Framework;

namespace Smdn.Formats.Thbgm {
  [TestFixture]
  public class TrackInfoTests {
    [Test]
    public void TestToString()
    {
      var track = ProductInfo.Th08.Tracks[0];

      Assert.AreEqual("th08_01.wav", track.ToString("$prefix_$trackno.wav"));
      Assert.AreEqual("01. 永夜抄　～ Eastern Night..wav", track.ToString("$trackno. $title.wav"));
      Assert.AreEqual("永01", track.ToString("$pshortest$trackno"));
      Assert.AreEqual("永夜抄 - 東方永夜抄　～ Imperishable Night. - 上海アリス幻樂団", track.ToString("$pshort2 - $product - $creator"));
      Assert.AreEqual("上海アリス幻樂団 (2004年08月15日)", track.ToString("$creator ($ryear年$rmonth月$rday日)"));
      Assert.AreEqual("$hoge$hogeth08$", track.ToString("$hoge$hoge$prefix$"));
    }

    [Test]
    public void TestThplayFormatProductInfoSamplingRateFieldSpecified()
    {
      var testfilename = "titles_thplay.txt";

      try {
        if (File.Exists(testfilename))
          File.Delete(testfilename);

        File.WriteAllText(testfilename, @"@骨川アリス幻樂団\東方臑茶魔体験版\thbgm_tr.dat,東方臑茶魔　～ The Acquaintance Of My Daddy. 体験版
#=SamplingRate,22050,99999999,99999999,99999999,スネ夫が激しい弾幕を展開する時に流れている曲1
99999999,99999999,99999999,スネ夫が激しい弾幕を展開する時に流れている曲2
");

        var product = ProductInfo.LoadFrom(testfilename);

        Assert.AreEqual(2, product.Tracks.Count);

        var track = product.Tracks[0];

        Assert.AreEqual(22050, track.StreamFormat.SamplesPerSecond);
        Assert.AreEqual(16, track.StreamFormat.BitsPerSample);
        Assert.AreEqual(2, track.StreamFormat.Channels);
        Assert.AreEqual("スネ夫が激しい弾幕を展開する時に流れている曲1", track.Title);

        track = product.Tracks[1];

        Assert.AreEqual(44100, track.StreamFormat.SamplesPerSecond);
        Assert.AreEqual(16, track.StreamFormat.BitsPerSample);
        Assert.AreEqual(2, track.StreamFormat.Channels);
        Assert.AreEqual("スネ夫が激しい弾幕を展開する時に流れている曲2", track.Title);
      }
      finally {
        if (File.Exists(testfilename))
          File.Delete(testfilename);
      }
    }

    [Test]
    public void TestThplayFormatProductInfoStreamFileFieldSpecified()
    {
      var testfilename = "titles_thplay.txt";

      try {
        if (File.Exists(testfilename))
          File.Delete(testfilename);

        File.WriteAllText(testfilename, @"@骨川アリス幻樂団\東方臑茶魔体験版\thbgm_tr.dat,東方臑茶魔　～ The Acquaintance Of My Daddy. 体験版
%track1.wav,99999999,99999999,99999999,スネ夫が激しい弾幕を展開する時に流れている曲1
99999999,99999999,99999999,スネ夫が激しい弾幕を展開する時に流れている曲2
");

        var product = ProductInfo.LoadFrom(testfilename);

        Assert.AreEqual(2, product.Tracks.Count);

        var track = product.Tracks[0];

        Assert.AreEqual(44100, track.StreamFormat.SamplesPerSecond);
        Assert.AreEqual(16, track.StreamFormat.BitsPerSample);
        Assert.AreEqual(2, track.StreamFormat.Channels);
        Assert.AreEqual("スネ夫が激しい弾幕を展開する時に流れている曲1", track.Title);

        try {
          using (var s = track.GetStream("/tmp/thplay/stream/", 2)) {
            Assert.Fail("expected IOException not thrown");
          }
        }
        catch (Exception ex) {
          var fnfex = ex as FileNotFoundException;
          var dnfex = ex as DirectoryNotFoundException;

          if (fnfex != null)
            Assert.AreEqual("/tmp/thplay/stream/track1.wav", fnfex.FileName);
          else if (dnfex != null)
            StringAssert.Contains("/tmp/thplay/stream/track1.wav", dnfex.Message);
          else
            throw;
        }

        track = product.Tracks[1];

        Assert.AreEqual(44100, track.StreamFormat.SamplesPerSecond);
        Assert.AreEqual(16, track.StreamFormat.BitsPerSample);
        Assert.AreEqual(2, track.StreamFormat.Channels);
        Assert.AreEqual("スネ夫が激しい弾幕を展開する時に流れている曲2", track.Title);

        try {
          using (var s = track.GetStream("/tmp/thplay/stream/stream.dat", 2)) {
            Assert.Fail("expected IOException not thrown");
          }
        }
        catch (Exception ex) {
          var fnfex = ex as FileNotFoundException;
          var dnfex = ex as DirectoryNotFoundException;

          if (fnfex != null)
            Assert.AreEqual("/tmp/thplay/stream/stream.dat", fnfex.FileName);
          else if (dnfex != null)
            StringAssert.Contains("/tmp/thplay/stream/stream.dat", dnfex.Message);
          else
            throw;
        }
      }
      finally {
        if (File.Exists(testfilename))
          File.Delete(testfilename);
      }
    }
  }
}