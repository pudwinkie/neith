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
using System.Threading;

using Smdn.Net.Pop3.Protocol;

namespace Smdn.Net.Pop3 {
  /*
   * RFC 2384 POP URL Scheme
   * http://tools.ietf.org/html/rfc2384
   */
  public static class PopUri {
    public static readonly string UriSchemePop  = "pop";
    public static readonly string UriSchemePops = "pops";

    private static int registered = 0;

    public static void RegisterParser()
    {
      if (0 == Interlocked.Exchange(ref registered, 1)) {
        UriParser.Register(new PopStyleUriParser(), UriSchemePop,  PopDefaultPorts.Pop);
        UriParser.Register(new PopStyleUriParser(), UriSchemePops, PopDefaultPorts.Pops);
      }
    }

    public static bool IsPop(Uri uri)
    {
      if (uri == null)
        throw new ArgumentNullException("uri");

      if (string.Equals(uri.Scheme, UriSchemePop, StringComparison.OrdinalIgnoreCase))
        return true;
      else if (string.Equals(uri.Scheme, UriSchemePops, StringComparison.OrdinalIgnoreCase))
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
      if (string.Equals(scheme, UriSchemePop, StringComparison.OrdinalIgnoreCase))
        return PopDefaultPorts.Pop;
      else if (string.Equals(scheme, UriSchemePops, StringComparison.OrdinalIgnoreCase))
        return PopDefaultPorts.Pops;
      else
        throw new ArgumentException(string.Format("scheme must be {0} or {1}", UriSchemePop, UriSchemePops), "scheme");
    }
  }
}
