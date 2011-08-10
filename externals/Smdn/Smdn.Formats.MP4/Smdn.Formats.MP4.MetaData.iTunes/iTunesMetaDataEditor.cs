// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2008-2011 smdn
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;

using Smdn.Formats.IsoBaseMediaFile;
using Smdn.Formats.IsoBaseMediaFile.Vendors.Apple.iTunes;
using Smdn.Formats.IsoBaseMediaFile.Standards.Iso;

namespace Smdn.Formats.MP4.MetaData.iTunes {
  public class iTunesMetaDataEditor : MetaDataEditor {
    public override bool HasMetaData {
      get { return FindContainer() != null; }
    }

    public string AlbumArtist {
      get { return GetString<AlbumArtistAtom>(); }
      set { SetString<AlbumArtistAtom>(value); }
    }

    public string Album {
      get { return GetString<AlbumAtom>(); }
      set { SetString<AlbumAtom>(value); }
    }

    public string Artist {
      get { return GetString<ArtistAtom>(); }
      set { SetString<ArtistAtom>(value); }
    }

    public List<ArtworkBase> Artworks {
      get { return artworks; }
    }

    public ushort Bpm {
      get { return GetUInt16<BpmAtom>(0) ?? 0; }
      set { SetUInt16<BpmAtom>(new[] {@value}); }
    }

    public string Category {
      get { return GetString<CategoryAtom>(); }
      set { SetString<CategoryAtom>(value); }
    }

    public string Comment {
      get { return GetString<CommentAtom>(); }
      set { SetString<CommentAtom>(value); }
    }

    public string Composer {
      get { return GetString<ComposerAtom>(); }
      set { SetString<ComposerAtom>(value); }
    }

    public string Copyright {
      get { return GetString<CopyrightAtom>(); }
      set { SetString<CopyrightAtom>(value); }
    }

    public string Description {
      get { return GetString<DescriptionAtom>(); }
      set { SetString<DescriptionAtom>(value); }
    }

    public ushort DiskNumber {
      get { return GetUInt16<DiskNumberAtom>(1) ?? 0; }
      set { SetUInt16<DiskNumberAtom>(new ushort[] {0, @value, GetUInt16<DiskNumberAtom>(2) ?? 0}); }
    }

    public ushort DiskCount {
      get { return GetUInt16<DiskNumberAtom>(2) ?? 0; }
      set { SetUInt16<DiskNumberAtom>(new ushort[] {0, GetUInt16<DiskNumberAtom>(1) ?? 0, @value}); }
    }

    public string Encoder {
      get { return GetString<EncoderAtom>(); }
      set { SetString<EncoderAtom>(value); }
    }

    public bool GaplessPlayback {
      get { return (GetByte<GaplessPlaybackAtom>(0) ?? 0) != 0; }
      set { SetBytes<GaplessPlaybackAtom>(new [] {@value ? (byte)1 : (byte)0}); }
    }

    public string Genre {
      get { return GetString<GenreAtom>(); }
      set { SetString<GenreAtom>(value); }
    }

    public byte GenreNumber {
      get { return GetByte<GenreNumberAtom>(0) ?? 0; }
      set { SetBytes<GenreNumberAtom>(new[] {@value}); }
    }

    public string Grouping {
      get { return GetString<GroupingAtom>(); }
      set { SetString<GroupingAtom>(value); }
    }

    public string Keyword {
      get { return GetString<KeywordAtom>(); }
      set { SetString<KeywordAtom>(value); }
    }

    public string Lyrics {
      get { return GetString<LyricsAtom>(); }
      set { SetString<LyricsAtom>(value); }
    }

    public string Title {
      get { return GetString<TitleAtom>(); }
      set { SetString<TitleAtom>(value); }
    }

    public ushort TrackNumber {
      get { return GetUInt16<TrackNumberAtom>(1) ?? 0; }
      set { SetUInt16<TrackNumberAtom>(new ushort[] {0, @value, GetUInt16<TrackNumberAtom>(2) ?? 0, 0}); }
    }

    public ushort TrackCount {
      get { return GetUInt16<TrackNumberAtom>(2) ?? 0; }
      set { SetUInt16<TrackNumberAtom>(new ushort[] {0, GetUInt16<TrackNumberAtom>(1) ?? 0, @value, 0}); }
    }

