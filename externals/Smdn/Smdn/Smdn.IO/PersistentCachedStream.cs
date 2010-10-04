// 
// Author:
//       smdn <smdn@mail.invisiblefulmoon.net>
// 
// Copyright (c) 2009-2010 smdn
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
using System.IO;

namespace Smdn.IO {
  public sealed class PersistentCachedStream : CachedStreamBase {
    public PersistentCachedStream(Stream innerStream)
      : this(innerStream, 40960, true)
    {
    }

    public PersistentCachedStream(Stream innerStream, int blockSize)
      : this(innerStream, blockSize, true)
    {
    }

    public PersistentCachedStream(Stream innerStream, bool leaveInnerStreamOpen)
      : this(innerStream, 40960, leaveInnerStreamOpen)
    {
    }

    public PersistentCachedStream(Stream innerStream, int blockSize, bool leaveInnerStreamOpen)
      : base(innerStream, blockSize, leaveInnerStreamOpen)
    {
    }

    public override void Close()
    {
      if (cachedBlocks != null) {
        cachedBlocks.Clear();
        cachedBlocks = null;
      }

      base.Close();
    }

    protected override byte[] GetBlock(long blockIndex)
    {
      if (cachedBlocks.ContainsKey(blockIndex))
        return cachedBlocks[blockIndex];

      var block = ReadBlock(blockIndex);

      cachedBlocks.Add(blockIndex, block);

      return block;
    }

    private Dictionary<long, byte[]> cachedBlocks = new Dictionary<long, byte[]>();
  }
}
