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

namespace Smdn.Formats.Playlists {
  public class Playlist {
    public static Playlist LoadFrom(string file)
    {
      return LoadFrom(file, (Encoding)null);
    }

    public static Playlist LoadFrom(string file, Encoding encoding)
    {
      return LoadFrom(file, PlaylistFormat.GuessFormatFromExtension(file, true), encoding);
    }

    public static Playlist LoadFrom(string file, PlaylistFormat format)
    {
      if (format == null)
        throw new ArgumentNullException("format");

      return LoadFrom(file, format, format.DefaultEncoding);
    }

    public static Playlist LoadFrom(string file, PlaylistFormat format, Encoding encoding)
    {
      if (format == null)
        throw new ArgumentNullException("format");

      using (var stream = File.OpenRead(file)) {
        return LoadFrom(stream, format, format.DefaultEncoding);
      }
    }

    public static Playlist LoadFrom(Stream stream, PlaylistFormat format)
    {
      if (format == null)
        throw new ArgumentNullException("format");

      return LoadFrom(stream, format, format.DefaultEncoding);
    }

    public static Playlist LoadFrom(Stream stream, PlaylistFormat format, Encoding encoding)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");
      if (format == null)
        throw new ArgumentNullException("format");

      var parser = format.Parse;

      if (parser == null)
        throw new NotImplementedException("parsing is not implemented yet");

      return parser(stream, encoding ?? format.DefaultEncoding);
    }

    /*
     * instance members
     */
    public string Title {
      get; set;
    }

    public string Author {
      get; set;
    }

    public string Generator {
      get; set;
    }

    public string Creator {
      get; set;
    }

    public DateTimeOffset? CreationDate {
      get; set;
    }

    public IList<PlaylistEntry> Entries {
      get; set;
    }

    public Playlist()
      : this(new PlaylistEntry[] {})
    {
    }

    public Playlist(IEnumerable<PlaylistEntry> entries)
    {
      this.Entries = new List<PlaylistEntry>(entries);
    }

    public void Save(string file)
    {
      Save(file, (Uri)null);
    }

    public void Save(string file, Uri baseUri)
    {
      Save(file, baseUri, (Encoding)null);
    }

    public void Save(string file, PlaylistFormat format)
    {
      Save(file, format, (Uri)null);
    }

    public void Save(string file, PlaylistFormat format, Uri baseUri)
    {
      if (format == null)
        throw new ArgumentNullException("format");

      Save(file, format, baseUri, format.DefaultEncoding);
    }

    public void Save(string file, Encoding encoding)
    {
      Save(file, (Uri)null, encoding);
    }

    public void Save(string file, Uri baseUri, Encoding encoding)
    {
      Save(file, PlaylistFormat.GuessFormatFromExtension(file, true), baseUri, encoding);
    }

    public void Save(string file, PlaylistFormat format, Encoding encoding)
    {
      Save(file, format, null, encoding);
    }

    public void Save(string file, PlaylistFormat format, Uri baseUri, Encoding encoding)
    {
      using (var stream = File.OpenWrite(file)) {
        stream.SetLength(0L);

        Save(stream, format, baseUri, format.DefaultEncoding);
      }
    }

    public void Save(Stream stream, PlaylistFormat format)
    {
      Save(stream, format, (Uri)null);
    }

    public void Save(Stream stream, PlaylistFormat format, Encoding encoding)
    {
      Save(stream, format, (Uri)null, encoding);
    }

    public void Save(Stream stream, PlaylistFormat format, Uri baseUri)
    {
      if (format == null)
        throw new ArgumentNullException("format");

      Save(stream, format, format.DefaultEncoding);
    }

    public void Save(Stream stream, PlaylistFormat format, Uri baseUri, Encoding encoding)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");
      if (format == null)
        throw new ArgumentNullException("format");

      var formatter = format.Format;

      if (formatter == null)
        throw new NotImplementedException("formatting is not implemented yet");

      formatter(this, stream, baseUri, encoding ?? format.DefaultEncoding);
    }
  }
}
