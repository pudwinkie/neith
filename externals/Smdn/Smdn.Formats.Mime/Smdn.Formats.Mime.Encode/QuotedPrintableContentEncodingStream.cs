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

namespace Smdn.Formats.Mime.Encode {
  public class QuotedPrintableContentEncodingStream : ContentEncodingStream {
    public QuotedPrintableContentEncodingStream(Stream stream, MimeFormat format)
      : base(stream, new ToQuotedPrintableTransform(), new byte[] {0x3d}, format)
    {
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      var lastOffset = offset;
      var lastCount = count;

      for (;0 < count; offset++, count--) {
        if (!(buffer[offset] == Octets.CR || buffer[offset] == Octets.LF))
          continue;

        var len = offset - lastOffset;

        if (0 < len)
          base.Write(buffer, lastOffset, len);

        InnerStream.WriteByte(buffer[offset]);

        LineLength = 0;

        lastCount = count - 1;
        lastOffset = offset + 1;
      }

      if (0 < lastCount)
        base.Write(buffer, lastOffset, lastCount);
    }
  }
}
