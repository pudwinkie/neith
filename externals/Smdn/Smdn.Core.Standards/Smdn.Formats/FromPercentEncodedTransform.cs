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
using System.Security.Cryptography;

namespace Smdn.Formats {
  public sealed class FromPercentEncodedTransform : ICryptoTransform {
    public bool CanTransformMultipleBlocks {
      get { return true; }
    }

    public bool CanReuseTransform {
      get { return true; }
    }

    public int InputBlockSize {
      get { return 1; }
    }

    public int OutputBlockSize {
      get { return 1; }
    }

    public FromPercentEncodedTransform()
      : this(false)
    {
    }

    public FromPercentEncodedTransform(bool decodePlusToSpace)
    {
      this.decodePlusToSpace = decodePlusToSpace;
    }

    public void Clear()
    {
      disposed = true;
    }

    void IDisposable.Dispose()
    {
      Clear();
    }

    public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);
      if (inputBuffer == null)
        throw new ArgumentNullException("inputBuffer");
      if (inputOffset < 0)
        throw new ArgumentException("inputOffset < 0", "inputOffset");
      if (inputBuffer.Length < inputCount)
        throw new ArgumentException("inputBuffer.Length < inputCount", "inputCount");
      if (inputBuffer.Length - inputCount < inputOffset)
        throw new ArgumentException("inputBuffer.Length - inputCount < inputOffset", "inputOffset");
      if (outputBuffer == null)
        throw new ArgumentNullException("outputBuffer");
      if (outputOffset < 0)
        throw new ArgumentException("outputOffset < 0", "outputOffset");
      if (outputBuffer.Length < inputCount)
        throw new ArgumentException("outputBuffer.Length < inputCount", "outputBuffer");
      if (outputBuffer.Length - inputCount < outputOffset)
        throw new ArgumentException("outputBuffer.Length - inputCount < outputOffset", "outputOffset");

      var ret = 0;

      while (0 < inputCount--) {
        var octet = inputBuffer[inputOffset++];

        if (bufferOffset == 0) {
          if (decodePlusToSpace && octet == 0x2b) { // '+' 0x2b
            outputBuffer[outputOffset++] = Octets.SP;
            ret++;
          }
          else if (octet == 0x25) { // '%' 0x25
            buffer[bufferOffset++] = octet;
          }
          else {
            outputBuffer[outputOffset++] = octet;
            ret++;
          }
        }
        else {
          // encoded char
          buffer[bufferOffset++] = octet;
        }

        if (bufferOffset == 3) {
          // decode
          byte d = 0x00;

          for (var i = 1; i < 3; i++) {
            d <<= 4;

            if (0x30 <= buffer[i] && buffer[i] <= 0x39)
              // '0' 0x30 to '9' 0x39
              d |= (byte)(buffer[i] - 0x30);
            else if (0x41 <= buffer[i] && buffer[i] <= 0x46)
              // 'A' 0x41 to 'F' 0x46
              d |= (byte)(buffer[i] - 0x37);
            else if (0x61 <= buffer[i] && buffer[i] <= 0x66)
              // 'a' 0x61 to 'f' 0x66
              d |= (byte)(buffer[i] - 0x57);
            else
              throw new FormatException("incorrect form");
          }

          outputBuffer[outputOffset++] = d;
          ret++;

          bufferOffset = 0;
        }
      }

      return ret;
    }

    public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);
      if (inputBuffer == null)
        throw new ArgumentNullException("inputBuffer");
      if (inputCount < 0)
        throw new ArgumentException("inputCount < 0", "inputCount");
      if (inputBuffer.Length < inputCount)
        throw new ArgumentException("inputBuffer.Length < inputCount", "inputCount");
      if (inputBuffer.Length - inputCount < inputOffset)
        throw new ArgumentException("inputBuffer.Length - inputCount < inputOffset", "inputOffset");
      if (InputBlockSize < inputCount)
        throw new ArgumentOutOfRangeException("inputCount", inputCount, "input length too long");

      var outputBuffer = new byte[inputCount/* * OutputBlockSize */];
      var len = TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, 0);

      Array.Resize(ref outputBuffer, len);

      return outputBuffer;
    }

    private byte[] buffer = new byte[3];
    private int bufferOffset = 0;
    private bool disposed = false;
    private readonly bool decodePlusToSpace;
  }
}
