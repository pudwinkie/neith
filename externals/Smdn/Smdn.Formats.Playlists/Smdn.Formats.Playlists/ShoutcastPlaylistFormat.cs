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
  internal class ShoutcastPlaylistFormat : PlaylistFormat {
    private static readonly MimeType mimeType = MimeType.CreateAudioType("x-scpls");

    public override string Name {
      get { return "Shoutcast Playlist"; }
    }

    public override string DefaultExtension {
      get { return ".pls"; }
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

    internal ShoutcastPlaylistFormat()
      : base()
    {
    }

    /*
     * format implementation
     * 
     * references:
     *   http://gonze.com/playlists/playlist-format-survey.html
     *   http://en.wikipedia.org/wiki/PLS_(file_format)
     */
    private static Playlist ParseImpl(Stream stream, Encoding encoding)
    {
      var reader = new StreamReader(stream, encoding);
      var playlistSectionFound = false;
      var numberOfEntriesFound = false;
      var version = string.Empty;
      PlaylistEntry[] entries = null;

      for (;;) {
        var line = reader.ReadLine();

        if (line == null)
          break;
        else if (line.Length == 0)
          continue;

        if (line.StartsWith("#") || line.StartsWith(";"))
          // comment line
          continue;

        line = line.TrimStart();

        if (playlistSectionFound) {
          var delim = line.IndexOf("=");

          if (delim < 0)
            continue;

          var key = line.Substring(0, delim).Trim();
          var val = line.Substring(delim + 1).Trim();

          if (key == "NumberOfEntries") {
            int entryCount;

            if (int.TryParse(val, out entryCount) && 0 <= entryCount)
              entries = new PlaylistEntry[entryCount];
            else
              throw new InvalidDataException(string.Format("invalid NumberOfEntries: {0}", line));

            numberOfEntriesFound = true;
          }
          else if (key == "Version") {
            version = val;
          }
          else {
            if (!numberOfEntriesFound)
              throw new InvalidDataException(string.Format("unexpected {0} before NumberOfEntries", key));

            string keyWithoutIndex = null;

            if (key.StartsWith("File"))
              keyWithoutIndex = "File";
            else if (key.StartsWith("Title"))
              keyWithoutIndex = "Title";
            else if (key.StartsWith("Length"))
              keyWithoutIndex = "Length";

            if (keyWithoutIndex == null)
              // ignore
              continue;

            int index;

            if (!int.TryParse(key.Substring(keyWithoutIndex.Length), out index))
              throw new InvalidDataException(string.Format("invalid index: {0}", line));

            index -= 1; // 1-based

            if (index < 0 || entries.Length <= index)
              throw new InvalidDataException(string.Format("index is out of range: {0}", line));

            if (entries[index] == null)
              entries[index] = new PlaylistEntry();

            switch (keyWithoutIndex) {
              case "File": entries[index].Location = PlaylistEntry.ConvertLocationToUri(val); break;
              case "Title": entries[index].Title = val; break;
              case "Length": {
                int length;

                if (int.TryParse(val, out length)) {
                  if (length == -1)
                    entries[index].Duration = null;
                  else
                    entries[index].Duration = TimeSpan.FromSeconds(length);
                }

                break;
              }
            }
          }
        }
        else {
          if (line.ToLowerInvariant().StartsWith("[playlist]"))
            playlistSectionFound = true;
        }
      } // for

      if (!playlistSectionFound)
        throw new InvalidDataException("playlist section not found");

      if (!string.IsNullOrEmpty(version) && version != "2")
        throw new NotSupportedException();

      return new Playlist(entries);
    }

    private static void FormatImpl(Playlist playlist, Stream stream, Uri baseUri, Encoding encoding)
    {
      var writer = new StreamWriter(stream, encoding);

      writer.WriteLine("[playlist]");
      writer.WriteLine("Version=2"); // XXX: here?
      writer.WriteLine("NumberOfEntries={0}", playlist.Entries.Count);

      for (int i = 0, entryNumber = 1 /* 1-based */; i < playlist.Entries.Count; i++, entryNumber++) {
        var entry = playlist.Entries[i];

        writer.WriteLine();
        writer.WriteLine("File{0}={1}", entryNumber, PlaylistEntry.ConvertLocationToSchemeOmittedString(entry.Location, baseUri, encoding));

        if (entry.Title != null)
          writer.WriteLine("Title{0}={1}", entryNumber, entry.Title);

        if (entry.Duration != null)
          writer.WriteLine("Length{0}={1}", entryNumber, (int)entry.Duration.Value.TotalSeconds);
      }

      writer.Flush();
    }
  }
}
