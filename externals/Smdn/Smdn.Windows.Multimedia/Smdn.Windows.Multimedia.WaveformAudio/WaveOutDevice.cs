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
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Smdn.Media;
using Smdn.Windows.Multimedia.Interop;

namespace Smdn.Windows.Multimedia.WaveformAudio {
  /*
   * http://msdn.microsoft.com/en-us/library/dd757715(VS.85).aspx
   * http://msdn.microsoft.com/en-us/library/dd797970(VS.85).aspx
   */
  public class WaveOutDevice : IWaveIODevice, IDisposable {
    private class WaveOutForm : WaveForm {
      public bool Done { get; set; }

      public WaveOutForm(WaveOutDevice device, int bufferSize, IntPtr userData)
        : base(device, bufferSize, userData)
      {
        Done = false;
      }

      protected internal override void Write(byte[] data)
      {
        base.Write(data);

        Done = false;
      }
    }

    /*
     * class members
     */
    public delegate void EnumerateDevicesHandler(uint deviceId, WAVEOUTCAPS caps);

    public static void EnumerateDevices(EnumerateDevicesHandler handler)
    {
      var devices = winmm.waveOutGetNumDevs();

      for (uint deviceID = 0; deviceID < devices; deviceID++) {
        WAVEOUTCAPS caps;

        WinMMException.ThrowIfWaveOutError(winmm.waveOutGetDevCaps(deviceID, out caps, WAVEOUTCAPS.Size));

        handler(deviceID, caps);
      }
    }

    public static bool IsFormatSupported(uint deviceID, long samplesPerSecond)
    {
      return IsFormatSupported(deviceID, WAVEFORMATEX.CreateLinearPcmFormat(samplesPerSecond, 16, 2));
    }

    public static bool IsFormatSupported(uint deviceID, long samplesPerSecond, int bitsPerSample, int channels)
    {
      return IsFormatSupported(deviceID, WAVEFORMATEX.CreateLinearPcmFormat(samplesPerSecond, bitsPerSample, channels));
    }

    public static bool IsFormatSupported(uint deviceID, WAVEFORMATEX format)
    {
      IntPtr discard;

      var result = winmm.waveOutOpen(out discard, deviceID, ref format, null, IntPtr.Zero, Consts.WAVE_FORMAT_QUERY);

      if ((WAVERR)result == WAVERR.WAVERR_BADFORMAT)
        return false;
      else
        WinMMException.ThrowIfWaveOutError(result);

      return true;
    }

    public static WAVEOUTCAPS GetCaps(uint deviceID)
    {
      WAVEOUTCAPS caps;

      WinMMException.ThrowIfWaveOutError(winmm.waveOutGetDevCaps(deviceID, out caps, WAVEOUTCAPS.Size));

      return caps;
    }

    public static bool IsCapacitySupported(uint deviceID, WAVECAPS caps)
    {
      return (GetCaps(deviceID).dwSupport & (uint)caps) != 0;
    }

    /*
     * instance members
     */
    public event EventHandler PlayDone;

    public IntPtr Handle {
      get { CheckDisposed(); return handle; }
    }

    public WAVEFORMATEX Format {
      get { CheckDisposed(); return format; }
    }

    public WaveOutVolume Volume {
      get
      {
        CheckDisposed();

        if (softVolume)
          return volume;
        else
          return GetVolume();
      }
      set
      {
        CheckDisposed();

        if (softVolume)
          volume = value;
        else
          SetVolume(value);
      }
    }

    public long Position {
      get { CheckDisposed(); return position; }
      set
      {
        CheckDisposed();

        if (!reader.BaseStream.CanSeek)
          throw ExceptionUtils.CreateNotSupportedSeekingStream();

        StopCore();

        reader.BaseStream.Position = initialOffset + (value / format.nBlockAlign) * format.nBlockAlign;
        finished = false;
        position = value;

        Play();
      }
    }

    public bool Repeat {
      get; set;
    }

    public WaveOutDevice(uint deviceID, long samplesPerSecond, int bitsPerSample, int channels)
      : this(deviceID,
             WAVEFORMATEX.CreateLinearPcmFormat(samplesPerSecond, bitsPerSample, channels))
    {
    }

    public WaveOutDevice(uint deviceID, long samplesPerSecond, int bitsPerSample, int channels, int bufferSize, int bufferCount)
      : this(deviceID,
             WAVEFORMATEX.CreateLinearPcmFormat(samplesPerSecond, bitsPerSample, channels),
             bufferSize,
             bufferCount)
    {
    }

    public WaveOutDevice(uint deviceID, WAVEFORMATEX format)
      : this(deviceID,
             format,
             (int)(((format.nAvgBytesPerSec / 4) / format.nBlockAlign) * format.nBlockAlign), // 0.25sec
             2)
    {
    }

    public WaveOutDevice(uint deviceID, WAVEFORMATEX format, int bufferSize, int bufferCount)
    {
      if (bufferSize <= 0)
        throw ExceptionUtils.CreateArgumentMustBeNonZeroPositive("bufferSize", bufferSize);
      if (bufferCount <= 0)
        throw ExceptionUtils.CreateArgumentMustBeNonZeroPositive("bufferCount", bufferCount);

      this.waveOutCallback = (waveOutProc)WaveOutCallback;
      this.format = format;

      WinMMException.ThrowIfWaveOutError(winmm.waveOutOpen(out this.handle, deviceID, ref this.format, this.waveOutCallback, IntPtr.Zero, Consts.CALLBACK_FUNCTION));

      this.softVolume = !IsCapacitySupported(deviceID, WAVECAPS.WAVECAPS_VOLUME);

      if (this.softVolume)
        volume = WaveOutVolume.Max;

      buffers = new WaveOutForm[bufferCount];

      for (var i = 0; i < bufferCount; i++) {
        buffers[i] = new WaveOutForm(this, bufferSize, new IntPtr(i));
      }
    }

