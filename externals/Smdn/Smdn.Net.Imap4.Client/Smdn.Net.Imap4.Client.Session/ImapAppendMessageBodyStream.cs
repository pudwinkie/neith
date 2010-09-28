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
using System.Threading;

using Smdn.IO;

namespace Smdn.Net.Imap4.Client.Session {
  public class ImapAppendMessageBodyStream : Stream {
    public override bool CanSeek {
      get { RejectDisposed(); return false; }
    }

    public override bool CanRead {
      get { RejectDisposed(); return true; }
    }

    public override bool CanWrite {
      get { RejectDisposed(); return true; }
    }

    public override bool CanTimeout {
      get { RejectDisposed(); return true; }
    }

    public override long Position {
      get { RejectDisposed(); return writeOffset; }
      set { RejectDisposed(); writeOffset = value; }
    }

    public override long Length {
      get
      {
        RejectDisposed();
        lengthSetEvent.WaitOne();
        return length;
      }
    }

    public override int WriteTimeout {
      get { RejectDisposed(); return writeTimeout; }
      set
      {
        RejectDisposed();
        if (value < -1)
          throw new ArgumentOutOfRangeException("WriteTimeout", value, "must be greater than or equals to -1");
        writeTimeout = value;
      }
    }

    public override int ReadTimeout {
      get { RejectDisposed(); return readTimeout; }
      set
      {
        RejectDisposed();
        if (value < -1)
          throw new ArgumentOutOfRangeException("ReadTimeout", value, "must be greater than or equals to -1");
        readTimeout = value;
      }
    }

    internal protected ImapAppendMessageBodyStream()
      : this(Timeout.Infinite, Timeout.Infinite)
    {
    }

    internal protected ImapAppendMessageBodyStream(int readWriteTimeout)
      : this(readWriteTimeout, readWriteTimeout)
    {
    }

    internal protected ImapAppendMessageBodyStream(int readTimeout, int writeTimeout)
    {
      this.ReadTimeout  = readTimeout;
      this.WriteTimeout = writeTimeout;

      buffer = new ChunkedMemoryStream();
    }

    public override void Close()
    {
      if (!disposed)
        UpdateLength();

      base.Close();
    }

    internal protected void UpdateLength()
    {
      if (!lengthSetEvent.WaitOne(0, false)) {
        this.length = buffer.Length;

        lengthSetEvent.Set();
      }
    }

    internal protected void InternalDispose()
    {
      if (disposed)
        return;

      Dispose();

      lengthSetEvent.Close();
      lengthSetEvent = null;

      buffer.Close();
      buffer = null;

      disposed = true;
    }

    public override void SetLength(long @value)
    {
      if (lengthSetEvent.WaitOne(0, false))
        throw new InvalidOperationException("length has been set");
      if (@value < 0L)
        throw new ArgumentOutOfRangeException("value", @value, "length must be zero or positive number");

      length = @value;
      lengthSetEvent.Set();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      throw new NotSupportedException();
    }

    public override void Flush()
    {
      // do nothing
    }

    private byte[] singleByteBuffer = new byte[1];

    public override void WriteByte(byte @value)
    {
      singleByteBuffer[0] = @value;

      Write(singleByteBuffer, 0, 1);
    }

    public override void Write(byte[] src, int offset, int count)
    {
      RejectDisposed();

      if (!Monitor.TryEnter(readWriteLock, writeTimeout))
        throw new TimeoutException("write timeout");

      try {
        writeOffset = buffer.Seek(writeOffset, SeekOrigin.Begin);

        buffer.Write(src, offset, count);

        writeOffset += count;

        Monitor.Pulse(readWriteLock);
      }
      finally {
        Monitor.Exit(readWriteLock);
      }

      WaitForReading();
    }

    private void WaitForReading()
    {
      if (lengthSetEvent.WaitOne(0, false) && 40960 < writeOffset - readOffset)
        Thread.Sleep(50);
    }

    public override int Read(byte[] dest, int offset, int count)
    {
      RejectDisposed();

      lengthSetEvent.WaitOne();

      if (readOffset == length)
        return 0;

      var ret = 0;

      for (;;) {
        if (!Monitor.TryEnter(readWriteLock, readTimeout))
          throw new TimeoutException("read timeout");

        try {
          if (readOffset == writeOffset && !Monitor.Wait(readWriteLock, readTimeout))
            throw new TimeoutException("buffer underrun");

          readOffset = buffer.Seek(readOffset, SeekOrigin.Begin);

          var read = buffer.Read(dest,
                                 offset,
                                 (readOffset + count < length)
                                   ? count
                                   : (int)(length - readOffset));

          readOffset  += read;
          count       -= read;
          ret         += read;
        }
        finally {
          Monitor.Exit(readWriteLock);
        }

        if (0 < count && readOffset < length)
          continue;

        return ret;
      }
    }

    private void RejectDisposed()
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private bool disposed = false;
    private long writeOffset  = 0L;
    private long readOffset   = 0L;
    private long length       = 0L;
    private int writeTimeout;
    private int readTimeout;

    private ChunkedMemoryStream buffer;
    private ManualResetEvent lengthSetEvent = new ManualResetEvent(false);
    private object readWriteLock = new object();
  }
}
