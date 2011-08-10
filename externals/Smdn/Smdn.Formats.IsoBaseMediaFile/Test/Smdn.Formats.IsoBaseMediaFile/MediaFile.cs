using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;

using Smdn.Formats.IsoBaseMediaFile.Standards.Iso;

using Standards = Smdn.Formats.IsoBaseMediaFile.Standards;

namespace Smdn.Formats.IsoBaseMediaFile {
  [TestFixture]
  public class MediaFileTests {
    private void TestContent(MediaFile mediaFile, bool testBoxOffset)
    {
      var ftyp = mediaFile.Find(Box.TypeFourCC.Ftyp) as FileTypeBox;

      Assert.IsNotNull(ftyp, "ftyp");
      Assert.AreEqual(Box.TypeFourCC.Ftyp, ftyp.Type);
      if (testBoxOffset)
        Assert.AreEqual(0, ftyp.Offset);
      Assert.AreEqual(24, ftyp.Size);
      Assert.IsNull(ftyp.UndefinedFieldData);
      Assert.AreEqual(new FourCC("M4A "), ftyp.MajorBrand);
      Assert.AreEqual(512, ftyp.MinorVersion);
      Assert.AreEqual(2, ftyp.CompatibleBrands.Count);
      Assert.AreEqual(Box.FtypFourCC.Isom, ftyp.CompatibleBrands[0]);
      Assert.AreEqual(Box.FtypFourCC.Iso2, ftyp.CompatibleBrands[1]);

      var free = mediaFile.Find(new FourCC("free")) as FreeSpaceBox;

      Assert.IsNotNull(free, "free");
      Assert.AreEqual(new FourCC("free"), free.Type);
      if (testBoxOffset)
        Assert.AreEqual(24, free.Offset);
      Assert.AreEqual(8, free.Size);

      var mdat = mediaFile.Find(Box.TypeFourCC.Mdat) as MediaDataBox;

      Assert.IsNotNull(mdat, "mdat");
      Assert.AreEqual(Box.TypeFourCC.Mdat, mdat.Type);
      if (testBoxOffset)
        Assert.AreEqual(32, mdat.Offset);
      Assert.AreEqual(86, mdat.Size);
      Assert.IsNotNull(mdat.Data);
      Assert.AreEqual(78, mdat.Data.Length);
      Assert.AreEqual(new byte[] {
        0xDE, 0x36, 0x00, 0x00, 0x6C, 0x69, 0x62, 0x66,
        0x61, 0x61, 0x63, 0x20, 0x31, 0x2E, 0x32, 0x36,
        0x2E, 0x31, 0x20, 0x28, 0x4A, 0x61, 0x6E, 0x20,
        0x32, 0x32, 0x20, 0x32, 0x30, 0x30, 0x38, 0x29,
        0x20, 0x55, 0x4E, 0x53, 0x54, 0x41, 0x42, 0x4C,
        0x45, 0x00, 0x00, 0x42, 0x00, 0x9A, 0xD0, 0x04,
        0x32, 0x00, 0x47, 0x21, 0x00, 0x4D, 0x68, 0x02,
        0x19, 0x00, 0x23, 0x80, 0x21, 0x00, 0x49, 0x90,
        0x02, 0x19, 0x00, 0x23, 0x80, 0x21, 0x00, 0x49,
        0x90, 0x02, 0x19, 0x00, 0x23, 0x80,
      }, (new Smdn.IO.BinaryReader(mdat.Data.OpenRead())).ReadToEnd());

      var moov = mediaFile.Find("moov") as MovieBox;

      Assert.IsNotNull(moov, "moov");

      var mvhd = Box.Find<MovieHeaderBox>(moov);

      Assert.IsNotNull(mvhd, "mvhd");
      Assert.AreEqual(new FourCC("mvhd"), mvhd.Type);
      if (testBoxOffset)
        Assert.AreEqual(126, mvhd.Offset);
      Assert.AreEqual(108, mvhd.Size);
      Assert.AreEqual(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), mvhd.CreationTime);
      Assert.AreEqual(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), mvhd.ModificationTime);
      Assert.AreEqual(1000, mvhd.TimeScale);
      Assert.AreEqual(93, mvhd.Duration);
      Assert.AreEqual(1.00m, mvhd.Rate);
      Assert.AreEqual(1.00m, mvhd.Volume);
      Assert.AreEqual(Matrix.Unity, mvhd.Matrix);
      Assert.AreEqual(2, mvhd.NextTrackId);

