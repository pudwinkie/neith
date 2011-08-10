using System;
using NUnit.Framework;

namespace Smdn.Formats.Playlists {
  [TestFixture]
  public class PlaylistFormatTests {
    [Test]
    public void TestFormats()
    {
      Assert.AreEqual(".m3u", PlaylistFormat.M3U.DefaultExtension);
      Assert.IsNotNull(PlaylistFormat.M3U.Format);
      Assert.IsNotNull(PlaylistFormat.M3U.Parse);

      Assert.AreEqual(".pls", PlaylistFormat.PLS.DefaultExtension);
      Assert.IsNotNull(PlaylistFormat.PLS.Format);
      Assert.IsNotNull(PlaylistFormat.PLS.Parse);

      Assert.AreEqual(".wpl", PlaylistFormat.WPL.DefaultExtension);
      Assert.IsNotNull(PlaylistFormat.WPL.Format);
      Assert.IsNotNull(PlaylistFormat.WPL.Parse);

      Assert.AreEqual(".xspf", PlaylistFormat.XSPF.DefaultExtension);
      Assert.IsNotNull(PlaylistFormat.XSPF.Format);
      Assert.IsNotNull(PlaylistFormat.XSPF.Parse);

      Assert.AreEqual(".asx", PlaylistFormat.ASX.DefaultExtension);
    }

    [Test]
    public void TestGuessFormatFromExtension()
    {
      foreach (var pair in new[] {
        new {Extension = ".m3u",  Expected = PlaylistFormat.M3U},
        new {Extension = ".pls",  Expected = PlaylistFormat.PLS},
        new {Extension = ".wpl",  Expected = PlaylistFormat.WPL},
        new {Extension = ".xspf", Expected = PlaylistFormat.XSPF},
        new {Extension = ".asx",  Expected = PlaylistFormat.ASX},
        new {Extension = ".txt",  Expected = (PlaylistFormat)null},
      }) {
        Assert.AreEqual(pair.Expected, PlaylistFormat.GuessFormatFromExtension(pair.Extension), pair.Extension);
      }
    }
  }
}
