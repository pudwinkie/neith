// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2009-2011 smdn
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

namespace Smdn.IO {
  public sealed class ChunkedMemoryStream : Stream {
    public static readonly int DefaultChunkSize = 40960;

    public delegate Chunk Allocator(int chunkSize);

    public abstract class Chunk : IDisposable {
      public abstract void Dispose();

      public byte[] Data;
      internal Chunk Next = null;
    }

    private class DefaultChunk : Chunk {
      public static DefaultChunk Allocate(int chunkSize)
      {
        return new DefaultChunk(chunkSize);
      }

      private DefaultChunk(int chunkSize)
      {
        base.Data = new byte[chunkSize];
      }

      public override void Dispose()
      {
        base.Data = null;
      }
    }

    private sealed class ChunkChain : IDisposable {
      public int ChunkSize {
        get { return chunkSize; }
      }

      public long Length {
        get { return ((chunkCount - 1) * chunkSize) + lastChunkLength; }
      }

      public long Position {
        get { return currentChunkIndex * chunkSize + currentChunkOffset; }
      }

      private int ReadableByteCount {
        get { return ((currentChunk.Next == null) ? lastChunkLength : chunkSize) - currentChunkOffset; }
      }

      public ChunkChain(int chunkSize, Allocator allocator)
      {
        this.chunkSize = chunkSize;
        this.allocator = allocator;

        currentChunkOffset = 0;
      }

      private Chunk TryGetFirstChunk(bool ensureCreated)
      {
        if (ensureCreated && firstChunk == null)
          currentChunk = firstChunk = allocator(chunkSize);

        return firstChunk;
      }

      public void Dispose()
      {
        var chunk = TryGetFirstChunk(false);

        for (;;) {
          if (chunk == null)
            break;

          var next = chunk.Next;

          chunk.Dispose();

          chunk = next;
        }

        firstChunk = null;
        currentChunk = null;
      }

      public void SetLength(long length)
      {
        if (length == Length)
          return; // do nothing

        chunkCount = 1;

        var chunk = TryGetFirstChunk(true);

        // allocate
        while ((int)chunkSize <= length) {
          if (chunk.Next == null)
            chunk.Next = allocator(chunkSize);

          chunk = chunk.Next;
          chunkCount++;

          length -= chunkSize;
        }

        lastChunkLength = (int)length;

        // dispose and remove from chain
        var next = chunk.Next;

        chunk.Next = null;
        chunk = next;

        while (chunk != null) {
          next = chunk.Next;

          chunk.Next = null;
          chunk.Dispose();

          chunk = next;
        }

        // set new position
        if (Length < Position)
          SetPosition(Length);
      }

      public void SetPosition(long offset)
      {
        if (Position == offset)
          return;

        if (Length < offset)
          SetLength(offset);

        currentChunk = TryGetFirstChunk(true);
        currentChunkIndex = 0;

        while ((int)chunkSize <= offset) {
          currentChunk = currentChunk.Next;
          currentChunkIndex++;

          offset -= chunkSize;
        }

        currentChunkOffset = (int)offset;
      }

      public int ReadByte()
      {
        if (TryGetFirstChunk(false) == null)
          return -1; // stream is empty

        if (ReadableByteCount == 0) {
          if (!MoveToNextChunk(false))
            return -1; // end of stream
        }

        return currentChunk.Data[currentChunkOffset++];
      }

      public int Read(byte[] buffer, int offset, int count)
      {
        if (TryGetFirstChunk(false) == null)
          return 0; // stream is empty

        var read = 0;

        for (;;) {
          if (ReadableByteCount == 0) {
            if (!MoveToNextChunk(false))
              return read; // end of stream
          }

          var bytesToRead = Math.Min(ReadableByteCount, count);

          Buffer.BlockCopy(currentChunk.Data, currentChunkOffset, buffer, offset, bytesToRead);

          offset += bytesToRead;
          count -= bytesToRead;
          read += bytesToRead;

          currentChunkOffset += bytesToRead;

          if (count <= 0)
            return read;
        }
      }

      public void WriteByte(byte @value)
      {
        TryGetFirstChunk(true);

        if (chunkSize == currentChunkOffset)
          MoveToNextChunk(true);

        currentChunk.Data[currentChunkOffset++] = @value;

        if (currentChunk.Next == null)
          lastChunkLength = currentChunkOffset;
      }

      public void Write(byte[] buffer, int offset, int count)
      {
        TryGetFirstChunk(true);

        for (;;) {
          if (currentChunkOffset == chunkSize)
            MoveToNextChunk(true);

          var bytesToWrite = Math.Min(chunkSize - currentChunkOffset, count);

          Buffer.BlockCopy(buffer, offset, currentChunk.Data, currentChunkOffset, bytesToWrite);

          offset += bytesToWrite;
          count -= bytesToWrite;

          currentChunkOffset += bytesToWrite;

          if (currentChunk.Next == null)
            lastChunkLength = currentChunkOffset;

          if (count <= 0)
            return;
        }
      }

