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
using System.Text;

namespace Smdn.Formats.Playlists {
  public class PlaylistEntry {
    internal static Uri ConvertLocationToUri(string location)
    {
      if (location == null)
        return null;
      else
        return new Uri(location);
    }

    internal static string ConvertLocationToString(Uri location, Uri baseUri, Encoding encoding)
    {
      if (location == null) {
        return null;
      }
      else {
        // TODO: encoding
        if (baseUri == null) {
          if (location.IsAbsoluteUri)
            return location.AbsoluteUri;
          else
            return Uri.EscapeUriString(location.ToString());
        }

        try {
          return baseUri.MakeRelativeUri(location).ToString();
        }
        catch (UriFormatException) {
          return location.AbsoluteUri;
        }
      }
    }

    internal static string ConvertLocationToSchemeOmittedString(Uri location, Uri baseUri, Encoding encoding)
    {
      if (location == null) {
        return null;
      }
      else if (!location.IsAbsoluteUri) {
        return location.ToString();
      }
      else if (location.IsFile) {
        if (baseUri == null)
          return location.LocalPath;

        try {
          return baseUri.MakeRelativeUri(location).ToString();
        }
        catch (UriFormatException) {
          return location.LocalPath;
        }
      }
      else {
        // TODO: encoding
        if (baseUri == null)
          return location.AbsoluteUri;
        else
          return baseUri.MakeRelativeUri(location).AbsoluteUri;
      }
    }

    public Uri Location {
      get; set;
    }

    public string FileLocation {
      get
      {
        if (Location == null)
          throw new InvalidOperationException("Location is null");
        else if (Location.IsFile)
          return Location.LocalPath;
        else
          throw new InvalidOperationException("Location is not file");
      }
    }

    public TimeSpan? Duration {
      get; set;
    }

    public string Title {
      get; set;
    }

    public string AlbumTitle {
      get; set;
    }

    public int? TrackNumber {
      get; set;
    }

    public PlaylistEntry()
      : this(null)
    {
    }

    public PlaylistEntry(Uri location)
    {
      this.Location = location;
    }
  }
}
