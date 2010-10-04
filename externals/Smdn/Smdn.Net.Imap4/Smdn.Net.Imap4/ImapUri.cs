// 
// Author:
//       smdn <smdn@mail.invisiblefulmoon.net>
// 
// Copyright (c) 2008-2010 smdn
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
using System.Threading;

using Smdn.Net.Imap4.Protocol;

namespace Smdn.Net.Imap4 {
  /*
   * RFC 5092 - IMAP URL Scheme
   * http://tools.ietf.org/html/rfc5092
   */
  public static class ImapUri {
    public static readonly string UriSchemeImap  = "imap";
    public static readonly string UriSchemeImaps = "imaps";

    private static int registered = 0;

    public static void RegisterParser()
    {
      if (0 == Interlocked.Exchange(ref registered, 1)) {
        UriParser.Register(new ImapStyleUriParser(), UriSchemeImap,  ImapDefaultPorts.Imap);
        UriParser.Register(new ImapStyleUriParser(), UriSchemeImaps, ImapDefaultPorts.Imaps);
      }
    }

    public static bool IsImap(Uri uri)
    {
      if (uri == null)
        throw new ArgumentNullException("uri");

      if (string.Equals(uri.Scheme, UriSchemeImap, StringComparison.OrdinalIgnoreCase))
        return true;
      else if (string.Equals(uri.Scheme, UriSchemeImaps, StringComparison.OrdinalIgnoreCase))
        return true;
      else
        return false;
    }

    public static int GetDefaultPortFromScheme(Uri uri)
    {
      if (uri == null)
        throw new ArgumentNullException("uri");

      return GetDefaultPortFromScheme(uri.Scheme);
    }

    public static int GetDefaultPortFromScheme(string scheme)
    {
      if (string.Equals(scheme, UriSchemeImap, StringComparison.OrdinalIgnoreCase))
        return ImapDefaultPorts.Imap;
      else if (string.Equals(scheme, UriSchemeImaps, StringComparison.OrdinalIgnoreCase))
        return ImapDefaultPorts.Imaps;
      else
        throw new ArgumentException(string.Format("scheme must be {0} or {1}", UriSchemeImap, UriSchemeImaps), "scheme");
    }
  }
}
