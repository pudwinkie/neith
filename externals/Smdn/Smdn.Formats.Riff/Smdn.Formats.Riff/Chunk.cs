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

namespace Smdn.Formats.Riff {
  public class Chunk {
    public FourCC FourCC {
      get; private set;
    }

    public long Offset {
      get { return offset; }
      set
      {
        if (value < 0)
          throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("Offset", value);
        offset = value;
      }
    }

    public long Size {
      get { return size; }
      set
      {
        if (value < 0)
          throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("Size", value);
        size = value;
      }
    }

    public virtual long DataOffset {
      get { return Offset + 8; }
    }

    public Chunk(FourCC fourcc)
      : this(fourcc, 0, 0)
    {
    }

    public Chunk(FourCC fourcc, long offset, long size)
    {
      this.FourCC = fourcc;
      this.Offset = offset;
      this.Size   = size;
    }

    public override string ToString()
    {
      return string.Format("'{0}' (Offset={1}, Size={2})", FourCC.ToString().Replace("\0", "\\0"), Offset, Size);
    }

    private long offset;
    private long size;
  }
}
