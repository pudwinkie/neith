// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2008-2011 smdn
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

namespace Smdn.Formats.Riff.Wave {
  public abstract class LinearPcmReader : Smdn.IO.BinaryReader {
    public static LinearPcmReader Create(Stream baseStream, WAVEFORMATEX format)
    {
      return Create(baseStream, format, false);
    }

    public static LinearPcmReader Create(Stream baseStream, WAVEFORMATEX format, bool leaveBaseStreamOpen)
    {
      if (format.wFormatTag != WAVE_FORMAT_TAG.WAVE_FORMAT_PCM)
        throw new ArgumentException("stream must be linear PCM");

      if (format.nChannels == 1) {
        if (format.wBitsPerSample == 8)
          return new Mono8BitLinearPcmReader(baseStream, format, leaveBaseStreamOpen);
        else if (format.wBitsPerSample == 16)
          return new Mono16BitLinearPcmReader(baseStream, format, leaveBaseStreamOpen);
      }
      else if (format.nChannels == 2) {
        if (format.wBitsPerSample == 8)
          return new Stereo8BitLinearPcmReader(baseStream, format, leaveBaseStreamOpen);
        else if (format.wBitsPerSample == 16)
          return new Stereo16BitLinearPcmReader(baseStream, format, leaveBaseStreamOpen);
      }

      throw new NotSupportedException("unsupported format");
    }

    public WAVEFORMATEX Format {
      get { return format; }
    }

    public int Channels {
      get { return format.nChannels; }
    }

    public long SamplesPerSecond {
      get { return (long)format.nSamplesPerSec; }
    }

    public int BitsPerSample {
      get { return format.wBitsPerSample; }
    }

    public long SampleLength {
      get { return BaseStream.Length / format.nBlockAlign; }
    }

    public long SamplePosition {
      get { return BaseStream.Position / format.nBlockAlign; }
      set { BaseStream.Position = value * format.nBlockAlign; }
    }

    internal protected LinearPcmReader(Stream baseStream, WAVEFORMATEX format, bool leaveBaseStreamOpen)
      : base(baseStream, leaveBaseStreamOpen)
    {
      this.format = format;
    }

    public virtual long SeekSample(long samples, SeekOrigin origin)
    {
      return BaseStream.Seek(samples * format.nBlockAlign, origin);
    }

    public virtual byte[] ReadSample()
    {
      var buffer = new byte[format.nBlockAlign];

      ReadBytesUnchecked(buffer, 0, buffer.Length, true);

      return buffer;
    }

    public virtual void ReadSample(out short left, out short right)
    {
      ushort l, r;

      ReadSample(out l, out r);

      unchecked {
        left  = (short)l;
        right = (short)r;
      }
    }

    public abstract void ReadSample(out ushort left, out ushort right);

    private WAVEFORMATEX format;
  }
}
