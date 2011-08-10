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

using Smdn.Interop;
using Smdn.Windows.Multimedia.Interop;

namespace Smdn.Windows.Multimedia.WaveformAudio {
  public unsafe class WaveForm : IDisposable {
    public IntPtr UserData {
      get { CheckDisposed(); return (*pwh).dwUser; }
    }

    public int Size {
      get { CheckDisposed(); return data.Size; }
    }

    internal WaveForm(IWaveIODevice device, int bufferSize, IntPtr userData)
    {
      if (device == null)
        throw new ArgumentNullException("device");

      this.device = device;
      this.wh = new HeapMemoryBuffer((int)WAVEHDR.Size);
      this.data = new HeapMemoryBuffer(bufferSize);
      this.pwh = (WAVEHDR*)wh;
      this.userData = userData;
    }

    ~WaveForm()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing) {
        if (wh != null) {
          wh.Dispose();
          wh = null;
        }

        if (data != null) {
          data.Dispose();
          data = null;
        }
      }

      device = null;
    }

    protected internal virtual void Prepare()
    {
      CheckDisposed();

      if (((*pwh).dwFlags & WAVEHDR_FLAG.WHDR_PREPARED) != 0)
        return;

      (*pwh).lpData = (IntPtr)data;
      (*pwh).dwBufferLength = (uint)data.Size;
      (*pwh).dwUser = userData;
      (*pwh).dwFlags = 0;

      if (device is WaveOutDevice)
        WinMMException.ThrowIfWaveOutError(winmm.waveOutPrepareHeader(device.Handle, pwh, WAVEHDR.Size));
      else
        throw new NotSupportedException();
    }

    protected internal virtual void Unprepare()
    {
      CheckDisposed();

      if (device is WaveOutDevice)
        WinMMException.ThrowIfWaveOutError(winmm.waveOutUnprepareHeader(device.Handle, pwh, WAVEHDR.Size));
      else
        throw new NotSupportedException();
    }

    protected internal virtual void Write(byte[] writeData)
    {
      Prepare();

      var bytesToWrite = Math.Min(data.Size, writeData.Length);

      data.Write(writeData, 0, bytesToWrite);

      if (bytesToWrite < data.Size)
        data.Set(0, bytesToWrite, data.Size - bytesToWrite);

      if (device is WaveOutDevice)
        WinMMException.ThrowIfWaveOutError(winmm.waveOutWrite(device.Handle, pwh, WAVEHDR.Size));
      else
        throw new NotSupportedException();
    }

    private void CheckDisposed()
    {
      if (device == null)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private IWaveIODevice device;
    private HeapMemoryBuffer wh;
    private HeapMemoryBuffer data;
    private readonly IntPtr userData;
    private readonly WAVEHDR* pwh;
  }
}