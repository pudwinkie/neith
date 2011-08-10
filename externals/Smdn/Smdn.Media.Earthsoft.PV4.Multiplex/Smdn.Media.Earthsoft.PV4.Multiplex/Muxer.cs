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

using Smdn.Formats.Earthsoft.PV4;
using Smdn.Formats.Riff.Wave;

namespace Smdn.Media.Earthsoft.PV4.Multiplex {
  public class Muxer {
    public DV DV {
      get { return dv; }
    }

    public Muxer(DV dv)
    {
      if (dv == null)
        throw new ArgumentNullException("dv");

      this.dv = dv;
    }

    public void MuxAudio(StreamFileFrameData frameData, LinearPcmReader reader, TimeSpan frameTime, long precedentAudioSampleCount)
    {
      MuxAudio(frameData, reader, dv.TimeSpanToVideoFrame(frameTime), precedentAudioSampleCount);
    }

    public void MuxAudio(StreamFileFrameData frameData, LinearPcmReader reader, int frameNumber, long precedentAudioSampleCount)
    {
      var sampleCount = frameData.Audio.SampleCount;

      frameData.SetAudio(precedentAudioSampleCount, sampleCount, reader.SamplesPerSecond);

      var block = frameData.Audio.Block;
      var offset = block.Offset;

      while (0 < sampleCount--) {
        ushort left, right;

        reader.ReadSample(out left, out right);

        block.Array[offset++] = (byte)(left >> 8);
        block.Array[offset++] = (byte)(left & 0xff);
        block.Array[offset++] = (byte)(right >> 8);
        block.Array[offset++] = (byte)(right & 0xff);
      }
    }

    private readonly DV dv;
  }
}
