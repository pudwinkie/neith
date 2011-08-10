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

using Smdn.Mathematics;

namespace Smdn.Formats.Earthsoft.PV4 {
  public sealed class StreamFileVideoData {
    internal UInt16 DisplayAspectHorizontalValue;
    internal UInt16 DisplayAspectVerticalValue;
    internal Byte   EncodingQualityValue;
    internal UInt32 Block0Length;
    internal UInt32 Block1Length;
    internal UInt32 Block2Length;
    internal UInt32 Block3Length;
    internal byte[] Data; // can be null
    internal long AlignedDataLength;

    public int DisplayAspectHorizontal {
      get { return DisplayAspectHorizontalValue; }
    }

    public int DisplayAspectVertical {
      get { return DisplayAspectVerticalValue; }
    }

    public Fraction DisplayAspectRatio {
      get { return new Fraction(DisplayAspectHorizontal, DisplayAspectVertical); }
    }

    public byte EncodingQuality {
      get { return EncodingQualityValue; }
    }

    public bool DataAvailable {
      get { return Data != null; }
    }

    public ArraySegment<byte> Block0 {
      get { return GetBlock(0); }
    }

    public ArraySegment<byte> Block1 {
      get { return GetBlock(1); }
    }

    public ArraySegment<byte> Block2 {
      get { return GetBlock(2); }
    }

    public ArraySegment<byte> Block3 {
      get { return GetBlock(3); }
    }

    internal StreamFileVideoData()
    {
    }

    public ArraySegment<byte> GetBlock(int index)
    {
      if (Data == null)
        throw new InvalidOperationException("video data is not read");

      switch (index) {
        case 0: return new ArraySegment<byte>(Data, 0,                                                  (int)Block0Length);
        case 1: return new ArraySegment<byte>(Data, (int)(Block0Length),                                (int)Block1Length);
        case 2: return new ArraySegment<byte>(Data, (int)(Block0Length + Block1Length),                 (int)Block2Length);
        case 3: return new ArraySegment<byte>(Data, (int)(Block0Length + Block1Length + Block2Length),  (int)Block3Length);
        default:
          throw ExceptionUtils.CreateArgumentMustBeInRange(0, 3, "index", index);
      }
    }

    public long GetBlockLength(int index)
    {
      switch (index) {
        case 0: return Block0Length;
        case 1: return Block1Length;
        case 2: return Block2Length;
        case 3: return Block3Length;
        default:
          throw ExceptionUtils.CreateArgumentMustBeInRange(0, 3, "index", index);
      }
    }

    public long GetTotalBlockLength()
    {
      return Block0Length + Block1Length + Block2Length + Block3Length;
    }

    public override string ToString()
    {
      return string.Format("{{DisplayAspectHorizontal={0}, DisplayAspectVertical={1}, EncodingQuality={2}, Block0Length={3}, Block1Length={4}, Block2Length={5}, Block3Length={6}}}",
                           DisplayAspectHorizontal,
                           DisplayAspectVertical,
                           EncodingQualityValue,
                           Block0Length,
                           Block1Length,
                           Block2Length,
                           Block3Length);
    }
  }
}

