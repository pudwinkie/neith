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
using System.Text;

namespace Smdn.Formats.Thbgm {
  public class WaveStream : Smdn.IO.ExtendStream {
    public static class WaveFourCC {
      public static readonly FourCC Riff = new FourCC("RIFF");
      public static readonly FourCC List = new FourCC("LIST");

      public static readonly FourCC JunkChunk = new FourCC("JUNK");

      public static readonly FourCC Wave = new FourCC("WAVE");
      public static readonly FourCC WaveFtmChunk = new FourCC("fmt ");
      public static readonly FourCC WaveDataChunk = new FourCC("data");

      public static readonly FourCC InfoList = new FourCC("INFO");
      public static readonly FourCC InfoInamChunk = new FourCC("INAM");
      public static readonly FourCC InfoIartChunk = new FourCC("IART");
      public static readonly FourCC InfoIstrChunk = new FourCC("ISTR");
      public static readonly FourCC InfoIprdChunk = new FourCC("IPRD");
    }

    /*
     * class members
     */
    public static WaveStream CreateFrom(BgmStream bgmStream)
    {
      return CreateFrom(bgmStream, false);
    }

    public static WaveStream CreateFrom(BgmStream bgmStream, bool leaveInnerStreamOpen)
    {
      return CreateFrom(bgmStream, leaveInnerStreamOpen, true, 0);
    }

    public static WaveStream CreateFrom(BgmStream bgmStream, bool insertInfo, int dataAlignment)
    {
      return CreateFrom(bgmStream, false, insertInfo, dataAlignment);
    }

    public static WaveStream CreateFrom(BgmStream bgmStream, bool leaveInnerStreamOpen, bool insertInfo, int dataAlignment)
    {
      return new WaveStream(bgmStream, leaveInnerStreamOpen, insertInfo, dataAlignment);
    }

    public static byte[] CreateRiffWaveHeaderFrom(BgmStream bgmStream, bool insertInfo, int dataAlignment)
    {
      if (UInt32.MaxValue < bgmStream.Length)
        throw new NotSupportedException("can't create RIFF that is larger than 4GB");
      if (dataAlignment < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("dataAlignment", dataAlignment);

      // create info chunk
      byte[] infoChunks = null;

      if (insertInfo) {
        using (var stream = new MemoryStream()) {
          var writer = new Smdn.IO.LittleEndianBinaryWriter(stream);

          foreach (var chunk in new[] {
            new {Chunk = WaveFourCC.InfoInamChunk, Data = bgmStream.TrackInfo.Title},
            new {Chunk = WaveFourCC.InfoIprdChunk, Data = bgmStream.TrackInfo.Product.Title},
            new {Chunk = WaveFourCC.InfoIartChunk, Data = bgmStream.TrackInfo.Product.Creator},
            new {Chunk = WaveFourCC.InfoIstrChunk, Data = bgmStream.TrackInfo.Product.Creator}, // XXX
          }) {
            var data = Encoding.Default.GetBytes((chunk.Data ?? string.Empty) + "\0");

            if ((data.Length & 0x1) == 0x1)
              Array.Resize(ref data, data.Length + 1);

            writer.Write(chunk.Chunk);
            writer.Write((UInt32)data.Length);
            writer.Write(data);
          }

          writer.Close();

          infoChunks = stream.ToArray();
        }
      }

      var headerSize = 56 + (infoChunks == null ? 0 : infoChunks.Length);

      using (var stream = new MemoryStream(headerSize)) {
        var writer = new Smdn.IO.LittleEndianBinaryWriter(stream);

        // RIFF
        writer.Write(WaveFourCC.Riff);
        writer.Write((UInt32)0); // ckSize place holder
        writer.Write(WaveFourCC.Wave);

        // 'fmt ' chunk
        writer.Write(WaveFourCC.WaveFtmChunk);
        writer.Write((UInt32)0x00000010);
        writer.Write((UInt16)Smdn.Media.WAVE_FORMAT_TAG.WAVE_FORMAT_PCM);
        writer.Write((UInt16)bgmStream.Format.Channels);
        writer.Write((UInt32)bgmStream.Format.SamplesPerSecond);
        writer.Write((UInt32)(bgmStream.Format.SamplesPerSecond * bgmStream.Format.BlockAlign)); // nAvgBytesPerSecond
        writer.Write((UInt16)bgmStream.Format.BlockAlign);
        writer.Write((UInt16)bgmStream.Format.BitsPerSample);

        // 'INFO' list
        if (infoChunks != null) {
          writer.Write(WaveFourCC.List);
          writer.Write((UInt32)(infoChunks.Length + 4));
          writer.Write(WaveFourCC.InfoList);
          writer.Write(infoChunks);
        }

        // 'JUNK' chunk
        if (0 < dataAlignment) {
          var padding = dataAlignment - ((writer.BaseStream.Position + 8) % dataAlignment);

          writer.Write(WaveFourCC.JunkChunk);
          writer.Write((UInt32)padding);
          writer.BaseStream.Seek(padding, SeekOrigin.Current);
        }

        // 'data' chunk
        writer.Write(WaveFourCC.WaveDataChunk);
        writer.Write((UInt32)bgmStream.Length);

        // update RIFF ckSize
        var headerLength = writer.BaseStream.Position;

        writer.BaseStream.Position = 4;

        writer.Write((UInt32)(headerLength - 4 + bgmStream.Length));

        writer.Close();

        return stream.ToArray();
      }
    }

    /*
     * instance members
     */
    private WaveStream(BgmStream innerStream, bool leaveInnerStreamOpen, bool insertInfo, int dataAlignment)
      : base(innerStream, CreateRiffWaveHeaderFrom(innerStream, insertInfo, dataAlignment), null, leaveInnerStreamOpen)
    {
    }
  }
}
