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

using Smdn.IO;

namespace Smdn.Formats.Thbgm {
  public class BgmStream : Stream {
    public Stream InnerStream {
      get { return stream; }
    }

    public override bool CanSeek {
      get { return !IsClosed && stream.CanSeek; }
    }

    public override bool CanRead {
      get { return !IsClosed && stream.CanRead; }
    }

    public override bool CanWrite {
      get { return /*!IsClosed &&*/ false; }
    }

    public override bool CanTimeout {
      get { return !IsClosed && stream.CanTimeout; }
    }

    private bool IsClosed {
      get { return stream == null; }
    }

    public int TimesToRepeat {
      get { return timesToRepeat; }
      set
      {
        if (value < 0)
          throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("TimesToRepeat", value);
        timesToRepeat = value;
        length = trackInfo.IntroLength + trackInfo.RepeatLength * timesToRepeat;
      }
    }

    public virtual int RepeatedTimes {
      get { return repeatedTimes; }
      set {
        if (value < 0)
          throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("RepeatedTimes", value);
        // no need to seek
        repeatedTimes = value;
      }
    }

    public StreamFormat Format {
      get { return trackInfo.StreamFormat; }
    }

    internal TrackInfo TrackInfo {
      get { return trackInfo; }
    }

    public override long Length {
      get { return length; }
    }

    public override long Position {
      get { return ConvertOffsetActualToVirtual(stream.Position); }
      set { stream.Position = ConvertOffsetVirtualToActual(value); }
    }

    protected internal BgmStream(string path, TrackInfo trackInfo, int timesToRepeat)
      : this(path, trackInfo, timesToRepeat, false)
    {
    }

    protected internal BgmStream(string path, TrackInfo trackInfo, int timesToRepeat, bool loadOnMemory)
    {
      if (trackInfo == null)
        throw new ArgumentNullException("trackInfo");

      var innerStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

      if (innerStream.Length <= trackInfo.IntroOffset)
        throw new ArgumentException("the intro offset of track must be less than the length of stream", "trackInfo");

      if (innerStream.Length < trackInfo.IntroOffset + trackInfo.IntroLength + trackInfo.RepeatLength)
        throw new ArgumentException("the end of track must be before the end of stream", "trackInfo");

      var partialStream = new PartialStream(innerStream,
                                            trackInfo.IntroOffset,
                                            trackInfo.IntroLength + trackInfo.RepeatLength,
                                            true /*readOnly*/,
                                            false /*leaveInnerStreamOpen*/);

      if (loadOnMemory)
        this.stream = new PersistentCachedStream(partialStream, trackInfo.StreamFormat.BytesPerSecond, false);
      else
        this.stream = partialStream;

      this.trackInfo = trackInfo;
      this.TimesToRepeat = timesToRepeat;

      SeekToIntroStart();
    }

    public override void Close()
    {
      if (stream != null) {
        stream.Close();
        stream = null;
      }
    }

    public override void SetLength(long @value)
    {
      CheckDisposed();

      throw ExceptionUtils.CreateNotSupportedSettingStreamLength();
    }

    public long SeekToIntroStart()
    {
      CheckDisposed();

      repeatedTimes = 0;

      return ConvertOffsetActualToVirtual(stream.Seek(0, SeekOrigin.Begin));
    }

    public long SeekToLoopStart()
    {
      CheckDisposed();

      return SeekToLoopStart(0);
    }

    public long SeekToLoopStart(int repeat)
    {
      CheckDisposed();

      repeatedTimes = repeat;

      return ConvertOffsetActualToVirtual(stream.Seek(trackInfo.IntroLength, SeekOrigin.Begin));
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      CheckDisposed();

      switch (origin) {
        case SeekOrigin.Begin:
          return ConvertOffsetActualToVirtual(stream.Seek(ConvertOffsetVirtualToActual(offset), SeekOrigin.Begin));
        case SeekOrigin.Current:
          return ConvertOffsetActualToVirtual(stream.Seek(offset, SeekOrigin.Current));
        case SeekOrigin.End:
          return ConvertOffsetActualToVirtual(stream.Seek(ConvertOffsetVirtualToActual(length + offset), SeekOrigin.Begin));
        default:
          throw ExceptionUtils.CreateArgumentMustBeValidEnumValue("origin", origin);
      }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      CheckDisposed();

      if (buffer == null)
        throw new ArgumentNullException("array");
      if (count < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("count", count);
      if (offset < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("offset", offset);
      if (buffer.Length - count < offset)
        throw ExceptionUtils.CreateArgumentAttemptToAccessBeyondEndOfArray("offset", buffer, offset, count);

      var read = 0;

      for (;;) {
        if (count == 0)
          return read;
        else if (length <= Position)
          // end of 'virtual' stream
          return read;

        var r = stream.Read(buffer, offset, count);

        if (r < count)
          SeekToLoopStart(repeatedTimes + 1);

        read   += r;
        offset += r;
        count  -= r;
      }
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      CheckDisposed();

      throw ExceptionUtils.CreateNotSupportedWritingStream();
    }

    public override void Flush()
    {
      CheckDisposed();

      throw ExceptionUtils.CreateNotSupportedWritingStream();
    }

    private long ConvertOffsetActualToVirtual(long actualOffset)
    {
      return actualOffset + trackInfo.RepeatLength * repeatedTimes;
    }

    private long ConvertOffsetVirtualToActual(long virtualOffset)
    {
      if (trackInfo.IntroLength < virtualOffset)
        repeatedTimes = (int)((virtualOffset - trackInfo.IntroLength) / trackInfo.RepeatLength);
      else
        repeatedTimes = 0;

      return virtualOffset - trackInfo.RepeatLength * repeatedTimes;
    }

    private void CheckDisposed()
    {
      if (IsClosed)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private Stream stream;
    private /*readonly*/ TrackInfo trackInfo;
    private long length;
    private int timesToRepeat; // 0 means only intro part
    private int repeatedTimes = 0;
  }
}
