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
  public class FromLoliTransform : ICryptoTransform {
    public bool CanReuseTransform {
      get { return true; }
    }

    public bool CanTransformMultipleBlocks {
      get { return true; }
    }

    public int InputBlockSize {
      get { return 8; }
    }

    public int OutputBlockSize {
      get { return 1; }
    }

    public void Dispose()
    {
      disposed = true;
    }

    public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);

      var ret = 0;

      for (var i = 0; i < inputCount; i++) {
        switch (inputBuffer[inputOffset++]) {
          case LoliOctets.ロ: /*buffer |= 0;*/ break; // 0b
          case LoliOctets.リ: buffer |= bit; break; // 1b
          case LoliOctets.コ: // CR
          case LoliOctets.ン: // LF
            continue; // ignore
          default:
            throw new LolizationException("invalid octet");
        }

        if ((bit >>= 1) == 0x00/*00000000b*/) {
          outputBuffer[outputOffset++] = (byte)buffer;
          ret++;
          buffer  = 0x00;
          bit     = 0x80 /*10000000b*/;
        }
      }

      return ret;
    }

    public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);
      if (inputCount < 0)
        throw new ArgumentOutOfRangeException("inputCount", "must be zero or positive number");

      var outputBuffer = new byte[inputCount * InputBlockSize];

      var ret = TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, 0);

      if (ret != outputBuffer.Length)
        Array.Resize(ref outputBuffer, ret);

      Reset();

      return outputBuffer;
    }

    internal void Reset()
    {
      buffer  = 0x00;
      bit     = 0x80;
    }

    private bool disposed;
    private int buffer = 0x00;
    private int bit = 0x80;
  }
}
