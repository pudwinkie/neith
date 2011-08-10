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

namespace Smdn {
  public class ByteStringBuilder {
    [System.Runtime.CompilerServices.IndexerName("Bytes")]
    public byte this[int index] {
      get
      {
        if (index < 0 || length <= index)
          throw new IndexOutOfRangeException();
        return buffer[index];
      }
      set
      {
        if (index < 0 || length <= index)
          throw new ArgumentOutOfRangeException("index", index, "index out of range");
        buffer[index] = value;
      }
    }

    public int Length {
      get { return length; }
      set
      {
        if (value < 0)
          throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("Length", value);
        length = value;
      }
    }

    public int Capacity {
      get { return buffer.Length; }
    }

    public int MaxCapacity {
      get { return maxCapacity; }
    }

    public ByteStringBuilder()
      : this(16, int.MaxValue)
    {
    }

    public ByteStringBuilder(int capacity)
      : this(capacity, int.MaxValue)
    {
    }

    public ByteStringBuilder(int capacity, int maxCapacity)
    {
      if (capacity <= 0)
        throw ExceptionUtils.CreateArgumentMustBeNonZeroPositive("capacity", capacity);
      if (maxCapacity < capacity)
        throw ExceptionUtils.CreateArgumentMustBeGreaterThanOrEqualTo("'capacity'", "maxCapacity", maxCapacity);

      this.buffer = new byte[capacity];
      this.length = 0;
      this.maxCapacity = maxCapacity;
    }

    public ByteStringBuilder Append(byte b)
    {
      EnsureCapacity(length + 1);

      buffer[length++] = b;

      return this;
    }

    public ByteStringBuilder Append(ByteString str)
    {
      if (str == null)
        throw new ArgumentNullException("str");

      return Append(str.Segment);
    }

    public ByteStringBuilder Append(ByteString str, int startIndex, int count)
    {
      if (str == null)
        throw new ArgumentNullException("str");

      return Append(str.GetSubSegment(startIndex, count));
    }

    public ByteStringBuilder Append(byte[] str)
    {
      return Append(new ArraySegment<byte>(str));
    }

    public ByteStringBuilder Append(byte[] str, int startIndex, int count)
    {
      return Append(new ArraySegment<byte>(str, startIndex, count));
    }

    public ByteStringBuilder Append(ArraySegment<byte> segment)
    {
      if (segment.Count == 0)
        return this;

      EnsureCapacity(length + segment.Count);

      Buffer.BlockCopy(segment.Array, segment.Offset, buffer, length, segment.Count);

      length += segment.Count;

      return this;
    }

    public ByteStringBuilder Append(string str)
    {
      if (str == null)
        throw new ArgumentNullException("str");

      if (str.Length == 0)
        return this;

      EnsureCapacity(length + str.Length);

      for (var index = 0; index < str.Length; index++, length++) {
        buffer[length] = (byte)str[index];
      }

      return this;
    }

    private void EnsureCapacity(int capacity)
    {
      if (capacity <= buffer.Length)
        return;

      capacity = Math.Max(capacity, buffer.Length * 2);

      if (maxCapacity < capacity)
        throw ExceptionUtils.CreateArgumentMustBeLessThanOrEqualTo("'MaxCapacity'", "capacity", capacity);

      var newBuffer = new byte[capacity];

      Buffer.BlockCopy(buffer, 0, newBuffer, 0, length);

      buffer = newBuffer;
    }

    public ArraySegment<byte> GetSegment()
    {
      return new ArraySegment<byte>(buffer, 0, length);
    }

    public ArraySegment<byte> GetSegment(int offset, int count)
    {
      if (offset < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("offset", offset);
      if (count < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("count", count);
      if (length - count < offset)
        throw new ArgumentException("index + count is larger than length");

      return new ArraySegment<byte>(buffer, offset, count);
    }

    public byte[] ToByteArray()
    {
      var bytes = new byte[length];

      Buffer.BlockCopy(buffer, 0, bytes, 0, length);

      return bytes;
    }

    public ByteString ToByteString(bool asMutable)
    {
      return ByteString.Create(asMutable, buffer, 0, length);
    }

    public override string ToString()
    {
      return ByteString.ToString(null,
                                 new ArraySegment<byte>(buffer, 0, length),
                                 0,
                                 length);
    }

    private byte[] buffer;
    private int length;
    private int maxCapacity;
  }
}
