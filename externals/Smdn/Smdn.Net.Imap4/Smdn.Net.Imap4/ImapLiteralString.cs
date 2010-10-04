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
using System.IO;
using System.Text;

namespace Smdn.Net.Imap4 {
  public class ImapLiteralString : ImapString, IImapLiteralString {
    public Encoding Charset {
      get { return charset; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("Charset");
        charset = value;
      }
    }

    public ImapLiteralOptions Options {
      get; set;
    }

    public ImapLiteralString(string val)
      : this(val, NetworkTransferEncoding.Transfer8Bit, ImapLiteralOptions.Default)
    {
    }

    public ImapLiteralString(string val, Encoding charset)
      : this(val, charset, ImapLiteralOptions.Default)
    {
    }

    public ImapLiteralString(string val, Encoding charset, ImapLiteralOptions options)
      : base(val)
    {
      this.Charset = charset;
      this.Options = options;
    }

    Stream IImapLiteralString.GetLiteralStream()
    {
      return new MemoryStream(charset.GetBytes(base.Value));
    }

    public override string ToString()
    {
      return string.Format("{{{0}}} {1}",
                           base.Value.Length,
                           base.Value);
    }

    private Encoding charset;
  }
}