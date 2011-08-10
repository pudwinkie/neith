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
using System.Runtime.InteropServices;

using Smdn.Interop;
using Smdn.Windows.Multimedia.Interop;

namespace Smdn.Windows.Multimedia {
  public class WinMMException : SystemException {
    public MMRESULT MMResult {
      get; private set;
    }

    internal static void ThrowIfError(MMRESULT result)
    {
      if (result == MMRESULT.MMSYSERR_NOERROR)
        return;

      throw new WinMMException(result);
    }

    internal static void ThrowIfWaveInError(MMRESULT result)
    {
      if (result == MMRESULT.MMSYSERR_NOERROR)
        return;

      using (var buffer = new CoTaskMemoryBuffer(Consts.MAXERRORLENGTH)) {
        if (winmm.waveInGetErrorText(result, (IntPtr)buffer, (uint)buffer.Size) == MMRESULT.MMSYSERR_NOERROR)
          throw new WinMMException(Marshal.PtrToStringAuto((IntPtr)buffer), result);
      }

      throw new WinMMException(result);
    }

    internal static void ThrowIfWaveOutError(MMRESULT result)
    {
      if (result == MMRESULT.MMSYSERR_NOERROR)
        return;

      using (var buffer = new CoTaskMemoryBuffer(Consts.MAXERRORLENGTH)) {
        if (winmm.waveOutGetErrorText(result, (IntPtr)buffer, (uint)buffer.Size) == MMRESULT.MMSYSERR_NOERROR)
          throw new WinMMException(Marshal.PtrToStringAuto((IntPtr)buffer), result);
      }

      throw new WinMMException(result);
    }

    internal WinMMException(MMRESULT result)
      : this(ToString(result), result)
    {
    }

    internal WinMMException(string message, MMRESULT result)
      : base(message)
    {
      this.MMResult = result;
    }

    private static string ToString(MMRESULT result)
    {
      var val = (uint)result;

      if (Enum.IsDefined(typeof(MMRESULT), val))
        return ((MMRESULT)val).ToString();
      else if (Enum.IsDefined(typeof(WAVERR), val))
        return ((WAVERR)val).ToString();
      else if (Enum.IsDefined(typeof(MIXERR), val))
        return ((MIXERR)val).ToString();
      else
        return string.Format("unknown error: {0}", result);
    }
  }
}
