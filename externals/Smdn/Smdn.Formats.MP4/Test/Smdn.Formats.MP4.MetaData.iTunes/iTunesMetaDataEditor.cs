using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using NUnit.Framework;

using Smdn.Formats.IsoBaseMediaFile;

using Standards = Smdn.Formats.IsoBaseMediaFile.Standards;

namespace Smdn.Formats.MP4.MetaData.iTunes {
  [TestFixture]
  public class iTunesMetaDataEditorTests {
    private void TestTags(iTunesMetaDataEditor editor)
    {
      Assert.IsTrue(editor.HasMetaData);

      Assert.AreEqual("album", editor.Album);
      Assert.AreEqual("albumartist", editor.AlbumArtist);
      Assert.AreEqual("trackartist", editor.Artist);
      Assert.AreEqual(1, editor.Artworks.Count);
      Assert.IsInstanceOfType(typeof(PngArtwork), editor.Artworks[0]);

      using (var image = editor.Artworks[0].ToImage()) {
        Assert.AreEqual(16, image.Width);
        Assert.AreEqual(16, image.Height);
      }

      Assert.AreEqual(0, editor.Bpm);
      Assert.IsNull(editor.Category);
      Assert.AreEqual("コメント", editor.Comment);
      Assert.IsNull(editor.Composer);
      Assert.IsNull(editor.Copyright);
      Assert.IsNull(editor.Description);
      Assert.AreEqual(0, editor.DiskCount);
      Assert.AreEqual(0, editor.DiskNumber);
      Assert.AreEqual("Lavf52.32.0", editor.Encoder);
      Assert.IsFalse(editor.GaplessPlayback);
      Assert.IsNull(editor.Genre);
      Assert.AreEqual(0, editor.GenreNumber);
      Assert.IsNull(editor.Grouping);
      Assert.IsNull(editor.Keyword);
      Assert.IsNull(editor.Lyrics);
      Assert.IsNull(editor.ReleaseDate);
      Assert.AreEqual(0, editor.TrackCount);
      Assert.AreEqual(1, editor.TrackNumber);
      Assert.AreEqual("title", editor.Title);
    }

    [Test]
    public void TestOpen()
    {
      using (var editor = new iTunesMetaDataEditor("test-itunes.m4a", false)) {
        TestTags(editor);
      }
    }

    [Test]
    public void TestSave()
    {
      var dest = "test-save.m4a";

      try {
        using (var mediaFile = new MediaFile("test-itunes.m4a")) {
          var editor = new iTunesMetaDataEditor(mediaFile);

          editor.Save(dest);
        }

        using (var editor = new iTunesMetaDataEditor(dest, false)) {
          TestTags(editor);
        }
      }
      finally {
        if (File.Exists(dest))
          File.Delete(dest);
      }
    }

    [Test]
    public void TestOverwrite()
    {
      var dest = "test-save.m4a";

      try {
        File.Copy("test-itunes.m4a", dest, true);

        using (var editor = new iTunesMetaDataEditor(dest, true)) {
          var artworkImage = editor.Artworks[0].ToImage();

          editor.RemoveMetaDataBoxes();

          Assert.IsNull(editor.MediaFile.Find("moov", "udta", "meta", "ilst"));
          Assert.IsNull(editor.MediaFile.Find("moov", "udta", "meta", "ilst", "\x00a9cmt"));
          Assert.IsNull(editor.MediaFile.Find("moov", "udta", "meta", "ilst", "covr"));
          Assert.IsNull(editor.MediaFile.Find("moov", "udta", "meta", "ilst", "trkn"));

          editor.Album = "album";
          editor.AlbumArtist = "albumartist";
          editor.Artist = "trackartist";
          editor.Artworks.Add(new PngArtwork(artworkImage));
          editor.Comment = "コメント";
          editor.Encoder = "Lavf52.32.0";
          editor.Title = "title";
          editor.TrackNumber = 1;

          editor.UpdateBoxes();

          Assert.IsNotNull(editor.MediaFile.Find("moov", "udta", "meta", "ilst"));
          Assert.IsNotNull(editor.MediaFile.Find("moov", "udta", "meta", "ilst", "\x00a9cmt"));
          Assert.IsNotNull(editor.MediaFile.Find("moov", "udta", "meta", "ilst", "covr"));
          Assert.IsNotNull(editor.MediaFile.Find("moov", "udta", "meta", "ilst", "trkn"));

          editor.Save();
        }

        using (var editor = new iTunesMetaDataEditor(dest, false)) {
          TestTags(editor);
        }
      }
      finally {
        if (File.Exists(dest))
          File.Delete(dest);
      }
    }

    [Test]
    public void TestHasMetaData()
    {
      using (var editor = new iTunesMetaDataEditor("test-itunes.m4a", false)) {
        Assert.IsNotNull(editor.MediaFile.Find("moov", "udta", "meta", "ilst"));
        Assert.IsTrue(editor.HasMetaData);

        editor.MediaFile.Remove("moov", "udta");

        Assert.IsFalse(editor.HasMetaData);
      }
    }

    [Test]
    public void TestRemoveMetaDataBoxes()
    {
      using (var editor = new iTunesMetaDataEditor("test-itunes.m4a", false)) {
        Assert.IsNotNull(editor.MediaFile.Find("moov", "udta", "meta", "ilst"));
        Assert.IsTrue(editor.HasMetaData);

        var meta = Box.Find<Standards.Iso.MetaBox>(editor.MediaFile.Find("moov", "udta") as Standards.Iso.UserDataBox);

        meta.Boxes.Add(new Standards.Iso.FreeSpaceBox());
        meta.Boxes.Add(new Standards.Iso.FreeSpaceBox());

        editor.RemoveMetaDataBoxes();

        Assert.IsFalse(editor.HasMetaData);
        Assert.IsNull(editor.MediaFile.Find("moov", "udta", "meta", "ilst"));
        Assert.IsNull(editor.MediaFile.Find("moov", "udta", "meta"));
        Assert.IsNull(editor.MediaFile.Find("moov", "udta"));
        Assert.IsNotNull(editor.MediaFile.Find("moov"));

        editor.RemoveMetaDataBoxes(); // call again
      }
    }

    [Test]
    public void TestRemoveMetaDataBoxesKeepMetaBox()
    {
      using (var editor = new iTunesMetaDataEditor("test-itunes.m4a", false)) {
        Assert.IsNotNull(editor.MediaFile.Find("moov", "udta", "meta", "ilst"));
        Assert.IsTrue(editor.HasMetaData);

        var meta = Box.Find<Standards.Iso.MetaBox>(editor.MediaFile.Find("moov", "udta") as Standards.Iso.UserDataBox);

        meta.Boxes.Add(new Standards.ID3.ID3v2Box());
        meta.Boxes.Add(new Standards.Iso.FreeSpaceBox());
        meta.Boxes.Add(new Standards.Iso.FreeSpaceBox());

        editor.RemoveMetaDataBoxes();

        Assert.IsFalse(editor.HasMetaData);
        Assert.IsNull(editor.MediaFile.Find("moov", "udta", "meta", "ilst"));
        Assert.IsNotNull(editor.MediaFile.Find("moov", "udta", "meta"));
        Assert.IsNotNull(editor.MediaFile.Find("moov", "udta"));
        Assert.IsNotNull(editor.MediaFile.Find("moov"));

        editor.RemoveMetaDataBoxes(); // call again
      }
    }
  }
}
