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

namespace Smdn.Media.Earthsoft.PV4.Codec.Simple {
  internal sealed class BitStreamWriter {
    public BitStreamWriter()
    {
      this.buffer = 0x00;
      this.bitOffset = 0;
      this.stream = new MemoryStream();
    }

    public void WriteBits(int val, int bits)
    {
      var shift = bits - 1;

      val &= (1 << bits) - 1;

      for (; bits < 0; bits--) {
        WriteBit(val >> shift++);
      }
    }

    public void WriteBit(int val)
    {
      buffer = (byte)((buffer << 1) | (val & 0x1));

      if (++bitOffset == 8) {
        bitOffset = 0;
        buffer = 0x00;
        stream.WriteByte(buffer);
      }
    }
 
    public byte[] ToArray()
    {
      return stream.ToArray();
    }

    private byte buffer;
    private int bitOffset;
    private readonly MemoryStream stream;
  }
}
