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
using System.IO;

using Smdn.Interop;

namespace Smdn.Media.Alsa {
  public class PcmStreamPlayer : Smdn.Media.PcmStreamPlayer {
    public override long PositionInBytes {
      get
      {
        if (context == null)
          return 0;
        else
          return context.Stream.Position;
      }
      set
      {
        //throw new NotImplementedException();
      }
    }

    public override int Volume {
      get
      {
        //throw new NotImplementedException();
        return 0;
      }
      set
      {
        //throw new NotImplementedException();
      }
    }

    public override bool Repeat {
      get
      {
        //throw new NotImplementedException();
        return false;
      }
      set
      {
        //throw new NotImplementedException();
      }
    }

    ~PcmStreamPlayer()
    {
      Dispose(false);
    }

    protected override void Dispose(bool disposing)
    {
      if (handle != IntPtr.Zero) {
        libasound.snd_pcm_close(handle);
        handle = IntPtr.Zero;
      }

      base.Dispose(disposing);
    }

    protected override void DoPlaySync(Stream linearPcmStream, WAVEFORMATEX format)
    {
      context = Prepare(linearPcmStream, format);

      Play(context);
    }

    protected override void DoPlayAsync(Stream linearPcmStream, WAVEFORMATEX format)
    {
      context = Prepare(linearPcmStream, format);

      System.Threading.ThreadPool.QueueUserWorkItem(Play, context);
    }

    private PlayingStreamContext Prepare(Stream linearPcmStream, WAVEFORMATEX format)
    {
      if (handle != IntPtr.Zero)
        libasound.snd_pcm_close(handle);
      if (context != null)
        context.Dispose();

      snd_pcm_format_t pcmFormat;

      if (format.wBitsPerSample == 8)
        pcmFormat = snd_pcm_format_t.SND_PCM_FORMAT_U8;
      else if (format.wBitsPerSample == 16)
        pcmFormat = snd_pcm_format_t.SND_PCM_FORMAT_S16_LE;
      else
        throw new NotSupportedException("unsupported sample format");

      AlsaException.ThrowIfError(libasound.snd_pcm_open(ref handle, "default", snd_pcm_stream_t.SND_PCM_STREAM_PLAYBACK, 0 /*blocking*/));
      AlsaException.ThrowIfError(libasound.snd_pcm_set_params(handle, pcmFormat, snd_pcm_access_t.SND_PCM_ACCESS_RW_INTERLEAVED, format.nChannels, format.nSamplesPerSec, 1, 500 * 1000));

      return new PlayingStreamContext(handle, linearPcmStream, format);
    }

    private class PlayingStreamContext : IDisposable {
      public IntPtr Handle;
      public Stream Stream;
      public CoTaskMemoryBuffer Buffer;
      public int BlockAlign;
      //public bool Paused;
      public bool Stopped;

      public PlayingStreamContext(IntPtr handle, Stream stream, WAVEFORMATEX format)
      {
        Handle = handle;
        Stream = stream;
        BlockAlign = format.nBlockAlign;
        Buffer = new CoTaskMemoryBuffer((int)((format.nAvgBytesPerSec / 10) / BlockAlign + 1) * BlockAlign); // 0.1 sec
        Stopped = false;
        //Paused = false;
      }

      public void Dispose()
      {
        Stream = null;

        if (Buffer != null) {
          Buffer.Free();
          Buffer = null;
        }
      }
    }

    private void Play(object state)
    {
      var context = (PlayingStreamContext)state;
      var buf = new byte[context.Buffer.Size];

      for (;;) {
        if (context.Stopped) {
          libasound.snd_pcm_drop(context.Handle);
          break;
        }

        var read = context.Stream.Read(buf, 0, buf.Length);

        if (read <= 0)
          break;

        System.Runtime.InteropServices.Marshal.Copy(buf, 0, context.Buffer.Ptr, read);

        var frames = libasound.snd_pcm_writei(context.Handle, context.Buffer.Ptr, (uint)(read / context.BlockAlign));

        if (frames < 0)
          AlsaException.ThrowIfError(libasound.snd_pcm_recover(context.Handle, frames, 0));
      }

      OnPlayDone(EventArgs.Empty);
    }

    protected override void DoPause()
    {
      // 0 = resume, 1 = pause
      AlsaException.ThrowIfError(libasound.snd_pcm_pause(handle, 1));
    }

    protected override void DoResume()
    {
      AlsaException.ThrowIfError(libasound.snd_pcm_pause(handle, 0));
      //AlsaException.ThrowIfError(libasound.snd_pcm_recover(handle, 0, 0));
    }

    protected override void DoStop()
    {
      //AlsaException.ThrowIfError(libasound.snd_pcm_pause(handle, 1));
      context.Stopped = true;
    }

    private PlayingStreamContext context;
    private IntPtr handle;
  }
}
