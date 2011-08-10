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

using Smdn.IO;

namespace Smdn.Formats.IsoBaseMediaFile {
  public class DataBlock : IDisposable {
    public bool IsEmpty {
      get
      {
        CheckDisposed();

        if (stream == null)
          return true;
        else
          return (0L < stream.Length);
      }
    }

    public long Length {
      get
      {
        CheckDisposed();

        if (stream == null)
          return 0L;
        else
          return stream.Length;
      }
    }

    public DataBlock()
    {
      this.stream = null;
    }

    public DataBlock(byte[] data)
      : this(new MemoryStream(data))
    {
    }

    public DataBlock(MemoryStream stream)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      this.stream = stream;
    }

    internal DataBlock(PartialStream stream)
    {
      this.stream = stream;
    }

    ~DataBlock()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing) {
        if (stream != null) {
          stream.Close();
          stream = null;
        }
      }

      disposed = true;
    }

    public byte[] ToByteArray()
    {
      using (var s = ToStream()) {
        s.Close();

        return s.ToArray();
      }
    }

    public MemoryStream ToStream()
    {
      var ret = new MemoryStream((int)Length);

      OpenRead().CopyTo(ret); // XXX: long -> int

      return ret;
    }

    public Stream OpenRead()
    {
      return Open(FileAccess.Read);
    }

    public Stream OpenWrite()
    {
      return Open(FileAccess.ReadWrite);
    }

    public Stream Open(FileAccess access)
    {
      CheckDisposed();

      if ((int)(access & FileAccess.Write) != 0) {
        if (stream == null) {
          stream = new MemoryStream();
        }
        else if (stream is PartialStream) {
          ReadIntoMemory();
        }

        return new UnclosableStream(stream);
      }
      else {
        if (stream == null) {
          return Stream.Null;
        }
        else {
          if (stream is PartialStream) {
            var partialStream = (stream as PartialStream).Clone();

            partialStream.Position = 0L;

            return new UnclosableStream(partialStream);
          }
          else /* if (stream is MemoryStream) */ {
            var memoryStream = stream as MemoryStream;

            memoryStream.Position = 0L;

            return new UnclosableStream(memoryStream);
            //return new MemoryStream(memoryStream.GetBuffer(), 0, (int)memoryStream.Length, false); // XXX: long -> int
          }
        }
      }
    }

    internal void ReadIntoMemory()
    {
      if (!(stream is PartialStream))
        return;

      var partialStream = stream as PartialStream;

      partialStream.Position = 0L;

      stream = new MemoryStream((int)partialStream.Length); // XXX: long -> int

      partialStream.CopyTo(stream, 0x100);
    }

    private void CheckDisposed()
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private bool disposed = false;
    private Stream stream;
  }
}
