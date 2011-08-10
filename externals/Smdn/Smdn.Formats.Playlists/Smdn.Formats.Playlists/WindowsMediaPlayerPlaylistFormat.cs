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
  internal class WindowsMediaPlayerPlaylistFormat : PlaylistFormat {
    private static readonly MimeType mimeType = MimeType.CreateApplicationType("vnd.ms-wpl");

    public override string Name {
      get { return "Windows Media Player Playlist"; }
    }

    public override string DefaultExtension {
      get { return ".wpl"; }
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

    internal WindowsMediaPlayerPlaylistFormat()
      : base()
    {
    }

    /*
     * format implementation
     * 
     * references:
     *   http://gonze.com/playlists/playlist-format-survey.html
     *   http://msdn.microsoft.com/en-us/library/dd564688(VS.85).aspx
     *   http://en.wikipedia.org/wiki/WPL
     */
    private static Playlist ParseImpl(Stream stream, Encoding encoding)
    {
      var document = new XmlDocument();

      document.Load(stream); // ignore encoding

      var playlist = new Playlist(document.ConvertNodesTo<PlaylistEntry>("/smil/body/seq/media", delegate(XmlNode mediaNode) {
        return new PlaylistEntry(PlaylistEntry.ConvertLocationToUri(mediaNode.GetSingleNodeValueOf("@src")));
      }));

      var headElement = document.SelectSingleNode("/smil/head");

      if (headElement != null) {
        playlist.Title = headElement.GetSingleNodeValueOf("title/text()");
        playlist.Author     = headElement.GetSingleNodeValueOf("meta[@name='Author']/@content");
        playlist.Generator  = headElement.GetSingleNodeValueOf("meta[@name='Generator']/@content");
      }

      return playlist;
    }

    private static void FormatImpl(Playlist playlist, Stream stream, Uri baseUri, Encoding encoding)
    {
      var document = new XmlDocument();

      document.AppendChild(document.CreateXmlDeclaration("1.0", encoding.WebName, null));

      var smilElement = (XmlElement)document.AppendChild(document.CreateElement("smil"));
      var headElement = (XmlElement)smilElement.AppendChild(document.CreateElement("head"));
      var bodyElement = (XmlElement)smilElement.AppendChild(document.CreateElement("body"));

      if (playlist.Title != null)
        headElement.AppendChild(document.CreateElement("title")).AppendChild(document.CreateTextNode(playlist.Title));

      AppendMetaElement(headElement, "Author", playlist.Author);
      AppendMetaElement(headElement, "Generator", playlist.Generator);

      var seqElement = (XmlElement)bodyElement.AppendChild(document.CreateElement("seq"));

      foreach (var media in playlist.Entries) {
        var mediaElement = (XmlElement)seqElement.AppendChild(document.CreateElement("media"));

        mediaElement.Attributes.Append(document.CreateAttribute("src")).Value = PlaylistEntry.ConvertLocationToSchemeOmittedString(media.Location, baseUri, encoding);
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

    private static void AppendMetaElement(XmlNode parent, string name, string content)
    {
      if (content == null)
        return;

      var metaElement = parent.AppendChild(parent.OwnerDocument.CreateElement("meta"));

      metaElement.Attributes.Append(parent.OwnerDocument.CreateAttribute("name")).Value = name;
      metaElement.Attributes.Append(parent.OwnerDocument.CreateAttribute("content")).Value = content;
    }
  }
}
