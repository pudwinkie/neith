// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2009-2011 smdn
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
using System.IO;
using System.Text;
using System.Xml;

using Smdn.Xml;

namespace Smdn.Formats.Playlists {
  internal class XmlShareablePlaylistFormat : PlaylistFormat {
    private static readonly MimeType mimeType = MimeType.CreateApplicationType("xspf+xml");
    private static readonly string xmlNamespaceUri = "http://xspf.org/ns/0/";

    public override string Name {
      get { return "XML Shareable Playlist Format"; }
    }

    public override string DefaultExtension {
      get { return ".xspf"; }
    }

    public override Encoding DefaultEncoding {
      get { return Encoding.UTF8; }
    }

    public override MimeType MimeType {
      get { return mimeType; }
    }

    public override Formatter Format {
      get { return FormatImpl; }
    }

    public override Parser Parse {
      get { return ParseImpl; }
    }

    internal XmlShareablePlaylistFormat()
      : base()
    {
    }

    /*
     * format implementation
     * 
     * references:
     *   http://www.xspf.org/xspf-v1.html
     *   http://en.wikipedia.org/wiki/XML_Shareable_Playlist_Format
     */
    private static Playlist ParseImpl(Stream stream, Encoding encoding)
    {
      var document = new XmlDocument();

      document.Load(stream); // ignore encoding

      var nsmgr = new XmlNamespaceManager(document.NameTable);

      try {
        nsmgr.PushScope();
        nsmgr.AddNamespace("xspf", xmlNamespaceUri);

        /*
         * 4.1.1 playlist
         * 4.1.1.1 attributes
         * 4.1.1.1.1 xmlns
         * 4.1.1.1.2 version
         */
        var playlistNode = document.SelectSingleNode("/xspf:playlist", nsmgr);

        if (playlistNode == null)
          throw new InvalidDataException("stream is not XSPF");

        switch (playlistNode.GetSingleNodeValueOf("@version", nsmgr)) {
          case "0":
          case "1":
            break;

          default:
            throw new NotSupportedException("unsupported version");
        }

        /*
         * 4.1.1 playlist
         * 4.1.1.2 elements
         * 4.1.1.2.1 title          : xspf:playlist elements MAY contain exactly one.
         * 4.1.1.2.2 creator        : xspf:playlist elements MAY contain exactly one.
         * 4.1.1.2.3 annotation     : xspf:playlist elements MAY contain exactly one.
         * 4.1.1.2.4 info           : xspf:playlist elements MAY contain exactly one.
         * 4.1.1.2.5 location       : xspf:playlist elements MAY contain exactly one.
         * 4.1.1.2.6 identifier     : xspf:playlist elements MAY contain exactly one.
         * 4.1.1.2.7 image          : xspf:playlist elements MAY contain exactly one.
         * 4.1.1.2.8 date           : xspf:playlist elements MAY contain exactly one.
         * 4.1.1.2.9 license        : xspf:playlist elements may contain zero or one license element.
         * 4.1.1.2.10 attribution   : xspf:playlist elements MAY contain exactly one.
         * 4.1.1.2.11 link          : xspf:playlist elements MAY contain zero or more link elements.
         * 4.1.1.2.12 meta          : xspf:playlist elements MAY contain zero or more meta elements.
         * 4.1.1.2.13 extension     : xspf:playlist elements MAY contain zero or more extension elements.
         * 4.1.1.2.14 trackList     : xspf:playlist elements MUST contain one and only one trackList element. The trackList element my be empty.
         */
        var trackListNode = playlistNode.SelectSingleNode("xspf:trackList", nsmgr);

        if (trackListNode == null)
          throw new InvalidDataException("/xspf:playlist/xspf:trackList not found");

        var playlist = new Playlist(trackListNode.ConvertNodesTo<PlaylistEntry>("xspf:track", nsmgr, delegate(XmlNode trackNode) {
          /*
           * 4.1.1.2.14.1.1 track
           * 4.1.1.2.14.1.1.1 elements
           * 4.1.1.2.14.1.1.1.1 location
           * 4.1.1.2.14.1.1.1.2 identifier
           * 4.1.1.2.14.1.1.1.3 title
           * 4.1.1.2.14.1.1.1.4 creator
           * 4.1.1.2.14.1.1.1.5 annotation
           * 4.1.1.2.14.1.1.1.6 info
           * 4.1.1.2.14.1.1.1.7 image
           * 4.1.1.2.14.1.1.1.8 album
           * 4.1.1.2.14.1.1.1.9 trackNum
           * 4.1.1.2.14.1.1.1.10 duration
           * 4.1.1.2.14.1.1.1.11 link
           * 4.1.1.2.14.1.1.1.12 meta
           * 4.1.1.2.14.1.1.1.13 extension
           */
          var track = new PlaylistEntry();

          track.Location      = trackNode.GetSingleNodeValueOf<Uri>("xspf:location/text()", nsmgr, PlaylistEntry.ConvertLocationToUri);
          track.Title         = trackNode.GetSingleNodeValueOf("xspf:title/text()", nsmgr);
          track.Duration      = trackNode.GetSingleNodeValueOf<TimeSpan?>("xspf:duration/text()", nsmgr, ToMillisecondsTimespan);
          track.AlbumTitle    = trackNode.GetSingleNodeValueOf("xspf:album/text()", nsmgr);
          track.TrackNumber   = trackNode.GetSingleNodeValueOf<int?>("xspf:trackNum/text()", nsmgr, null, ConvertUtils.ToInt32Nullable);

          return track;
        }));

        playlist.Title        = playlistNode.GetSingleNodeValueOf("xspf:title/text()", nsmgr);
        playlist.Creator      = playlistNode.GetSingleNodeValueOf("xspf:creator/text()", nsmgr);
        playlist.CreationDate = playlistNode.GetSingleNodeValueOf<DateTimeOffset?>("xspf:date/text()", nsmgr, null, DateTimeConvert.FromW3CDateTimeOffsetStringNullable);

        return playlist;
      }
      finally {
        nsmgr.PopScope();
      }
    }