      var tkhd = mediaFile.Find("moov", "trak", "tkhd") as TrackHeaderBox;

      Assert.IsNotNull(tkhd, "tkhd");
      Assert.AreEqual(new FourCC("tkhd"), tkhd.Type);

      Assert.AreEqual(mvhd.CreationTime, tkhd.CreationTime);
      Assert.AreEqual(mvhd.ModificationTime, tkhd.ModificationTime);
      Assert.AreEqual(mvhd.Duration, tkhd.Duration);
      Assert.AreEqual(0.00m, tkhd.Width);
      Assert.AreEqual(0.00m, tkhd.Height);

      var stbl = mediaFile.Find("moov", "trak", "mdia", "minf", "stbl") as SampleTableBox;

      Assert.IsNotNull(stbl, "stbl");

      var stsd = Box.Find<SampleDescriptionBox>(stbl);

      Assert.IsNotNull(stsd, "stsd");
      Assert.AreEqual(1, stsd.Entries.Count);

      var mp4a = stsd.Entries[0] as Smdn.Formats.IsoBaseMediaFile.Standards.Iso.MP4.MP4AudioSampleEntry;

      Assert.IsNotNull(mp4a, "mp4a");
      Assert.AreEqual(2, mp4a.ChannelCount);
      Assert.AreEqual(16, mp4a.SampleSize);
      Assert.AreEqual(44100.00m, mp4a.SampleRate);

      var stts = Box.Find<DecodingTimeToSampleBox>(stbl);

      Assert.IsNotNull(stts, "stts");
      Assert.AreEqual(1, stts.EntryCount);
      Assert.AreEqual(4, stts.Entries[0].SampleCount);
      Assert.AreEqual(1024, stts.Entries[0].SampleDelta);

      var stsc = Box.Find<SampleToChunkBox>(stbl);

      Assert.IsNotNull(stsc, "stsc");
      Assert.AreEqual(1, stsc.EntryCount);
      Assert.AreEqual(1, stsc.Entries[0].FirstChunk);
      Assert.AreEqual(1, stsc.Entries[0].SamplesPerChunk);
      Assert.AreEqual(1, stsc.Entries[0].SampleDescriptionIndex);

      var stsz = Box.Find<SampleSizeBox>(stbl);

      Assert.IsNotNull(stsz, "stsz");
      Assert.AreEqual(0, stsz.SampleSize);
      Assert.AreEqual(4, stsz.SampleCount);
      Assert.AreEqual(51, stsz.EntrySizes[0]);
      Assert.AreEqual(9, stsz.EntrySizes[1]);
      Assert.AreEqual(9, stsz.EntrySizes[2]);
      Assert.AreEqual(9, stsz.EntrySizes[3]);

      var stco = Box.Find<ChunkOffsetBox>(stbl);

      Assert.IsNotNull(stco, "stco");
      Assert.AreEqual(4, stco.EntryCount);
      Assert.AreEqual(40, stco.ChunkOffsets[0]);
      Assert.AreEqual(91, stco.ChunkOffsets[1]);
      Assert.AreEqual(100, stco.ChunkOffsets[2]);
      Assert.AreEqual(109, stco.ChunkOffsets[3]);

      var meta = mediaFile.Find("moov", "udta", "meta") as MetaBox;

