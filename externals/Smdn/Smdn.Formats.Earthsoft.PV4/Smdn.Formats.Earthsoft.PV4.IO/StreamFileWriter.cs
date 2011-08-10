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
  public class StreamFileWriter : Smdn.IO.BigEndianBinaryWriter {
    public StreamFileWriter(Stream stream)
      : base(stream, false, 512)
    {
    }

    public long Seek(long offset, SeekOrigin origin)
    {
      return BaseStream.Seek(offset, origin);
    }

    public long SeekToHeader()
    {
      return BaseStream.Seek(0L, SeekOrigin.Begin);
    }

    public long SeekToFirstFrame()
    {
      return BaseStream.Seek(StreamFileHeaderData.Size, SeekOrigin.Begin);
    }

    public long SeekToFrame(IndexFileEntry frame)
    {
      return BaseStream.Seek(frame.FrameOffset, SeekOrigin.Begin);
    }

    public void Write(StreamFileHeaderData header)
    {
      header.GetBytes(Storage, 0);

      WriteUnchecked(Storage, 0, 512);

      WriteZero(StreamFileHeaderData.Size - 512L);
    }

    public void Write(StreamFileFrameData frameData)
    {
      CheckDisposed();

      var audio = frameData.Audio;
      var video = frameData.Video;

      BinaryConvertExtensions.GetBytesBE(audio.PrecedentSampleCountValue, Storage, 0);
      BinaryConvert          .GetBytesBE(audio.SampleCountValue,          Storage, 6);
      BinaryConvert          .GetBytesBE(audio.SamplingFrequencyValue,    Storage, 8);

      for (var offset = 12; offset < 256; offset++) {
        Storage[offset] = 0;
      }

      BinaryConvert.GetBytesBE(video.DisplayAspectHorizontalValue,  Storage, 256);
      BinaryConvert.GetBytesBE(video.DisplayAspectVerticalValue,    Storage, 258);
      Storage[260] = video.EncodingQualityValue;

      for (var offset = 261; offset < 384; offset++) {
        Storage[offset] = 0;
      }

      BinaryConvert.GetBytesBE(video.Block0Length, Storage, 384);
      BinaryConvert.GetBytesBE(video.Block1Length, Storage, 388);
      BinaryConvert.GetBytesBE(video.Block2Length, Storage, 392);
      BinaryConvert.GetBytesBE(video.Block3Length, Storage, 396);

      for (var offset = 400; offset < 512; offset++) {
        Storage[offset] = 0;
      }

      WriteUnchecked(Storage, 0, 512);

      // audio
      if (audio.Data == null)
        WriteZero(audio.AlignedDataLength);
      else
        WriteUnchecked(audio.Data, 0, (int)audio.AlignedDataLength);

      // video
      if (video.Data == null)
        WriteZero(video.AlignedDataLength);
      else
        WriteUnchecked(video.Data, 0, (int)video.AlignedDataLength);
    }
  }
}
