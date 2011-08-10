// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2004-2011 smdn
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

namespace Smdn.Formats.Thbgm {
  public abstract class StreamFormat : IEquatable<StreamFormat> {
    public static readonly StreamFormat ThXX = new ThXXStreamFormat();
    public static readonly StreamFormat Th06 = new Th06StreamFormat();

    public static StreamFormat Create(string streamFilePath,
                                      int samplesPerSecond,
                                      int bitsPerSample,
                                      int channels)
    {
      if (samplesPerSecond <= 0)
        throw ExceptionUtils.CreateArgumentMustBeNonZeroPositive("samplesPerSecond", samplesPerSecond);
      if (bitsPerSample <= 0)
        throw ExceptionUtils.CreateArgumentMustBeNonZeroPositive("bitsPerSample", bitsPerSample);
      if (channels <= 0)
        throw ExceptionUtils.CreateArgumentMustBeNonZeroPositive("channels", channels);

      return new GenericStreamFormat(streamFilePath,
                                     samplesPerSecond,
                                     bitsPerSample,
                                     channels);
    }

    public abstract int SamplesPerSecond { get; }
    public abstract int BitsPerSample { get; }
    public abstract int Channels { get; }

    public virtual int BlockAlign {
      get { return (BitsPerSample / 8) * Channels; }
    }

    public virtual int BytesPerSecond {
      get { return SamplesPerSecond * BlockAlign; }
    }

    [CLSCompliant(false)]
    public virtual WAVEFORMATEX GetWaveFormatEx()
    {
      return WAVEFORMATEX.CreateLinearPcmFormat(SamplesPerSecond, BitsPerSample, Channels);
    }

    internal protected abstract string GetStreamFile(string thbgmPath, int trackNumber);

    public virtual long ToAlignedByteCount(TimeSpan timeSpan)
    {
      return AlignOffset(timeSpan.TotalSeconds * BytesPerSecond);
    }

    public virtual TimeSpan ToTimeSpan(double offset)
    {
      return TimeSpan.FromSeconds(AlignOffset(offset) / (double)BytesPerSecond);
    }

    public virtual TimeSpan ToTimeSpan(long offset)
    {
      return TimeSpan.FromSeconds(AlignOffset(offset) / (double)BytesPerSecond);
    }

    public virtual long AlignOffset(double offset)
    {
      return (long)(offset / BlockAlign) * BlockAlign;
    }

    public virtual long AlignOffset(long offset)
    {
      return (offset / BlockAlign) * BlockAlign;
    }
    
    public override bool Equals(object obj)
    {
      if (obj is StreamFormat)
        return Equals(obj as StreamFormat);
      else
        return false;
    }

    public virtual bool Equals(StreamFormat other)
    {
      if (other == null)
        return false;
      else if (StreamFormat.ReferenceEquals(this, other))
        return true;
      else if (this.SamplesPerSecond == other.SamplesPerSecond &&
          this.BitsPerSample == other.BitsPerSample &&
          this.Channels == other.Channels)
        return true;
      else
        return false;
    }

    public override int GetHashCode()
    {
      return SamplesPerSecond.GetHashCode() ^ BitsPerSample.GetHashCode() ^ Channels.GetHashCode();
    }

    private class ThXXStreamFormat : StreamFormat {
      public override int SamplesPerSecond {
        get { return 44100; }
      }

      public override int BitsPerSample {
        get { return 16; }
      }

      public override int Channels {
        get { return 2; }
      }

      internal protected override string GetStreamFile(string thbgmPath, int trackNumber)
      {
        return thbgmPath;
      }
    }

    private class Th06StreamFormat : ThXXStreamFormat {
      internal protected override string GetStreamFile(string thbgmPath, int trackNumber)
      {
        return Path.Combine(Path.GetDirectoryName(thbgmPath), string.Format("th06_{0:D2}.wav", trackNumber));
      }
    }

    private class GenericStreamFormat : StreamFormat {
      public override int SamplesPerSecond {
        get { return samplesPerSecond; }
      }

      public override int BitsPerSample {
        get { return bitsPerSample; }
      }

      public override int Channels {
        get { return channels; }
      }

      public GenericStreamFormat(string streamFilePath,
                                 int samplesPerSecond,
                                 int bitsPerSample,
                                 int channels)
      {
        this.streamFilePath = streamFilePath;
        this.samplesPerSecond = samplesPerSecond;
        this.bitsPerSample = bitsPerSample;
        this.channels = channels;
      }

      internal protected override string GetStreamFile(string thbgmPath, int trackNumber)
      {
        if (streamFilePath == null)
          return thbgmPath;
        else if (Path.IsPathRooted(streamFilePath))
          return streamFilePath;
        else
          return Path.Combine(Path.GetDirectoryName(thbgmPath), streamFilePath);
      }

      private string streamFilePath;
      private int samplesPerSecond;
      private int bitsPerSample;
      private int channels;
    }
  }
}
