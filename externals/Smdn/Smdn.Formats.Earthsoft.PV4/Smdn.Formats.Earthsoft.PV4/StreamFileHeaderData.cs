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

using Smdn.Formats.Earthsoft.PV4.IO;

namespace Smdn.Formats.Earthsoft.PV4 {
  // ファイルフォーマット資料
  // http://earthsoft.jp/PV/tech-file.html

  // 1. ストリームファイル (*.dv)
  //   ひとつの「ヘッダデータ」の後に、ひとつ以上の「音声・映像フレームデータ」が続きます。
  //   1. ヘッダデータ
  public sealed class StreamFileHeaderData : IEquatable<StreamFileHeaderData> {
    // オフセット  バイト数  説明
    // 0  1   'P'
    // 1  1   'V'
    // 2  1   '3'
    public static readonly ByteString StreamPreamble = ByteString.CreateImmutable("PV3");

    // 3  1   コーデックバージョン (現在は 2)
    public Byte CodecVersion;

    // 4  1   映像の水平ピクセル数÷16
    public Byte HorizontalPixels;

    // 5  1   映像の垂直ピクセル数÷8
    public Byte VerticalPixels;

    // 6  1   各種フラグ
    // [0ビット] 0:インターレース / 1:プログレッシブ
    public FrameScanning FrameScanning;

    // 7  249   予約
    // reserved;

    // 256  2×128   量子化テーブル (輝度・色差)
    private QuantizerTable[] quantizationTables = new QuantizerTable[] {
      new QuantizerTable(),
      new QuantizerTable(),
    };

    public QuantizerTable LuminanceQuantizerTable {
      get { return quantizationTables[0]; }
    }

    public QuantizerTable ChrominanceQuantizerTable {
      get { return quantizationTables[1]; }
    }

    // 512  15872   予約
    // 16384  -   最初の音声・映像フレームデータの始まり
    public static readonly int Size = 16384;

    public StreamFileHeaderData()
    {
    }

    internal StreamFileHeaderData(byte[] bytes, int startIndex)
    {
      // オフセット  バイト数  説明
      // 0  1   'P'
      // 1  1   'V'
      // 2  1   '3'
      if (!StreamPreamble.IsPrefixOf(new ArraySegment<byte>(bytes, startIndex, bytes.Length - startIndex)))
        throw new InvalidDataException("invalid file format (probably stream is not PV3)");

      // 3  1   コーデックバージョン (現在は 2)
      CodecVersion = bytes[startIndex + 3];

      if (CodecVersion != 2)
        throw new NotSupportedException("unsupported codec version");

      // 4  1   映像の水平ピクセル数÷16
      HorizontalPixels = bytes[startIndex + 4];

      // 5  1   映像の垂直ピクセル数÷8
      VerticalPixels = bytes[startIndex + 5];

      // 6  1   各種フラグ
      // [0ビット] 0:インターレース / 1:プログレッシブ
      switch (bytes[startIndex + 6] & 0x01) {
        case 0: FrameScanning = FrameScanning.Interlaced; break;
        case 1: FrameScanning = FrameScanning.Progressive; break;
        default: FrameScanning = FrameScanning.Unknown; break;
      }

      // 7  249   予約

      // 256  2×128   量子化テーブル (輝度・色差)
      startIndex += 256;

      for (var i = 0; i < quantizationTables.Length; i++) {
        for (var index = 0; index < 64; index++) {
          quantizationTables[i][index] = BinaryConvert.ToUInt16BE(bytes, startIndex);

          startIndex += 2;
        }
      }

      // 512  15872   予約

      // 16384  -   最初の音声・映像フレームデータの始まり
    }

    internal void GetBytes(byte[] bytes, int startIndex)
    {
      Buffer.BlockCopy(StreamPreamble.Segment.Array,
                       StreamPreamble.Segment.Offset,
                       bytes,
                       startIndex,
                       StreamPreamble.Segment.Count);

      bytes[startIndex + 3] = CodecVersion;
      bytes[startIndex + 4] = HorizontalPixels;
      bytes[startIndex + 5] = VerticalPixels;
      bytes[startIndex + 6] = (FrameScanning == FrameScanning.Interlaced) ? (Byte)0x00 : (Byte)0x01;

      for (startIndex = 7; startIndex < 256; startIndex++) {
        bytes[startIndex] = 0;
      }

      for (var i = 0; i < quantizationTables.Length; i++) {
        for (var y = 0; y < 8; y++)  {
          for (var x = 0; x < 8; x++)  {
            BinaryConvert.GetBytesBE(quantizationTables[i][x, y], bytes, startIndex);

            startIndex += 2;
          }
        }
      }
    }

    public override bool Equals(object obj)
    {
      if (obj is StreamFileHeaderData)
        return Equals(obj as StreamFileHeaderData);
      else
        return false;
    }

    public bool Equals(StreamFileHeaderData other)
    {
      return (this.CodecVersion == other.CodecVersion &&
              this.HorizontalPixels == other.HorizontalPixels &&
              this.VerticalPixels == other.VerticalPixels &&
              this.FrameScanning == other.FrameScanning &&
              this.LuminanceQuantizerTable.Equals(other.LuminanceQuantizerTable) &&
              this.ChrominanceQuantizerTable.Equals(other.ChrominanceQuantizerTable));
    }

    public override int GetHashCode()
    {
      return ((CodecVersion << 24) | (HorizontalPixels << 16) | (VerticalPixels << 8) | (int)FrameScanning) ^
        LuminanceQuantizerTable.GetHashCode() ^
        ChrominanceQuantizerTable.GetHashCode();
    }

    public override string ToString()
    {
      return string.Format("{{CodecVersion={0}, HorizontalPixels={1}({2}x16), VerticalPixels={3}({4}x8), FrameScanning={5}}}",
                           CodecVersion,
                           HorizontalPixels << 4,
                           HorizontalPixels,
                           VerticalPixels << 3,
                           VerticalPixels,
                           FrameScanning);
    }
  }
}
