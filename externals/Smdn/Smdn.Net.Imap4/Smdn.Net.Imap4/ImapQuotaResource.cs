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

namespace Smdn.Net.Imap4 {
  /*
   * RFC 2087 - IMAP4 QUOTA extension
   * http://tools.ietf.org/html/rfc2087
   */
  public sealed class ImapQuotaResource {
    public string Name {
      get; private set;
    }

    public long Usage {
      get; private set;
    }

    public long Limit {
      get; private set;
    }

    public ImapQuotaResource(string name, long limit)
      : this(name, 0L, limit)
    {
    }

    public ImapQuotaResource(string name, long usage, long limit)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      else if (name.Length == 0)
        throw new ArgumentException("empty name", "name");

      if (usage < 0)
        throw new ArgumentOutOfRangeException("usage", usage, "must be zero or positive number");
      if (limit < 0)
        throw new ArgumentOutOfRangeException("limit", limit, "must be zero or positive number");

      this.Name = name;
      this.Usage = usage;
      this.Limit = limit;
    }

    public override string ToString()
    {
      return string.Format("{{Name={0}, Usage={1}, Limit={2}}}",
                           Name,
                           Usage,
                           Limit);
    }
  }
}
