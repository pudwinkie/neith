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
using System.Collections.Generic;
using System.IO;
using System.Text;

using Smdn.IO;

namespace Smdn.Formats.Playlists {
  public abstract class PlaylistFormat {
    public delegate void Formatter(Playlist playlist, Stream stream, Uri baseUri, Encoding encoding);
    public delegate Playlist Parser(Stream stream, Encoding encoding);

    public static readonly PlaylistFormat ASX = new AdvancedStreamRedirectorFormat();
    public static readonly PlaylistFormat M3U = new M3UPlaylistFormat();
    public static readonly PlaylistFormat PLS = new ShoutcastPlaylistFormat();
    public static readonly PlaylistFormat WPL = new WindowsMediaPlayerPlaylistFormat();
    public static readonly PlaylistFormat XSPF = new XmlShareablePlaylistFormat();

    public static readonly IEnumerable<PlaylistFormat> Formats = new[] {
      ASX,
      M3U,
      PLS,
      WPL,
      XSPF,
    };

    public static PlaylistFormat GuessFormatFromExtension(string pathOrExtension)
    {
      return GuessFormatFromExtension(pathOrExtension, false);
    }

    public static PlaylistFormat GuessFormatFromExtension(string pathOrExtension, bool throwExceptionIfNotSupported)
    {
      if (pathOrExtension == null)
        throw new ArgumentNullException("pathOrExtension");

      foreach (var format in PlaylistFormat.Formats) {
        if (PathUtils.AreExtensionEqual(pathOrExtension, format.DefaultExtension))
          return format;
      }

      if (throwExceptionIfNotSupported)
        throw new NotSupportedException();
      else
        return null;
    }

    public abstract string Name { get; }
    public abstract string DefaultExtension { get; }
    public abstract Encoding DefaultEncoding { get; }
    public abstract MimeType MimeType { get; }
    public abstract Formatter Format { get; }
    public abstract Parser Parse { get; }

    protected PlaylistFormat()
    {
    }

    public override string ToString()
    {
      return Name;
    }
  }
}
