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

namespace Smdn.Media.Alsa {
  // http://www.alsa-project.org/alsa-doc/alsa-lib/
  public static class libasound {
    private const string dllname = "libasound";

    [DllImport(dllname)] public static extern IntPtr snd_strerror(int errornum);

    [DllImport(dllname)] public static extern int snd_pcm_open(ref /* snd_pcm_t** */IntPtr handle, string name, snd_pcm_stream_t stream, int mode);
    [DllImport(dllname)] public static extern int snd_pcm_start(/* snd_pcm_t* */IntPtr handle);
    [DllImport(dllname)] public static extern int snd_pcm_pause(/* snd_pcm_t* */IntPtr handle, int enable);
    [DllImport(dllname)] public static extern int snd_pcm_resume(/* snd_pcm_t* */IntPtr handle);
    [DllImport(dllname)] public static extern int snd_pcm_drain(/* snd_pcm_t* */IntPtr handle);
    [DllImport(dllname)] public static extern int snd_pcm_drop(/* snd_pcm_t* */IntPtr handle);
    [DllImport(dllname)] public static extern int snd_pcm_close(/* snd_pcm_t* */IntPtr handle);
    [DllImport(dllname)] public static extern int snd_pcm_recover(/* snd_pcm_t* */IntPtr handle, int err, int silent);
    [DllImport(dllname)] public static extern int snd_pcm_writei(/* snd_pcm_t* */IntPtr handle, IntPtr buffer, uint size);
    [DllImport(dllname)] public static extern int snd_pcm_set_params(/* snd_pcm_t* */IntPtr handle, snd_pcm_format_t format, snd_pcm_access_t access, uint channels, uint rate, int soft_resample, uint latency);
  }
}
