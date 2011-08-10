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

using Smdn.Mathematics;

namespace Smdn.Formats.Earthsoft.PV4 {
  public sealed class StreamFileAudioData {
    internal UInt48 PrecedentSampleCountValue;
    internal UInt16 SampleCountValue;
    internal UInt32 SamplingFrequencyValue;
    internal byte[] Data; // can be null
    internal long AlignedDataLength;

    public long PrecedentSampleCount {
      get { return PrecedentSampleCountValue.ToInt64(); }
    }

    public int SampleCount {
      get { return SampleCountValue; }
    }

    public long SamplingFrequency {
      get { return SamplingFrequencyValue; }
    }

    public Fraction SamplingRate {
      get { return new Fraction(SamplingFrequencyValue, 1L); }
    }

    public bool DataAvailable {
      get { return Data != null; }
    }

    public ArraySegment<byte> Block {
      get
      {
        if (Data == null)
          throw new InvalidOperationException("audio data is not read");

        return new ArraySegment<byte>(Data, 0, SampleCountValue << 2);
      }
    }

    internal StreamFileAudioData()
    {
    }

    internal StreamFileAudioData Clone()
    {
      var cloned = (StreamFileAudioData)MemberwiseClone();

      cloned.Data = new byte[this.Data.Length];

      Buffer.BlockCopy(this.Data, 0, cloned.Data, 0, this.Data.Length);

      return cloned;
    }

    public override string ToString()
    {
      return string.Format("{{PrecedentSampleCount={0}, SampleCount={1}, SamplingFrequency={2}}}",
                           PrecedentSampleCountValue,
                           SampleCountValue,
                           SamplingFrequencyValue);
    }
  }
}