    public virtual void Close()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    void IDisposable.Dispose()
    {
      Close();
    }

    protected virtual void Dispose(bool disposing)
    {
      if (handle == IntPtr.Zero)
        return;

      StopCore();

      if (buffers != null) {
        for (var i = 0; i < buffers.Length; i++) {
          buffers[i].Dispose();
          buffers[i] = null;
        }

        buffers = null;
      }

      // handle will be set IntPtr.Zero by MM_WOM_CLOSE
      winmm.waveOutClose(handle);
    }

    private void WaveOutCallback(IntPtr hwo, MM_WOM uMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2)
    {
      try {
        switch (uMsg) {
          case MM_WOM.MM_WOM_OPEN:
            // nothing to do
            break;

          case MM_WOM.MM_WOM_DONE:
            unsafe {
              WAVEHDR* pwh = (WAVEHDR*)dwParam1.ToPointer();
              Done((*pwh).dwUser.ToInt32());
            }
            break;

          case MM_WOM.MM_WOM_CLOSE:
            handle = IntPtr.Zero;
            break;
        }
      }
      catch {
        // ignore any exception
      }
    }

    private void Done(int index)
    {
      buffers[index].Done = true;

      if (resetting)
        return;

      if (finished) {
        if (Repeat && reader.BaseStream.CanSeek) {
          Rewind();
        }
        else {
          CheckPlayDone();
          return;
        }
      }

      WriteTo(buffers[index]);
    }

    private void CheckPlayDone()
    {
      var allDone = true;

      for (var i = 0; i < buffers.Length; i++) {
        if (!buffers[i].Done) {
          allDone = false;
          break;
        }
      }

      if (allDone) {
        ThreadPool.QueueUserWorkItem(delegate(object state) {
          OnPlayDone(EventArgs.Empty);
        });
      }
    }

    public void Play(Stream linearPcmStream)
    {
      CheckDisposed();

      finished = false;
      position = 0L;

      reader = new BinaryReader(linearPcmStream);

      if (linearPcmStream.CanSeek)
        initialOffset = linearPcmStream.Position;

      Play();
      Resume();
    }

    private void Play()
    {
      resetting = false;

      for (var i = 0; i < buffers.Length; i++) {
        WriteTo(buffers[i]);

        if (finished)
          break;
      }
    }

    private void WriteTo(WaveForm buffer)
    {
      var readingSamples = buffer.Size / format.nBlockAlign;
      byte[] data;

      data = reader.ReadBytes(readingSamples * format.nBlockAlign);

      var samples = data.Length / format.nBlockAlign;

      if (softVolume) {
        if (format.nChannels == 2) {
          if (format.wBitsPerSample == 16) {
            unsafe {
              fixed (byte* wav = data) {
                var wavs = (short*)wav;

                for (var sample = 0; sample < samples; sample++) {
                  *(wavs++) = (short)(((int)(*wavs) * volume.Left) >> 16);
                  *(wavs++) = (short)(((int)(*wavs) * volume.Right) >> 16);
                }
              }
            }
          }
          else {
            throw new NotImplementedException();
          }
        }
        else {
          throw new NotImplementedException();
        }
      }

      if (samples < readingSamples)
        finished = true;

      buffer.Write(data);

      position += data.Length;
    }

    public void Resume()
    {
      CheckDisposed();

      WinMMException.ThrowIfWaveOutError(winmm.waveOutRestart(handle));
    }

    public void Pause()
    {
      CheckDisposed();

      WinMMException.ThrowIfWaveOutError(winmm.waveOutPause(handle));
    }

    public void Stop()
    {
      StopCore();

      Rewind();
    }

    private void StopCore()
    {
      resetting = true;

      WinMMException.ThrowIfWaveOutError(winmm.waveOutReset(handle));

      for (var i = 0; i < buffers.Length; i++) {
        buffers[i].Unprepare();
      }
    }

    private void Rewind()
    {
      reader.BaseStream.Position = initialOffset;

      finished = false;

      position = 0L;
    }

    protected virtual void OnPlayDone(EventArgs e)
    {
      var ev = this.PlayDone;

      if (ev != null)
        ev(this, e);
    }

    private WaveOutVolume GetVolume()
    {
      uint volume;

      WinMMException.ThrowIfWaveOutError(winmm.waveOutGetVolume(handle, out volume));

      return WaveOutVolume.FromUInt32(volume);
    }

    private void SetVolume(WaveOutVolume volume)
    {
      WinMMException.ThrowIfWaveOutError(winmm.waveOutSetVolume(handle, (uint)volume));
    }

    private long GetPositionInBytes()
    {
      var mmt = new MMTIME(TIME.TIME_BYTES);

      WinMMException.ThrowIfWaveOutError(winmm.waveOutGetPosition(handle, ref mmt, MMTIME.Size));

      return mmt.u.cb;
    }

    private void CheckDisposed()
    {
      if (handle == IntPtr.Zero)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private IntPtr handle;
    private readonly WAVEFORMATEX format;
    private waveOutProc waveOutCallback;
    private WaveOutForm[] buffers;
    private WaveOutVolume volume;
    private readonly bool softVolume;
    private bool finished = false;
    private bool resetting = false;
    private long initialOffset;
    private long position;
    private BinaryReader reader;
  }
}