      private bool MoveToNextChunk(bool write)
      {
        if (currentChunk.Next == null) {
          if (!write)
            return false;

          currentChunk.Next = allocator(chunkSize);
          chunkCount++;
          lastChunkLength = 0;
        }

        currentChunk = currentChunk.Next;
        currentChunkIndex++;

        currentChunkOffset = 0;

        return true;
      }

      public byte[] ToArray()
      {
        var buffer = new byte[Length];
        var offset = 0L;
        var chunk = TryGetFirstChunk(false);

        if (chunk == null)
          return buffer; // stream is empty (length must be zero)

        for (;;) {
          if (chunk.Next == null) {
            Array.Copy(chunk.Data, 0, buffer, offset, lastChunkLength);
            //Buffer.BlockCopy(chunk.Data, 0, buffer, offset, lastChunkLength);

            return buffer;
          }
          else {
            Array.Copy(chunk.Data, 0, buffer, offset, chunkSize);
            //Buffer.BlockCopy(chunk.Data, 0, buffer, offset, chunkSize);

            chunk = chunk.Next;
            offset += chunkSize;
          }
        }
      }

      private readonly int chunkSize;
      private Allocator allocator;
      private Chunk firstChunk;
      private Chunk currentChunk;

      private int currentChunkOffset;
      private long currentChunkIndex = 0;
      private long chunkCount = 1;
      private int lastChunkLength = 0;
    }

    public override bool CanSeek {
      get { return !IsClosed /*&& true*/; }
    }

    public override bool CanRead {
      get { return !IsClosed /*&& true*/; }
    }

    public override bool CanWrite {
      get { return !IsClosed /*&& true*/; }
    }

    public override bool CanTimeout {
      get { return false; }
    }

    private bool IsClosed {
      get { return chain == null; }
    }

    public override long Position {
      get { CheckDisposed(); return chain.Position; }
      set
      {
        CheckDisposed();

        if (value < 0)
          throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("Position", value);

        chain.SetPosition(value);
      }
    }

    public override long Length {
      get { CheckDisposed(); return chain.Length; }
    }

    public int ChunkSize {
      get { CheckDisposed(); return chain.ChunkSize; }
    }

    public ChunkedMemoryStream()
      : this(DefaultChunkSize, DefaultChunk.Allocate)
    {
    }

    public ChunkedMemoryStream(int chunkSize)
      : this(chunkSize, DefaultChunk.Allocate)
    {
    }

    public ChunkedMemoryStream(Allocator allocator)
      : this(DefaultChunkSize, allocator)
    {
    }

    public ChunkedMemoryStream(int chunkSize, Allocator allocator)
    {
      if (chunkSize <= 0)
        throw ExceptionUtils.CreateArgumentMustBeNonZeroPositive("chunkSize", chunkSize);
      if (allocator == null)
        throw new ArgumentNullException("allocator");

      this.chain = new ChunkChain(chunkSize, allocator);
    }

    public override void Close()
    {
      if (chain != null) {
        chain.Dispose();
        chain = null;
      }

      base.Close();
    }

    public override void SetLength(long @value)
    {
      CheckDisposed();

      if (@value < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("value", @value);

      chain.SetLength(@value);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      CheckDisposed();

      // Stream.Seek spec: Seeking to any location beyond the length of the stream is supported.
      switch (origin) {
        case SeekOrigin.Current:
          offset += chain.Position;
          goto case SeekOrigin.Begin;

        case SeekOrigin.End:
          offset += chain.Length;
          goto case SeekOrigin.Begin;

        case SeekOrigin.Begin:
          if (offset < 0L)
            throw ExceptionUtils.CreateIOAttemptToSeekBeforeStartOfStream();;
          chain.SetPosition(offset);
          return chain.Position;

        default:
          throw ExceptionUtils.CreateArgumentMustBeValidEnumValue("origin", origin);
      }
    }

    public override int ReadByte()
    {
      CheckDisposed();

      return chain.ReadByte();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      CheckDisposed();

      if (buffer == null)
        throw new ArgumentNullException("buffer");
      if (offset < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("offset", offset);
      if (count < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("count", count);
      if (buffer.Length - count < offset)
        throw ExceptionUtils.CreateArgumentAttemptToAccessBeyondEndOfArray("offset", buffer, offset, count);

      if (count == 0)
        return 0;
      else
        return chain.Read(buffer, offset, count);
    }

    public override void WriteByte(byte @value)
    {
      CheckDisposed();

      chain.WriteByte(@value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      CheckDisposed();

      if (buffer == null)
        throw new ArgumentNullException("buffer");
      if (offset < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("offset", offset);
      if (count < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("count", count);
      if (buffer.Length - count < offset)
        throw ExceptionUtils.CreateArgumentAttemptToAccessBeyondEndOfArray("offset", buffer, offset, count);

      if (count == 0)
        return;
      else
        chain.Write(buffer, offset, count);
    }

    public override void Flush()
    {
      CheckDisposed();

      // do nothing
    }

    public byte[] ToArray()
    {
      CheckDisposed();

      return chain.ToArray();
    }

    private void CheckDisposed()
    {
      if (IsClosed)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private ChunkChain chain = null;
  }
}
