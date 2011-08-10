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
using System.IO;
using System.Threading;

using Smdn.IO;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Session {
  internal class ImapAppendMessageBodyBuffer : ImapSession.AppendContext {
    public override Stream WriteStream {
      get { return writeStream; }
    }

    internal Stream ReadStream {
      get { return readStream; }
    }

    internal ImapAppendMessageBodyBuffer(ImapSession session,
                                         long? length,
                                         int readTimeout,
                                         int writeTimeout)
    {
      this.session = session;
      this.length = length;
      this.buffer = new ChunkedMemoryStream();

      this.lengthSetEvent = new ManualResetEvent(length.HasValue);

      this.readStream = new _ReadStream(this);
      this.writeStream = new _WriteStream(this);
      this.readTimeout = readTimeout;
      this.writeTimeout = writeTimeout;
    }

    internal void SetAppendAsyncResult(IAsyncResult appendAsyncResult)
    {
      this.appendAsyncResult = appendAsyncResult;
    }

    public override ImapCommandResult GetResult(out ImapAppendedUidSet appendedUidSet)
    {
      if (session == null)
        throw new InvalidOperationException("already finished");

      if (writeStream != null)
        writeStream.Close();

      try {
        return session.EndAppend(appendAsyncResult, out appendedUidSet);
      }
      finally {
        session = null;
        appendAsyncResult = null;

        readStream.Close();
        readStream = null;

        buffer.Close();
        buffer = null;

        lengthSetEvent.Close();
        lengthSetEvent = null;
      }
    }

    private void CloseWriteStream()
    {
      if (writeStream == null)
        return; // already closed

      writeStream = null;

      if (!length.HasValue)
        length = writeOffset;

      lengthSetEvent.Set();
    }

    private long GetLength()
    {
      if (readStream == null)
        throw new ObjectDisposedException(GetType().FullName);

      lengthSetEvent.WaitOne();

      return length.Value;
    }

    private void Flush()
    {
      if (writeStream == null)
        throw new ObjectDisposedException(GetType().FullName);

      // XXX
    }

    private void Write(byte[] src, int offset, int count)
    {
      if (writeStream == null)
        throw new ObjectDisposedException(GetType().FullName);

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

      // wait for reading
      if (lengthSetEvent.WaitOne(0, false) && 40960 < writeOffset - readOffset)
        Thread.Sleep(50);
    }

    private int Read(byte[] dest, int offset, int count)
    {
      if (readStream == null)
        throw new ObjectDisposedException(GetType().FullName);

      lengthSetEvent.WaitOne();

      if (readOffset == length.Value)
        return 0; // XXX: bufferLength < length

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
                                 (readOffset + count < length.Value)
                                   ? count
                                   : (int)(length.Value - readOffset));

          readOffset  += read;
          offset      += read;
          count       -= read;
          ret         += read;
        }
        finally {
          Monitor.Exit(readWriteLock);
        }

        if (0 < count && readOffset < length.Value)
          continue;

        return ret;
      }
    }

    private ImapSession session;
    private IAsyncResult appendAsyncResult;

    private long? length;
    private ManualResetEvent lengthSetEvent;
    private ChunkedMemoryStream buffer;
    private long readOffset = 0L;
    private long writeOffset = 0L;
    private readonly int readTimeout;
    private readonly int writeTimeout;
    private readonly object readWriteLock = new object();
    private _ReadStream readStream;
    private _WriteStream writeStream;

    private class _WriteStream : Stream {
      public override bool CanSeek {
        get { return false; }
      }

      public override bool CanRead {
        get { return false; }
      }

      public override bool CanWrite {
        get { return !IsClosed; }
      }

      public override bool CanTimeout {
        get { return false; }
      }

      private bool IsClosed {
        get { return buffer == null; }
      }

      public override long Position {
        get { throw ExceptionUtils.CreateNotSupportedSeekingStream(); }
        set { throw ExceptionUtils.CreateNotSupportedSeekingStream(); }
      }

      public override long Length {
        get { throw ExceptionUtils.CreateNotSupportedSeekingStream(); }
      }

      public _WriteStream(ImapAppendMessageBodyBuffer buffer)
      {
        this.buffer = buffer;
      }

      public override void Close()
      {
        if (buffer != null) {
          buffer.CloseWriteStream();
          buffer = null;
        }

        base.Close();
      }

      public override long Seek(long offset, SeekOrigin origin)
      {
        CheckDisposed();

        throw ExceptionUtils.CreateNotSupportedSeekingStream();
      }

      public override void SetLength(long value)
      {
        CheckDisposed();

        throw ExceptionUtils.CreateNotSupportedSettingStreamLength();
      }

      public override void Flush()
      {
        CheckDisposed();

        buffer.Flush();
      }

      private byte[] singleByteBuffer = new byte[1];

      public override void WriteByte(byte @value)
      {
        CheckDisposed();

        singleByteBuffer[0] = @value;

        buffer.Write(singleByteBuffer, 0, 1);
      }

      public override void Write(byte[] src, int offset, int count)
      {
        CheckDisposed();

        buffer.Write(src, offset, count);
      }

      public override int Read(byte[] dest, int offset, int count)
      {
        CheckDisposed();

        throw ExceptionUtils.CreateNotSupportedReadingStream();
      }

      private void CheckDisposed()
      {
        if (IsClosed)
          throw new ObjectDisposedException(GetType().FullName);
      }

      private ImapAppendMessageBodyBuffer buffer;
    }

    private class _ReadStream : Stream {
      public override bool CanSeek {
        get { return false; }
      }

      public override bool CanRead {
        get { return !IsClosed; }
      }

      public override bool CanWrite {
        get { return false; }
      }

      public override bool CanTimeout {
        get { return false; }
      }

      private bool IsClosed {
        get { return buffer == null; }
      }

      public override long Position {
        get { throw ExceptionUtils.CreateNotSupportedSeekingStream(); }
        set { throw ExceptionUtils.CreateNotSupportedSeekingStream(); }
      }

      public override long Length {
        get { CheckDisposed(); return buffer.GetLength(); }
      }

      public _ReadStream(ImapAppendMessageBodyBuffer buffer)
      {
        this.buffer = buffer;
      }

      public override void Close()
      {
        buffer = null;

        base.Close();
      }

      public override long Seek(long offset, SeekOrigin origin)
      {
        CheckDisposed();

        throw ExceptionUtils.CreateNotSupportedSeekingStream();
      }

      public override void SetLength(long value)
      {
        CheckDisposed();

        throw ExceptionUtils.CreateNotSupportedSettingStreamLength();
      }

      public override void Flush()
      {
        CheckDisposed();

        throw ExceptionUtils.CreateNotSupportedWritingStream();
      }

      public override void Write(byte[] src, int offset, int count)
      {
        CheckDisposed();

        throw ExceptionUtils.CreateNotSupportedWritingStream();
      }

      public override int Read(byte[] dest, int offset, int count)
      {
        CheckDisposed();

        return buffer.Read(dest, offset, count);
      }

      private void CheckDisposed()
      {
        if (IsClosed)
          throw new ObjectDisposedException(GetType().FullName);
      }

      private ImapAppendMessageBodyBuffer buffer;
    }
  }
}