    public string ReleaseDate {
      get { return GetString<ReleaseDateAtom>(); }
      set { SetString<ReleaseDateAtom>(value); }
    }

    public iTunesMetaDataEditor(string mediaFile)
      : base(mediaFile)
    {
      GetArtworks();
    }

    public iTunesMetaDataEditor(string mediaFile, bool openAsWritable)
      : base(mediaFile, openAsWritable)
    {
      GetArtworks();
    }

    public iTunesMetaDataEditor(MediaFile mediaFile)
      : base(mediaFile)
    {
      GetArtworks();
    }

    public iTunesMetaDataEditor(MetaDataEditor editor)
      : base(editor)
    {
      GetArtworks();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing) {
        foreach (var artwork in artworks) {
          artwork.Dispose();
        }

        artworks.Clear();
      }

      base.Dispose(disposing);
    }

    private string GetString<TAtom>() where TAtom : MetaDataAtomBase
    {
      var data = FindAtomData<TAtom, TextDataAtom>();

      if (data == null)
        return null;
      else
        return data.Text;
    }

    private void SetString<TAtom>(string @value) where TAtom : MetaDataAtomBase, new()
    {
      if (@value == null)
        RemoveAtom<TAtom>();
      else
        FindOrCreateAtomData<TAtom, TextDataAtom>().Text = @value;
    }

    private ushort? GetUInt16<TAtom>(int index) where TAtom : MetaDataAtomBase
    {
      var values = GetBytes<TAtom>();

      if (values == null)
        return null;
      else if (values.Count <= index * 2 + 1)
        return null;
      else
        return (ushort)(values[index * 2] << 8 | values[index * 2 + 1]);
    }

    private byte? GetByte<TAtom>(int index) where TAtom : MetaDataAtomBase
    {
      var values = GetBytes<TAtom>();

      if (values == null)
        return null;
      else if (values.Count <= index)
        return null;
      else
        return values[index];
    }

    private List<byte> GetBytes<TAtom>() where TAtom : MetaDataAtomBase
    {
      if (typeof(TAtom) == typeof(BpmAtom)) {
        var data = FindAtomData<TAtom, BpmDataAtom>();

        if (data == null)
          return null;
        else
          return data.Values;
      }
      else {
        var data = FindAtomData<TAtom, IntDataAtom>();

        if (data == null)
          return null;
        else
          return data.Values;
      }
    }

    private void SetUInt16<TAtom>(IEnumerable<ushort> values) where TAtom : MetaDataAtomBase, new()
    {
      var bytes = new List<byte>();

      foreach (var val in values) {
        bytes.Add((byte)(val >> 8));
        bytes.Add((byte)(val & 0xff));
      }

      SetBytes<TAtom>(bytes);
    }

    private void SetBytes<TAtom>(IEnumerable<byte> values) where TAtom : MetaDataAtomBase, new()
    {
      var allzero = true;

      foreach (var val in values) {
        if (val != 0) {
          allzero = false;
          break;
        }
      }

      if (allzero) {
        RemoveAtom<TAtom>();
      }
      else {
        List<byte> vals;

        if (typeof(TAtom) == typeof(BpmAtom))
          vals = FindOrCreateAtomData<TAtom, BpmDataAtom>().Values;
        else
          vals = FindOrCreateAtomData<TAtom, IntDataAtom>().Values;

        vals.Clear();
        vals.AddRange(values);
      }
    }

    private void RemoveAtom<TAtom>() where TAtom : MetaDataAtomBase
    {
      var ilst = FindContainer();

      if (ilst == null)
        return;

      var box = Box.Find<TAtom>(ilst);

      if (box == null)
        return;

      ilst.Boxes.Remove(box);
    }

    private MetaDataContainerAtom FindContainer()
    {
      return MediaFile.Find("moov", "udta", "meta", "ilst") as MetaDataContainerAtom;
    }

    private TDataAtom FindAtomData<TAtom, TDataAtom>() where TAtom : MetaDataAtomBase where TDataAtom : DataAtomBase, new()
    {
      var ilst = FindContainer();

      if (ilst == null)
        return null;

      return MediaFile.Find("moov", "udta", "meta", "ilst", KnownBox.GetTypeOf(typeof(TAtom)), "data") as TDataAtom;
    }

