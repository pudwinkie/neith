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
using System.IO.Compression;

namespace Smdn.Net.Imap4.Protocol {
  public class ImapDeflateStream : Stream {
    public Stream InnerStream {
      get { CheckDisposed(); return innerStream; }
    }

    public override bool CanSeek {
      get { CheckDisposed(); return false; }
    }

    public override bool CanRead {
      get { CheckDisposed(); return innerStream.CanRead; }
    }

    public override bool CanWrite {
      get { CheckDisposed(); return innerStream.CanWrite; }
    }

    public override bool CanTimeout {
      get { CheckDisposed(); return innerStream.CanTimeout; }
    }

    public override long Position {
      get { throw new NotSupportedException(); }
      set { throw new NotSupportedException(); }
    }

    public override long Length {
      get { throw new NotSupportedException(); }
    }

    public ImapDeflateStream(Stream innerStream)
    {
      if (innerStream == null)
        throw new ArgumentNullException("innerStream");

      this.innerStream = innerStream;

      readStream = new DeflateStream(innerStream, CompressionMode.Decompress);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing) {
        if (innerStream != null)
          innerStream.Close();
        if (readStream != null)
          readStream.Close();
      }

      innerStream = null;
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
      CheckDisposed();

      // do nothing
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      CheckDisposed();

      if (buffer == null)
        throw new ArgumentNullException("buffer");

      if (!CanRead)
        throw new NotSupportedException("stream is not readable");

#if false
      //return readStream.Read(buffer, offset, count);
#else
      throw new NotImplementedException();
#endif
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      CheckDisposed();

      if (!CanWrite)
        throw new NotSupportedException("stream is not writable");

#if false
      using (var w = new DeflateStream(innerStream, CompressionMode.Compress, true)) {
        w.Write(buffer, offset, count);
      }
#else
      throw new NotImplementedException();
#endif
    }

    private void CheckDisposed()
    {
      if (innerStream == null)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private Stream innerStream;
    private Stream readStream;
  }
}
