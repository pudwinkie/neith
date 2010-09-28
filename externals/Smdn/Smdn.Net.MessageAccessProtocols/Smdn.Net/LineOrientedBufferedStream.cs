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

using Smdn.Formats;

namespace Smdn.Net {
  public sealed class LineOrientedBufferedStream : Smdn.IO.StrictLineOrientedStream {
    public override bool CanSeek {
      get { return false; }
    }

    public override long Position {
      get { throw new NotSupportedException(); }
      set { throw new NotSupportedException(); }
    }

    public override long Length {
      get { throw new NotSupportedException(); }
    }

    public LineOrientedBufferedStream(Stream stream)
      : base(stream, 8192)
    {
      this.buffer = new byte[8192];
    }

    protected override void Dispose(bool disposing)
    {
      buffer = null;
    }

    public override void SetLength(long value)
    {
      throw new NotSupportedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      throw new NotSupportedException();
    }

    public override void Flush()
    {
      RejectDisposed();

      // do nothing
    }

    public void WriteToCrLf(byte[] src)
    {
      RejectDisposed();

      if (src == null)
        throw new ArgumentNullException("src");

      var offset = 0;
      var count = src.Length;

      if (buffer.Length - bufferCount < count) {
        // flush buffered
        base.Write(buffer, 0, bufferCount);

        bufferCount = 0;

        // write immediately
        for (;;) {
          if (count < buffer.Length)
            break;

          base.Write(src, offset, buffer.Length);

          offset  += buffer.Length;
          count   -= buffer.Length;
        }
      }

      if (bufferCount == 0 &&
          2 <= count && src[offset + count - 2] == Octets.CR && src[offset + count - 1] == Octets.LF) {
        // write to CRLF
        base.Write(src, offset, count);
      }
      else {
        // write into buffer
        Buffer.BlockCopy(src, offset, buffer, bufferCount, count);

        bufferCount += count;

        if (2 <= bufferCount && buffer[bufferCount - 2] == Octets.CR && buffer[bufferCount - 1] == Octets.LF) {
          // write to CRLF
          base.Write(buffer, 0, bufferCount);

          bufferCount = 0;
        }
      }
    }

    public void Write(Stream sourceStream)
    {
      RejectDisposed();

      if (sourceStream == null)
        throw new ArgumentNullException("sourceStream");

      if (0 < bufferCount) {
        // flush buffered
        base.Write(buffer, 0, bufferCount);

        bufferCount = 0;
      }

      for (;;) {
        var read = sourceStream.Read(buffer, 0, buffer.Length);

        base.Write(buffer, 0, read);

        if (read <= 0)
          break;
      }
    }

    public override void Write(byte[] src, int offset, int count)
    {
      throw new NotSupportedException();
    }

    private void RejectDisposed()
    {
      if (buffer == null)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private byte[] buffer = null;
    private int bufferCount;
  }
}