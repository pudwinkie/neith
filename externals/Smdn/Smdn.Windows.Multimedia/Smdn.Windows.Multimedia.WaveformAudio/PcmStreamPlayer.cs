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

using Smdn.Media;
using Smdn.Windows.Multimedia.Interop;

namespace Smdn.Windows.Multimedia.WaveformAudio {
  public class PcmStreamPlayer : Smdn.Media.PcmStreamPlayer {
    public override long PositionInBytes {
      get
      {
        if (device == null)
          return 0;
        else
          return device.Position;
      }
      set
      {
        if (device != null)
          device.Position = value;
      }
    }

    public override int Volume {
      get
      {
        var vol = (device == null) ? volume : device.Volume;

        // http://smdn.jp/misc/forum/ThbgmExtractor/#n2
        // XXX: not to use double
        return (int)Math.Round(100 * (vol.Left - WaveOutVolume.Min.Left) / (double)(WaveOutVolume.Max.Left - WaveOutVolume.Min.Left));
      }
      set {
        volume = new WaveOutVolume(((WaveOutVolume.Max.Left - WaveOutVolume.Min.Left) * value / 100) + WaveOutVolume.Min.Left);

        if (device != null)
          device.Volume = volume;
      }
    }

    public override bool Repeat {
      get
      {
        if (device == null)
          return repeat;
        else
          return device.Repeat;
      }
      set
      {
        if (device == null)
          repeat = value;
        else
          device.Repeat = value;
      }
    }

    ~PcmStreamPlayer()
    {
      Dispose(false);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing) {
        if (device != null) {
          device.Close();
          device = null;
        }
      }

      base.Dispose(disposing);
    }

    private uint FindDevice(WAVEFORMATEX format)
    {
      uint? deviceId = null;

      WaveOutDevice.EnumerateDevices(delegate(uint id, WAVEOUTCAPS caps) {
        if (deviceId == null && WaveOutDevice.IsFormatSupported(id, format))
          deviceId = id;
      });

      if (deviceId == null)
        throw new NotSupportedException("unsupported pcm format");
      else
        return deviceId.Value;
    }

    protected override void DoPlaySync(Stream linearPcmStream, WAVEFORMATEX format)
    {
      throw new NotImplementedException();
    }

    protected override void DoPlayAsync(Stream linearPcmStream, WAVEFORMATEX format)
    {
      if (device != null && !WAVEFORMATEX.Equals(device.Format, format)) {
        // recreate device with specified format
        device.Close();
        device = null;
      }

      if (device == null)
        device = new WaveOutDevice(FindDevice(format), format);

      device.Repeat = repeat;
      device.PlayDone += delegate {
        OnPlayDone(EventArgs.Empty);
      };

      device.Volume = volume;
      device.Play(linearPcmStream);
    }

    protected override void DoPause()
    {
      device.Pause();
    }

    protected override void DoResume()
    {
      device.Resume();
    }

    protected override void DoStop()
    {
      device.Stop();
    }

    private WaveOutDevice device;
    private WaveOutVolume volume = WaveOutVolume.Max;
    private bool repeat;
  }
}
