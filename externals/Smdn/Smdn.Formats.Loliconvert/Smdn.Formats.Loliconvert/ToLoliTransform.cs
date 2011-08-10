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
using System.Security.Cryptography;

namespace Smdn.Formats.Loliconvert {
  public class ToLoliTransform : ICryptoTransform {
    public bool CanReuseTransform {
      get { return true; }
    }

    public bool CanTransformMultipleBlocks {
      get { return true; }
    }

    public int InputBlockSize {
      get { return 1; }
    }

    public int OutputBlockSize {
      get { return 10 /* 8 + CRLF */; }
    }

    public void Dispose()
    {
      disposed = true;
    }

    private static readonly byte[][] quartets = new byte[][] {
      /* 0000 */ new byte[] {LoliOctets.ロ, LoliOctets.ロ, LoliOctets.ロ, LoliOctets.ロ},
      /* 0001 */ new byte[] {LoliOctets.ロ, LoliOctets.ロ, LoliOctets.ロ, LoliOctets.リ},
      /* 0010 */ new byte[] {LoliOctets.ロ, LoliOctets.ロ, LoliOctets.リ, LoliOctets.ロ},
      /* 0011 */ new byte[] {LoliOctets.ロ, LoliOctets.ロ, LoliOctets.リ, LoliOctets.リ},
      /* 0100 */ new byte[] {LoliOctets.ロ, LoliOctets.リ, LoliOctets.ロ, LoliOctets.ロ},
      /* 0101 */ new byte[] {LoliOctets.ロ, LoliOctets.リ, LoliOctets.ロ, LoliOctets.リ},
      /* 0110 */ new byte[] {LoliOctets.ロ, LoliOctets.リ, LoliOctets.リ, LoliOctets.ロ},
      /* 0111 */ new byte[] {LoliOctets.ロ, LoliOctets.リ, LoliOctets.リ, LoliOctets.リ},
      /* 1000 */ new byte[] {LoliOctets.リ, LoliOctets.ロ, LoliOctets.ロ, LoliOctets.ロ},
      /* 1001 */ new byte[] {LoliOctets.リ, LoliOctets.ロ, LoliOctets.ロ, LoliOctets.リ},
      /* 1010 */ new byte[] {LoliOctets.リ, LoliOctets.ロ, LoliOctets.リ, LoliOctets.ロ},
      /* 1011 */ new byte[] {LoliOctets.リ, LoliOctets.ロ, LoliOctets.リ, LoliOctets.リ},
      /* 1100 */ new byte[] {LoliOctets.リ, LoliOctets.リ, LoliOctets.ロ, LoliOctets.ロ},
      /* 1101 */ new byte[] {LoliOctets.リ, LoliOctets.リ, LoliOctets.ロ, LoliOctets.リ},
      /* 1110 */ new byte[] {LoliOctets.リ, LoliOctets.リ, LoliOctets.リ, LoliOctets.ロ},
      /* 1111 */ new byte[] {LoliOctets.リ, LoliOctets.リ, LoliOctets.リ, LoliOctets.リ},
    };

    public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);

      var ret = outputOffset;

      for (var i = 0; i < inputCount; i++) {
        if (lineLength == 5) {
          // CRLF
          outputBuffer[outputOffset++] = LoliOctets.コ;
          outputBuffer[outputOffset++] = LoliOctets.ン;
          lineLength = 0;
        }

        var octet = inputBuffer[inputOffset++];
        var quartet = octet >> 4;

        Buffer.BlockCopy(quartets[quartet], 0, outputBuffer, outputOffset, 4);
        outputOffset += 4;

        quartet = octet & 0x0f;

        Buffer.BlockCopy(quartets[quartet], 0, outputBuffer, outputOffset, 4);
        outputOffset += 4;

        lineLength++;
      }

      return outputOffset - ret;
    }

    public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);
      if (inputCount < 0)
        throw new ArgumentOutOfRangeException("inputCount", "must be zero or positive number");

      var outputBuffer = new byte[inputCount * OutputBlockSize];

      var ret = TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, 0);

      if (ret != outputBuffer.Length)
        Array.Resize(ref outputBuffer, ret);

      Reset();

      return outputBuffer;
    }

    internal void Reset()
    {
      lineLength = 0;
    }

    private bool disposed = false;
    private int lineLength = 0;
  }
}
