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

namespace Smdn.Formats.Feeds {
  // http://www.brindys.com/winrss/feedformat.html
  // http://en.wikipedia.org/wiki/Feed_URI_scheme
  // http://www.25hoursaday.com/draft-obasanjo-feed-URI-scheme-02.html (deleted?)
  public class FeedUri : Uri {
    public static readonly string UriSchemeFeed = "feed";
    public static readonly string UriSchemeHttpFeed = "feed:http";
    public static readonly string UriSchemeHttpsFeed = "feed:https";
    public static readonly string UriSchemeFtpFeed = "feed:ftp";

    public new string Scheme {
      get
      {
        switch (base.Scheme) {
          case "http":
            return UriSchemeHttpFeed;

          case "https":
            return UriSchemeHttpsFeed;

          case "ftp":
            return UriSchemeFtpFeed;

          default:
            return base.Scheme;
        }
      }
    }

    public FeedUri(Uri uri)
      : this(uri.ToString())
    {
    }

    public FeedUri(string uriString)
      : base(ToInternalSchemeExpression(uriString))
    {
    }

    public Uri ToUrl()
    {
      return new Uri(base.ToString());
    }

    public override string ToString()
    {
      return ToFeedSchemeExpression(base.ToString());
    }

    private static string ToFeedSchemeExpression(string uriString)
    {
      return string.Concat("feed:", uriString);
    }

    private static string ToInternalSchemeExpression(string uriString)
    {
      if (uriString.StartsWith("feed://", StringComparison.OrdinalIgnoreCase)) {
        return uriString.Replace("feed://", "http://");
      }
      else if (uriString.StartsWith("feed:", StringComparison.OrdinalIgnoreCase)) {
        return uriString.Replace("feed:", "");
      }
      else {
        throw new UriFormatException("invalid scheme");
      }
    }
  }
}
