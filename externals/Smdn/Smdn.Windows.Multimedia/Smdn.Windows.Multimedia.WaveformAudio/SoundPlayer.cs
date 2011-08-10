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

using Smdn.Windows.Multimedia.Interop;

namespace Smdn.Windows.Multimedia.WaveformAudio {
  public class SoundPlayer {
    public static void Stop()
    {
      winmm.PlaySound(IntPtr.Zero, IntPtr.Zero, SND.None);
    }

    public static void Play(byte[] buffer)
    {
      Play(buffer, SND.ASYNC);
    }

    public static void PlaySync(byte[] buffer)
    {
      Play(buffer, SND.SYNC);
    }

    public static void PlayLooping(byte[] buffer)
    {
      Play(buffer, SND.LOOP | SND.ASYNC);
    }

    private static void Play(byte[] buffer, SND flags)
    {
      if (buffer == null)
        throw new ArgumentNullException("buffer");

      flags |= SND.MEMORY;

      winmm.PlaySound(buffer, IntPtr.Zero, flags);
    }

    public static void Play(string file)
    {
      Play(file, SND.ASYNC);
    }

    public static void PlaySync(string file)
    {
      Play(file, SND.SYNC);
    }

    public static void PlayLooping(string file)
    {
      Play(file, SND.LOOP | SND.ASYNC);
    }

    private static void Play(string file, SND flags)
    {
      if (file == null)
        throw new ArgumentNullException("file");

      flags |= SND.FILENAME;

      winmm.PlaySound(file, IntPtr.Zero, flags);
    }

    /*
     * instance members
     */
    private SoundPlayer()
    {
      // TODO: impl
    }
  }
}