    private static TimeSpan? ToMillisecondsTimespan(string val)
    {
      var milliseconds = ConvertUtils.ToInt32Nullable(val);

      if (milliseconds == null)
        return null;
      else
        return TimeSpan.FromMilliseconds(milliseconds.Value);
    }

    private static void FormatImpl(Playlist playlist, Stream stream, Uri baseUri, Encoding encoding)
    {
      var document = new XmlDocument();

      document.AppendChild(document.CreateXmlDeclaration("1.0", encoding.WebName, null));

      var playlistElement = (XmlElement)document.AppendChild(document.CreateElement("playlist", xmlNamespaceUri));

      playlistElement.Attributes.Append(document.CreateAttribute("version")).Value = "1";

      AppendTextElement(playlistElement, "title", xmlNamespaceUri, playlist.Title);
      AppendTextElement(playlistElement, "creator", xmlNamespaceUri, playlist.Creator);

      if (playlist.CreationDate != null)
        AppendTextElement(playlistElement, "date", xmlNamespaceUri, DateTimeConvert.ToW3CDateTimeString(playlist.CreationDate.Value));

      var trackListElement = (XmlElement)playlistElement.AppendChild(document.CreateElement("trackList", xmlNamespaceUri));

      foreach (var track in playlist.Entries) {
        var trackElement = (XmlElement)trackListElement.AppendChild(document.CreateElement("track", xmlNamespaceUri));

        AppendTextElement(trackElement, "location", xmlNamespaceUri, PlaylistEntry.ConvertLocationToString(track.Location, baseUri, encoding));
        AppendTextElement(trackElement, "title", xmlNamespaceUri, track.Title);

        if (track.Duration != null)
          AppendTextElement(trackElement, "duration", xmlNamespaceUri, XmlConvert.ToString((int)track.Duration.Value.TotalMilliseconds));
        if (track.AlbumTitle != null)
          AppendTextElement(trackElement, "album", xmlNamespaceUri, track.AlbumTitle);
        if (track.TrackNumber != null)
          AppendTextElement(trackElement, "trackNum", xmlNamespaceUri, XmlConvert.ToString(track.TrackNumber.Value));
      }

      var settings = new XmlWriterSettings();

      settings.Encoding = encoding;
      settings.Indent = true;
      settings.IndentChars = "  ";
      settings.NewLineChars = "\n";

      var writer = XmlWriter.Create(stream, settings);

      document.WriteTo(writer);

      writer.Flush();
    }

    private static void AppendTextElement(XmlElement parent, string name, string ns, string @value)
    {
      if (@value != null)
        parent.AppendChild(parent.OwnerDocument.CreateElement(name, ns)).AppendChild(parent.OwnerDocument.CreateTextNode(@value));
    }
  }
}
