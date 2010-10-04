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
using System.Security.Cryptography;

namespace Smdn.Formats.Mime.Encode {
  public abstract class ContentEncodingStream : Stream {
    public override bool CanSeek {
      get { return false; }
    }

    public override bool CanRead {
      get { return false; }
    }

    public override bool CanWrite {
      get { return true; }
    }

    public override bool CanTimeout {
      get { return stream.CanTimeout; }
    }

    public override long Position {
      get { throw new NotSupportedException(); }
      set { throw new NotSupportedException(); }
    }

    public override long Length {
      get { throw new NotSupportedException(); }
    }

    public byte[] EOL {
      get { return eol; }
    }

    protected Stream InnerStream {
      get { return stream; }
    }

    protected int LineLength {
      get { return lineLength; }
      set { lineLength = value; }
    }

    protected ContentEncodingStream(Stream stream, ICryptoTransform transform, byte[] eolPrependBytes, MimeFormat format)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");
      if (!stream.CanWrite)
        throw new ArgumentException("stream must be writable", "stream");
      if (format == null)
        throw new ArgumentNullException("format");

      var bufferBlockSize = transform.CanTransformMultipleBlocks
        ? (folding == 0 ? 128 : folding)
        : 1;

      this.transform = transform;
      this.folding = format.Folding;
      this.outputBuffer = new byte[bufferBlockSize * transform.OutputBlockSize];
      this.inputBuffer  = new byte[bufferBlockSize * transform.InputBlockSize];
      this.inputCount = 0;
      this.stream = stream;
      this.eol = format.GetEOLBytes();
      this.eolPrependBytes = eolPrependBytes;
    }

    public override void Close()
    {
      FlushFinalBlock();

      base.Close();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing) {
        if (stream != null)
          stream.Close();
        if (transform != null)
          transform.Dispose();
      }

      stream = null;
      transform = null;
      inputBuffer = null;
      outputBuffer = null;
    }

    public override void SetLength(long @value)
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

      stream.Flush();
    }

    public virtual void FlushFinalBlock()
    {
      CheckDisposed();

      if (0 < inputCount) {
        var transformed = transform.TransformFinalBlock(inputBuffer, 0, inputCount);

        stream.Write(transformed, 0, transformed.Length);

        inputCount = 0;
      }

      stream.Flush();
    }

    public override int Read(byte[] dest, int offset, int count)
    {
      throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      CheckDisposed();

      if (buffer == null)
        throw new ArgumentNullException("buffer");
      if (offset < 0)
        throw new ArgumentOutOfRangeException("offset", offset, "must be greater than or equals to 0");
      if (count < 0)
        throw new ArgumentOutOfRangeException("count", count, "must be greater than or equals to 0");
      if (buffer.Length < offset + count)
        throw new ArgumentException("invalid range");

      if (0 < folding && folding < lineLength + transform.OutputBlockSize)
        WriteEOL();

      if (0 < inputCount) {
        var c = Math.Min(count, transform.InputBlockSize - inputCount);

        Buffer.BlockCopy(buffer, offset, inputBuffer, inputCount, c);

        var len = transform.TransformBlock(inputBuffer, 0, inputCount, outputBuffer, 0);

        count     -= c;
        offset    += c;
        inputCount = 0;

        if (0 < len) {
          stream.Write(outputBuffer, 0, len);

          lineLength += len;
        }

        if (count <= 0)
          return;
      }

      /*
      if (transform.CanTransformMultipleBlocks) {
        // TODO: impl
        transform.TransformBlock();
        if (count <= 0)
          return;
      }
      */

      for (;;) {
        if (0 < folding && folding < lineLength + transform.OutputBlockSize + eolPrependBytes.Length)
          WriteEOL();

        if (transform.InputBlockSize <= count) {
          var len = transform.TransformBlock(buffer, offset, transform.InputBlockSize, outputBuffer, 0);

          count   -= transform.InputBlockSize;
          offset  += transform.InputBlockSize;

          if (0 < len) {
            stream.Write(outputBuffer, 0, len);

            lineLength += len;
          }
        }
        else {
          Buffer.BlockCopy(buffer, offset, inputBuffer, 0, count);

          inputCount = count;

          return;
        }
      } // for
    }

    private void WriteEOL()
    {
      stream.Write(eolPrependBytes, 0, eolPrependBytes.Length);
      stream.Write(eol, 0, eol.Length);

      lineLength = 0;
    }

    private void CheckDisposed()
    {
      if (stream == null)
        throw new ObjectDisposedException(GetType().Name);
    }

    private ICryptoTransform transform;
    private byte[] outputBuffer;
    private byte[] inputBuffer;
    private int inputCount;
    private Stream stream;
    private /*readonly*/ byte[] eol;
    private /*readonly*/ byte[] eolPrependBytes;
    private readonly int folding;
    private int lineLength;
  }
}
