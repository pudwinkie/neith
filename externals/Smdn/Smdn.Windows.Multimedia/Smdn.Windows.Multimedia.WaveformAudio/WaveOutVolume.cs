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

namespace Smdn.Windows.Multimedia.WaveformAudio {
  public struct WaveOutVolume
  {
    public static readonly WaveOutVolume Max = new WaveOutVolume(0xffff, 0xffff);
    public static readonly WaveOutVolume Min = new WaveOutVolume(0x0000, 0x0000);

    public int Left {
      get { return left; }
      set
      {
        if (value < 0x0000)
          throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("Left", value);
        else if (0x10000 <= value)
          throw ExceptionUtils.CreateArgumentMustBeLessThan(0x10000, "Left", value);
        else
          left = value;
      }
    }

    public int Right {
      get { return right; }
      set
      {
        if (value < 0x0000)
          throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("Right", value);
        else if (0x10000 < value)
          throw ExceptionUtils.CreateArgumentMustBeLessThan(0x10000, "Right", value);
        else
          right = value;
      }
    }

    public WaveOutVolume(int volume)
      : this(volume, volume)
    {
    }

    public WaveOutVolume(int left, int right)
    {
      this.left = 0;
      this.right = 0;

      this.Left = left;
      this.Right = right;
    }

    public static explicit operator uint(WaveOutVolume vol)
    {
      return (((uint)vol.right << 16) | (uint)vol.left);
    }

    public static WaveOutVolume FromUInt32(uint volume)
    {
      return new WaveOutVolume((int)(volume & 0xffff), (int)((volume >> 16) & 0xffff));
    }

    private int left;
    private int right;
  }
}
