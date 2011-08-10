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
  internal class M3UPlaylistFormat : PlaylistFormat {
    private static readonly MimeType mimeType = MimeType.CreateAudioType("x-mpegurl");

    public override string Name {
      get { return "M3U"; }
    }

    public override string DefaultExtension {
      get { return ".m3u"; }
    }

    public override Encoding DefaultEncoding {
      get { return Encoding.Default; }
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

    internal M3UPlaylistFormat()
      : base()
    {
    }

    /*
     * format implementation
     * 
     * references:
     *   http://gonze.com/playlists/playlist-format-survey.html
     *   http://en.wikipedia.org/wiki/M3U
     *   http://schworak.com/programming/music/playlist_m3u.asp
     */
    private static Playlist ParseImpl(Stream stream, Encoding encoding)
    {
      var reader = new StreamReader(stream, encoding);

      var entries = new List<PlaylistEntry>();
      var firstLine = true;
      var extended = false;
      PlaylistEntry entry = null;

      for (;;) {
        var line = reader.ReadLine();

        if (line == null)
          break;
        else if (line.Length == 0)
          continue;

        if (firstLine) {
          firstLine = false;

          if (line.StartsWith("#EXTM3U"))
            extended = true;
        }

        if (extended && line.StartsWith("#EXTINF:")) {
          if (entry == null)
            entry = new PlaylistEntry();

          var extinf = line.Substring(8);
          var delim = extinf.IndexOf(",");

          if (0 <= delim) {
            var lengthString = extinf.Substring(0, delim);

            entry.Title = extinf.Substring(delim + 1);

            int length;

            if (int.TryParse(lengthString, out length)) {
              if (length == -1)
                entry.Duration = null;
              else
                entry.Duration = TimeSpan.FromSeconds(length);
            }
          }
        }
        else if (line.StartsWith("#")) {
          // comment line
          continue;
        }
        else {
          var location = PlaylistEntry.ConvertLocationToUri(line.Trim());

          if (entry == null)
            entry = new PlaylistEntry(location);
          else
            entry.Location = location;

          entries.Add(entry);

          entry = null;
        }
      } // for

      return new Playlist(entries);
    }

    private static void FormatImpl(Playlist playlist, Stream stream, Uri baseUri, Encoding encoding)
    {
      var writer = new StreamWriter(stream, encoding);

      // header
      writer.WriteLine("#EXTM3U");

      foreach (var entry in playlist.Entries) {
        if (entry.Duration != null || entry.Title != null)
          writer.WriteLine("#EXTINF:{0},{1}",
                           entry.Duration == null ? -1 : (int)entry.Duration.Value.TotalSeconds,
                           entry.Title);

        writer.WriteLine(PlaylistEntry.ConvertLocationToSchemeOmittedString(entry.Location, baseUri, encoding));
      }

      writer.Flush();
    }
  }
}