      Assert.IsNotNull(meta, "meta");
      if (testBoxOffset)
        Assert.AreEqual(706, meta.Offset);
      Assert.AreEqual(89, meta.Size);
      Assert.IsNotNull(meta.Handler);
      Assert.AreEqual(new FourCC("mdir"), meta.Handler.HandlerType);
      Assert.AreEqual(new FourCC("ilst"), meta.Boxes[0].Type);
    }

    [Test]
    public void TestRead()
    {
      using (var m4a = new MediaFile("test.m4a")) {
        TestContent(m4a, true);
      }
    }

    [Test]
    public void TestSaveToStream()
    {
      var m4a = new MediaFile("test.m4a");
      var stream = new MemoryStream();

      m4a.Save(stream);

      stream.Close();

      Assert.AreEqual(File.ReadAllBytes("test.m4a"), stream.ToArray());

      m4a.Close();
    }

    [Test]
    public void TestSaveToFile()
    {
      var origin = "test.m4a";
      var file = "test-saved.m4a";

      try {
        File.Copy(origin, file, true);

        var m4a = new MediaFile(origin);

        m4a.Save(file);

        using (Stream expected = File.OpenRead(origin))
        using (Stream actual   = File.OpenRead(file)) {
          FileAssert.AreEqual(expected, actual);
        }
      }
      finally {
        if (File.Exists(file))
          File.Delete(file);
      }
    }

    [Test, ExpectedException(typeof(IOException))]
    public void TestSaveToInputFile()
    {
      var origin = "test.m4a";
      var file = "test-saved.m4a";

      try {
        File.Copy(origin, file, true);

        var m4a = new MediaFile(file);

        m4a.Save(file);
      }
      finally {
        if (File.Exists(file))
          File.Delete(file);
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSaveToInputFileStream()
    {
      var origin = "test.m4a";
      var file = "test-saved.m4a";

      try {
        File.Copy(origin, file, true);

        using (var stream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)) {
          var m4a = new MediaFile(stream);

          m4a.Save(stream);
        }
      }
      finally {
        if (File.Exists(file))
          File.Delete(file);
      }
    }

    [Test]
    public void TestSaveOverwrite()
    {
      var origin = "test.m4a";
      var file = "test-overwrite.m4a";

      try {
        File.Copy(origin, file, true);

        using (var m4a = new MediaFile(file, true)) {
          m4a.Save();
        }
      }
      finally {
        if (File.Exists(file))
          File.Delete(file);
      }
    }

    [Test]
    public void TestSaveOverwriteRestructured()
    {
      var origin = "test.m4a";
      var file = "test-overwrite.m4a";

      try {
        File.Copy(origin, file, true);

        using (var m4a = new MediaFile(file, true)) {
          var boxes = new List<Box>(m4a.Boxes);

          Assert.AreEqual(4, boxes.Count, "box count before remove");

          foreach (var box in boxes) {
            m4a.Remove(box);
          }

          Assert.AreEqual(0, (new List<Box>(m4a.Boxes)).Count, "box count after remove");

          m4a.Append(boxes[3]); // moov
          m4a.Append(boxes[1]); // free
          m4a.Append(boxes[0]); // ftyp
          m4a.Append(boxes[2]); // mdat

          m4a.Save();
        }

        Assert.AreNotEqual(File.ReadAllBytes(origin), File.ReadAllBytes(file));

        using (var restructured = new MediaFile(file)) {
          TestContent(restructured, false);
        }
      }
      finally {
        if (File.Exists(file))
          File.Delete(file);
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSaveOverwriteNotWritableStream()
    {
      var origin = "test.m4a";
      var file = "test-overwrite.m4a";

      try {
        File.Copy(origin, file, true);

        using (var m4a = new MediaFile(file, false)) {
          m4a.Save();
        }
      }
      finally {
        if (File.Exists(file))
          File.Delete(file);
      }
    }

    [Ignore("not works")]
    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSaveToSameFileStream()
    {
      var origin = "test.m4a";
      var file = "test-saved.m4a";

      try {
        File.Copy(origin, file, true);

        using (var readStream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)) {
          var m4a = new MediaFile(readStream);

          using (var writeStream = new FileStream(file, FileMode.Open, FileAccess.Write, FileShare.None)) {
            m4a.Save(writeStream);
          }
        }
      }
      finally {
        if (File.Exists(file))
          File.Delete(file);
      }
    }

    [Test]
    public void TestFind()
    {
      var trak = new Standards.Iso.TrackBox();

      trak.Boxes.Add(new Standards.Iso.TrackHeaderBox());

      var moov = new Standards.Iso.MovieBox();

      moov.Boxes.Add(new MovieHeaderBox());
      moov.Boxes.Add(trak);

      var media = new MediaFile(new Box[] {
        new Standards.Iso.FileTypeBox(),
        new Standards.Iso.FreeSpaceBox(),
        moov,
      });

      Assert.IsInstanceOfType(typeof(Standards.Iso.FileTypeBox), media.Find(typeof(Standards.Iso.FileTypeBox)));
      Assert.IsInstanceOfType(typeof(Standards.Iso.FileTypeBox), media.Find("ftyp"));

      Assert.IsInstanceOfType(typeof(Standards.Iso.FreeSpaceBox), media.Find(typeof(Standards.Iso.FreeSpaceBox)));
      Assert.IsInstanceOfType(typeof(Standards.Iso.FreeSpaceBox), media.Find("free"));

      Assert.IsInstanceOfType(typeof(Standards.Iso.MovieBox), media.Find(typeof(Standards.Iso.MovieBox)));
      Assert.IsInstanceOfType(typeof(Standards.Iso.MovieBox), media.Find("moov"));

      Assert.IsInstanceOfType(typeof(Standards.Iso.MovieHeaderBox), media.Find("moov", "mvhd"));
      Assert.IsInstanceOfType(typeof(Standards.Iso.TrackBox), media.Find("moov", "trak"));
      Assert.IsInstanceOfType(typeof(Standards.Iso.TrackHeaderBox), media.Find("moov", "trak", "tkhd"));
    }

    [Test]
    public void TestFindAll()
    {
      var trak1 = new Standards.Iso.TrackBox();

      trak1.Boxes.Add(new Standards.Iso.TrackHeaderBox());

      var trak2 = new Standards.Iso.TrackBox();

      trak2.Boxes.Add(new Standards.Iso.TrackHeaderBox());

      var trak3 = new Standards.Iso.TrackBox();

      trak3.Boxes.Add(new Standards.Iso.TrackHeaderBox());

      var moov = new Standards.Iso.MovieBox();

      moov.Boxes.Add(new MovieHeaderBox());
      moov.Boxes.Add(trak1);
      moov.Boxes.Add(trak2);
      moov.Boxes.Add(trak3);

      var media = new MediaFile(new Box[] {
        new Standards.Iso.FileTypeBox(),
        new Standards.Iso.FreeSpaceBox(),
        new Standards.Iso.FreeSpaceBox(),
        new Standards.Iso.FreeSpaceBox(),
        moov,
      });

      var foundFtyp = media.FindAll(typeof(Standards.Iso.FileTypeBox));

      Assert.AreEqual(1, foundFtyp.Count);
      CollectionAssert.AllItemsAreInstancesOfType(foundFtyp, typeof(Standards.Iso.FileTypeBox));

      foundFtyp = media.FindAll("ftyp");

      Assert.AreEqual(1, foundFtyp.Count);
      CollectionAssert.AllItemsAreInstancesOfType(foundFtyp, typeof(Standards.Iso.FileTypeBox));

      var foundFree = media.FindAll(typeof(Standards.Iso.FreeSpaceBox));

      Assert.AreEqual(3, foundFree.Count);
      CollectionAssert.AllItemsAreInstancesOfType(foundFree, typeof(Standards.Iso.FreeSpaceBox));

      foundFree = media.FindAll("free");

      Assert.AreEqual(3, foundFree.Count);
      CollectionAssert.AllItemsAreInstancesOfType(foundFree, typeof(Standards.Iso.FreeSpaceBox));

      var foundTkhd = media.FindAll("moov", "trak", "tkhd");

      Assert.AreEqual(3, foundTkhd.Count);
      CollectionAssert.AllItemsAreInstancesOfType(foundTkhd, typeof(Standards.Iso.TrackHeaderBox));
    }

    [Test]
    public void TestFindOrCreate()
    {
      var media = new MediaFile();

      Assert.IsNull(media.Find("moov"));
      Assert.IsInstanceOfType(typeof(Standards.Iso.MovieBox), media.FindOrCreate("moov"));

      Assert.IsNull(media.Find("moov", "trak"));
      Assert.IsInstanceOfType(typeof(Standards.Iso.TrackBox), media.FindOrCreate("moov", "trak"));
    }

    [Test]
    public void TestAppend()
    {
      var media = new MediaFile();

      media.Append(new Standards.Iso.FileTypeBox());

      Assert.IsInstanceOfType(typeof(Standards.Iso.FileTypeBox), media.Find("ftyp"));

      media.Append(new Standards.Iso.FreeSpaceBox());

      Assert.IsInstanceOfType(typeof(Standards.Iso.FreeSpaceBox), media.Find("free"));

      media.Append(new Standards.Iso.MovieBox());

      Assert.IsInstanceOfType(typeof(Standards.Iso.MovieBox), media.Find("moov"));

      var boxes = new List<Box>(media.Boxes);

      Assert.IsInstanceOfType(typeof(Standards.Iso.FileTypeBox), boxes[0]);
      Assert.IsInstanceOfType(typeof(Standards.Iso.FreeSpaceBox), boxes[1]);
      Assert.IsInstanceOfType(typeof(Standards.Iso.MovieBox), boxes[2]);
    }

    [Test]
    public void TestRemove()
    {
      var media = new MediaFile(new Box[] {
        new Standards.Iso.FileTypeBox(),
        new Standards.Iso.FreeSpaceBox(),
        new Standards.Iso.MovieBox(),
      });

      List<Box> boxes;

      boxes = new List<Box>(media.Boxes);

      Assert.AreEqual(3, boxes.Count);
      Assert.IsInstanceOfType(typeof(Standards.Iso.FileTypeBox), boxes[0]);
      Assert.IsInstanceOfType(typeof(Standards.Iso.FreeSpaceBox), boxes[1]);
      Assert.IsInstanceOfType(typeof(Standards.Iso.MovieBox), boxes[2]);

      media.Remove("free");

      boxes = new List<Box>(media.Boxes);

      Assert.AreEqual(2, boxes.Count);
      Assert.IsInstanceOfType(typeof(Standards.Iso.FileTypeBox), boxes[0]);
      Assert.IsInstanceOfType(typeof(Standards.Iso.MovieBox), boxes[1]);

      media.Remove(boxes[0]);

      boxes = new List<Box>(media.Boxes);

      Assert.AreEqual(1, boxes.Count);
      Assert.IsInstanceOfType(typeof(Standards.Iso.MovieBox), boxes[0]);
    }

    [Test]
    public void TestAppendInto()
    {
      var moov = new Standards.Iso.MovieBox();

      var media = new MediaFile(new Box[] {
        new Standards.Iso.FileTypeBox(),
        new Standards.Iso.FreeSpaceBox(),
        moov,
      });

      media.AppendInto(new Standards.Iso.TrackBox(), "moov");
      media.AppendInto(new Standards.Iso.TrackHeaderBox(), "moov", "trak");

      Assert.IsInstanceOfType(typeof(Standards.Iso.TrackHeaderBox), media.Find("moov", "trak", "tkhd"));
    }

    [Test]
    public void TestRemoveFrom()
    {
      var moov = new Standards.Iso.MovieBox();
      var trak = new Standards.Iso.TrackBox();
      var tkhd = new Standards.Iso.TrackHeaderBox();

      moov.Boxes.Add(trak);
      trak.Boxes.Add(tkhd);

      var media = new MediaFile(new Box[] {
        new Standards.Iso.FileTypeBox(),
        new Standards.Iso.FreeSpaceBox(),
        moov,
      });

      Assert.AreEqual(tkhd, media.Find("moov", "trak", "tkhd"));

      media.RemoveFrom(tkhd, "moov", "trak");

      Assert.IsNull(media.Find("moov", "trak", "tkhd"));

      Assert.AreEqual(trak, media.Find("moov", "trak"));

      media.RemoveFrom(trak, "moov");

      Assert.IsNull(media.Find("moov", "trak"));

      Assert.AreEqual(0, moov.Boxes.Count);
    }

    [Test]
    public void TestInsertBefore()
    {
    }

    [Test]
    public void TestInsertAfter()
    {
    }
  }
}
