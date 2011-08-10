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

namespace Smdn.Formats.Earthsoft.PV4.IO {
  public class StreamFileReader : Smdn.IO.BigEndianBinaryReader {
    public StreamFileReader(string file)
      : this(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
    {
    }

    public StreamFileReader(Stream stream)
      : base(stream, false, 512)
    {
    }

    public long Seek(long offset, SeekOrigin origin)
    {
      return BaseStream.Seek(offset, origin);
    }

    public long SeekToHeader()
    {
      return BaseStream.Seek(0, SeekOrigin.Begin);
    }

    public long SeekToFirstFrame()
    {
      return BaseStream.Seek(StreamFileHeaderData.Size, SeekOrigin.Begin);
    }

    public long SeekToFrame(IndexFileEntry frame)
    {
      return BaseStream.Seek(frame.FrameOffset, SeekOrigin.Begin);
    }

    public StreamFileHeaderData ReadHeader()
    {
      if (BaseStream.Position != 0L)
        SeekToHeader();

      ReadBytesUnchecked(Storage, 0, 512, true);

      var header = new StreamFileHeaderData(Storage, 0);

      BaseStream.Seek(StreamFileHeaderData.Size - 512L, SeekOrigin.Current);

      return header;
    }

    public byte[] ReadHeaderAsByteArray()
    {
      if (BaseStream.Position != 0L)
        SeekToHeader();

      return ReadBytes(StreamFileHeaderData.Size);
    }

    public StreamFileFrameData ReadFrameData()
    {
      return ReadFrameData(true, true);
    }

    public StreamFileFrameData ReadFrameData(bool readAudio, bool readVideo)
    {
      StreamFileAudioData audio = null;
      StreamFileVideoData video = null;

      if (ReadFrameData(ref audio, readAudio, ref video, readVideo))
        return new StreamFileFrameData(audio, video);
      else
        return null;
    }

    public bool ReadFrameData(StreamFileFrameData buffer)
    {
      return ReadFrameData(buffer, true, true);
    }

    public bool ReadFrameData(StreamFileFrameData buffer, bool readAudio, bool readVideo)
    {
      if (buffer == null)
        throw new ArgumentNullException("buffer");

      var audio = buffer.Audio;
      var video = buffer.Video;

      return ReadFrameData(ref audio, readAudio, ref video, readVideo);
    }

    private bool ReadFrameData(ref StreamFileAudioData audio, bool readAudio, ref StreamFileVideoData video, bool readVideo)
    {
      CheckDisposed();

      if (ReadBytesUnchecked(Storage, 0, StreamFileFrameData.FixedDataSize, false) != StreamFileFrameData.FixedDataSize)
        return false;

      var buffered = (video != null);

      if (audio == null)
        audio = new StreamFileAudioData();
      if (video == null)
        video = new StreamFileVideoData();

      audio.PrecedentSampleCountValue = BinaryConvertExtensions.ToUInt48BE(Storage, 0);
      audio.SampleCountValue          = BinaryConvert          .ToUInt16BE(Storage, 6);
      audio.SamplingFrequencyValue    = BinaryConvert          .ToUInt32BE(Storage, 8);

      video.DisplayAspectHorizontalValue  = BinaryConvert.ToUInt16BE(Storage, 256);
      video.DisplayAspectVerticalValue    = BinaryConvert.ToUInt16BE(Storage, 258);
      video.EncodingQualityValue          = Storage[260];

      video.Block0Length = BinaryConvert.ToUInt32BE(Storage, 384);
      video.Block1Length = BinaryConvert.ToUInt32BE(Storage, 388);
      video.Block2Length = BinaryConvert.ToUInt32BE(Storage, 392);
      video.Block3Length = BinaryConvert.ToUInt32BE(Storage, 396);

      if ((video.Block0Length & 0x0000001f) != 0) throw new InvalidDataException("length of video block #0 must be n*32");
      if ((video.Block1Length & 0x0000001f) != 0) throw new InvalidDataException("length of video block #1 must be n*32");
      if ((video.Block2Length & 0x0000001f) != 0) throw new InvalidDataException("length of video block #2 must be n*32");
      if ((video.Block3Length & 0x0000001f) != 0) throw new InvalidDataException("length of video block #3 must be n*32");

      // audio data
      var audioDataLength = audio.SampleCountValue << 2; // 2ch * 16bit
      //audio.AlignedDataLength = (audioDataLength <= 4096 - 512) ? 4096 - 512 : 4096 - 512 + ((audioDataLength - 512) % 4096);

      audio.AlignedDataLength = (audioDataLength <= 3584L)
        ? 3584L
        : 3584L + ((audioDataLength - 512) & ~4095); // XXX

      if (readAudio) {
        if (audio.Data == null || audio.Data.Length < audio.AlignedDataLength)
          audio.Data = new byte[audio.AlignedDataLength];

        ReadBytesUnchecked(audio.Data, 0, (int)audio.AlignedDataLength, true);
      }
      else {
        audio.Data = null;

        BaseStream.Seek(audio.AlignedDataLength, SeekOrigin.Current);
      }

      // video data
      var videoDataLength = video.Block0Length + video.Block1Length + video.Block2Length + video.Block3Length;

      video.AlignedDataLength = (videoDataLength + 4095L) & ~4095L;

      if (readVideo) {
        if (video.Data == null || video.Data.Length < video.AlignedDataLength) {
          if (buffered)
            video.Data = new byte[((video.AlignedDataLength >> 20) + 1) << 20]; // 0x100000
          else
            video.Data = new byte[video.AlignedDataLength];
        }

        if (ReadBytesUnchecked(video.Data, 0, (int)video.AlignedDataLength, false) < videoDataLength) // allow to ignore alignment
          throw new EndOfStreamException();
      }
      else {
        video.Data = null;

        BaseStream.Seek(video.AlignedDataLength, SeekOrigin.Current);
      }

      return true;
    }

    public byte[] ReadFrameDataAsByteArray(IndexFileEntry frame)
    {
      SeekToFrame(frame);

      return ReadBytes(frame.FrameSize);
    }
  }
}
