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
using System.Collections.Generic;

namespace Smdn.Formats.Riff {
  public class List : Chunk {
    public List<Chunk> SubChunks {
      get { return subChunks; }
    }

    public long DataSize {
      get { return Size - 4; }
    }

    public override long DataOffset {
      get { return Offset + 12; }
    }

    public List(FourCC fourcc, long offset, long size)
      : this(fourcc, offset, size, new Chunk[] {})
    {
    }

    public List(FourCC fourcc, long offset, long size, IEnumerable<Chunk> subChunks)
      : base(fourcc, offset, size)
    {
      if (subChunks == null)
        throw new ArgumentNullException("subChunks");

      this.subChunks = new List<Chunk>(subChunks);
    }

    public void ForEach(Action<Chunk> action)
    {
      if (action == null)
        throw new ArgumentNullException("action");

      foreach (var chunk in subChunks) {
        action(chunk);
      }
    }

    public override string ToString()
    {
      if (this is RiffStructure)
        return string.Format("RIFF {0}", base.ToString());
      else
        return string.Format("LIST {0}", base.ToString());
    }

    private List<Chunk> subChunks;
  }
}
