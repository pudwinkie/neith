// 
// Author:
//       smdn <smdn@mail.invisiblefulmoon.net>
// 
// Copyright (c) 2008-2010 smdn
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
using System.Text;

namespace Smdn.Net {
  public static class NetworkTransferEncoding {
#region "TransferEncoding class"
    private class TransferEncoding : Encoding {
      public TransferEncoding(int bits)
      {
        if (bits < 0 || 8 < bits)
          throw new ArgumentOutOfRangeException("bits", bits, "must be from 0 to 8");

        maxValue = (char)(1 << bits);
      }

      public override int GetMaxCharCount(int byteCount)
      {
        if (byteCount < 0)
          throw new ArgumentOutOfRangeException("byteCount", byteCount, "must be zero or positive number");

        return byteCount;
      }

      public override int GetMaxByteCount(int charCount)
      {
        if (charCount < 0)
          throw new ArgumentOutOfRangeException("charCount", charCount, "must be zero or positive number");

        return charCount;
      }

      public override int GetByteCount(char[] chars, int index, int count)
      {
        if (index < 0)
          throw new ArgumentOutOfRangeException("index", index, "must be zero or positive number");
        if (count < 0)
          throw new ArgumentOutOfRangeException("count", count, "must be zero or positive nubmer");

        if (EncoderFallback == null)
          return count - index;

        EncoderFallbackBuffer buffer = null;
        var byteCount = 0;

        for (;index < count; index++) {
          if (chars[index] < maxValue) {
            byteCount++;
          }
          else {
            if (buffer == null)
              buffer = EncoderFallback.CreateFallbackBuffer();

            buffer.Fallback(chars[index], index);

            var fallbackChars = new char[buffer.Remaining];

            for (var r = 0; r < fallbackChars.Length; r++) {
              fallbackChars[r] = buffer.GetNextChar();
            }

            byteCount += GetByteCount(fallbackChars, 0, fallbackChars.Length);
          }
        }

        return byteCount;
      }

      public override int GetCharCount(byte[] bytes, int index, int count)
      {
        if (index < 0)
          throw new ArgumentOutOfRangeException("index", index, "must be zero or positive number");
        if (count < 0)
          throw new ArgumentOutOfRangeException("count", count, "must be zero or positive number");

        return count;
      }

      public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
      {
        if (chars == null)
          throw new ArgumentNullException("chars");
        if (bytes == null)
          throw new ArgumentNullException("chars");
        if (charIndex < 0)
          throw new ArgumentOutOfRangeException("charIndex", charIndex, "must be zero or positive number");
        if (chars.Length < charIndex + charCount)
          throw new ArgumentOutOfRangeException("charCount", charCount, "must be less than length of chars");
        if (byteIndex < 0)
          throw new ArgumentOutOfRangeException("byteIndex", byteIndex, "must be zero or positive number");

        var byteCount = 0;

        for (var i = 0; i < charCount; i++, charIndex++, byteIndex++, byteCount++) {
          if (chars[charIndex] < maxValue) {
            bytes[byteIndex] = (byte)chars[charIndex];
            byteCount++;
          }
          else {
            if (EncoderFallback == null) {
              bytes[byteIndex] = (byte)'?';
              byteCount++;
            }
            else {
              var buffer = EncoderFallback.CreateFallbackBuffer();

              if (buffer.Fallback(chars[charIndex], charIndex)) {
                var fallbackChars = new char[buffer.Remaining];

                for (var r = 0; r < fallbackChars.Length; r++) {
                  fallbackChars[r] = buffer.GetNextChar();
                }

                var c = GetBytes(fallbackChars, 0, fallbackChars.Length, bytes, byteIndex);

                byteIndex += c - 1;
                byteCount += c;
              }
              else {
                bytes[byteIndex] = (byte)'?';
                byteCount++;
              }
            }
          }
        }

        return byteCount;
      }

      public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
      {
        if (bytes == null)
          throw new ArgumentNullException("bytes");
        if (chars == null)
          throw new ArgumentNullException("chars");
        if (byteIndex < 0)
          throw new ArgumentOutOfRangeException("byteIndex", byteIndex, "must be zero or positive number");
        if (bytes.Length < byteIndex + byteCount)
          throw new ArgumentOutOfRangeException("byteCount", byteCount, "must be less than length of bytes");
        if (charIndex < 0)
          throw new ArgumentOutOfRangeException("charIndex", charIndex, "must be zero or positive number");

        var charCount = 0;

        for (var i = 0; i < byteCount; i++, byteIndex++, charIndex++, charCount++) {
          chars[charIndex] = (char)bytes[byteIndex];
        }

        return charCount;
      }

      private readonly char maxValue;
    }
#endregion

    // for string which contains 7-bit characters
    public static readonly Encoding Transfer7Bit;

    // for string which contains 8-bit characters
    public static readonly Encoding Transfer8Bit;

    static NetworkTransferEncoding()
    {
      Transfer7Bit = (new TransferEncoding(7)).Clone() as Encoding;
      Transfer7Bit.DecoderFallback = new DecoderExceptionFallback();
      Transfer7Bit.EncoderFallback = new EncoderExceptionFallback();

      Transfer8Bit = (new TransferEncoding(8)).Clone() as Encoding;
      Transfer8Bit.DecoderFallback = new DecoderExceptionFallback();
      Transfer8Bit.EncoderFallback = new EncoderExceptionFallback();
    }
  }
}