    private MetaDataContainerAtom FindOrCreateContainer()
    {
      var ilst = FindContainer();

      if (ilst == null) {
        ilst = new MetaDataContainerAtom();

        MediaFile.FindOrCreate("moov", "udta");
        MediaFile.FindOrCreate<MetaBox>("moov", "udta", "meta").Boxes.Add(ilst);
      }

      return ilst;
    }

    private TDataAtom FindOrCreateAtomData<TAtom, TDataAtom>() where TAtom : MetaDataAtomBase, new() where TDataAtom : DataAtomBase, new()
    {
      var ilst = FindOrCreateContainer();

      var atom = Box.Find<TAtom>(ilst);

      if (atom == null) {
        atom = new TAtom();

        ilst.Boxes.Add(atom);
      }

      var dataAtom = Box.Find<TDataAtom>(atom);

      if (!(dataAtom is TDataAtom)) {
        dataAtom = new TDataAtom();

        atom.Boxes.Add(dataAtom);
      }

      return dataAtom;
    }

    private void UpdateMetaHandler()
    {
      var meta = MediaFile.Find("moov", "udta", "meta") as MetaBox;

      if (meta == null)
        return;

      meta.Handler = new HandlerBox();
      meta.Handler.HandlerType = "mdir";
      meta.Handler.reserved[0] = (uint)(new FourCC("appl")).ToInt32BigEndian();
      meta.Handler.reserved[1] = 0;
      meta.Handler.reserved[2] = 0;
      meta.Handler.Name = "\0\0";
    }

    private void GetArtworks()
    {
      artworks.Clear();

      var ilst = FindContainer();

      if (ilst == null)
        return;

      var artworkAtom = Box.Find<ArtworkAtom>(ilst);

      if (artworkAtom == null)
        return;

      foreach (var data in Box.FindAll(artworkAtom, typeof(DataAtomBase))) {
        if (data is JpegDataAtom)
          artworks.Add(new JpegArtwork(data as JpegDataAtom));
        else if (data is PngDataAtom)
          artworks.Add(new PngArtwork(data as PngDataAtom));
      }
    }

    private void SetArtworks()
    {
      var ilst = FindOrCreateContainer();
      var artworkAtom = Box.Find<ArtworkAtom>(ilst);

      if (artworks.Count == 0) {
        // no artworks
        if (artworkAtom != null)
          ilst.Boxes.Remove(artworkAtom);
        return;
      }

      if (artworkAtom == null) {
        artworkAtom = new ArtworkAtom();

        ilst.Boxes.Add(artworkAtom);
      }
      else {
        artworkAtom.Boxes.Clear();
      }

      artworks.ForEach(delegate(ArtworkBase artwork) {
        if (artwork is JpegArtwork)
          artworkAtom.Boxes.Add(new JpegDataAtom(artwork.ImageStream));
        else if (artwork is PngArtwork)
          artworkAtom.Boxes.Add(new PngDataAtom(artwork.ImageStream));
      });
    }

    public override void UpdateBoxes()
    {
      SetArtworks();

      if (HasMetaData)
        UpdateMetaHandler();
    }

    public override void RemoveMetaDataBoxes()
    {
      artworks.Clear();

      var moov = MediaFile.Find("moov") as MovieBox;

      if (moov == null)
        return;

      var udta = Box.Find<UserDataBox>(moov);

      if (udta == null)
        return;

      var meta = Box.Find<MetaBox>(udta);

      if (meta == null)
        return;

      var ilst = Box.Find<MetaDataContainerAtom>(meta);

      if (ilst == null)
        return;

      // remove 'ilst' from 'meta'
      meta.Boxes.Remove(ilst);

      // check boxes contained in 'meta'
      var containsBox = false;

      foreach (var box in meta.Boxes) {
        if (!(box is FreeSpaceBox)) {
          containsBox = true;
          break;
        }
      }

      if (containsBox)
        // 'meta' contains other boxes, so keep them
        return;

      // 'meta' contains only 'hdlr' and/or 'free', so remove 'meta' from 'udta'
      udta.Boxes.Remove(meta);

      if (udta.Boxes.Count == 0)
        // 'udta' contains no boxes, so remove it
        moov.Boxes.Remove(udta);
    }

    private readonly List<ArtworkBase> artworks = new List<ArtworkBase>();
  }
}
