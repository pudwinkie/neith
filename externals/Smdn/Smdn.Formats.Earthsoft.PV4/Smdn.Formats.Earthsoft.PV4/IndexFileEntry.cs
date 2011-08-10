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
using System.Runtime.InteropServices;

using Smdn.IO;

namespace Smdn.Formats.Earthsoft.PV4 {
  // ファイルフォーマット資料
  // http://earthsoft.jp/PV/tech-file.html

  // 2. インデックスファイル (*.dvi)
  [StructLayout(LayoutKind.Explicit, Pack = 1)]
  public struct IndexFileEntry :
    IEquatable<IndexFileEntry>
  {
    //   オフセット  バイト数  説明
    //   0  4   音声・映像フレームデータの開始オフセット÷4096
    [FieldOffset(0)] private UInt32 frameOffset;
    //   4  2   音声・映像フレームデータのサイズ÷4096
    [FieldOffset(4)] private UInt16 frameSize;
    //   6  6   音声・映像フレームデータの (※1) と同内容
    // => 先頭からひとつ前の「音声・映像フレーム」までの積算音声フレーム数 (※1)
    [FieldOffset(6)] private UInt48 precedentAudioSampleCount;
    //   12   2   音声・映像フレームデータの (※2) と同内容
    // => 音声フレーム数 (※2)
    [FieldOffset(12)] private UInt16 audioSampleCount;
    //   14   1   音声・映像フレームデータの (※3) と同内容
    // => エンコード品質 (※3)
    [FieldOffset(14)] public Byte EncodingQuality;
    //   15   1   予約
    [FieldOffset(15)] public Byte Reserved;
    //   16×n   -   次の「音声・映像フレーム」に関するインデックスデータの始まり
    public static readonly int Size = 16;

    [FieldOffset(0)] private long field0;
    [FieldOffset(8)] private long field1;

    public static readonly IndexFileEntry Empty = new IndexFileEntry();

    public long FrameOffset {
      get { return (long)frameOffset << 12; }
      set { frameOffset = checked((UInt32)((value + (1 << 12) - 1) >> 12)); }
    }

    public int FrameSize {
      get { return (int)frameSize << 12; }
      set { frameSize = checked((UInt16)((value + (1 << 12) - 1) >> 12)); }
    }

    public long PrecedentAudioSampleCount {
      get { return (long)precedentAudioSampleCount; }
      set { precedentAudioSampleCount = checked((UInt48)value); }
    }

    public int AudioSampleCount {
      get { return (int)audioSampleCount; }
      set { audioSampleCount = checked((UInt16)value); }
    }

    public IndexFileEntry(long frameOffset, int frameSize)
      : this()
    {
      FrameOffset = frameOffset;
      FrameSize = frameSize;
    }

    public IndexFileEntry(byte[] bytes, int startIndex)
      : this()
    {
      if (bytes == null)
        throw new ArgumentNullException("bytes");
      if (startIndex < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("startIndex", startIndex);
      if (bytes.Length - Size < startIndex)
        throw ExceptionUtils.CreateArgumentAttemptToAccessBeyondEndOfArray("startIndex", bytes, startIndex, Size);

      frameOffset               = BinaryConvert          .ToUInt32BE(bytes, startIndex +  0);
      frameSize                 = BinaryConvert          .ToUInt16BE(bytes, startIndex +  4);
      precedentAudioSampleCount = BinaryConvertExtensions.ToUInt48BE(bytes, startIndex +  6);
      audioSampleCount          = BinaryConvert          .ToUInt16BE(bytes, startIndex + 12);
      EncodingQuality           = bytes[startIndex + 14];
      Reserved                  = bytes[startIndex + 15];
    }

    public void GetBytes(byte[] bytes, int startIndex)
    {
      if (bytes == null)
        throw new ArgumentNullException("bytes");
      if (startIndex < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("startIndex", startIndex);
      if (bytes.Length - Size < startIndex)
        throw ExceptionUtils.CreateArgumentAttemptToAccessBeyondEndOfArray("startIndex", bytes, startIndex, Size);

      BinaryConvert          .GetBytesBE(frameOffset,               bytes, startIndex + 0);
      BinaryConvert          .GetBytesBE(frameSize,                 bytes, startIndex + 4);
      BinaryConvertExtensions.GetBytesBE(precedentAudioSampleCount, bytes, startIndex + 6);
      BinaryConvert          .GetBytesBE(audioSampleCount,          bytes, startIndex + 12);
      bytes[startIndex + 14] = EncodingQuality;
      bytes[startIndex + 15] = Reserved;
    }

    public byte[] GetBytes()
    {
      var bytes = new byte[Size];

      GetBytes(bytes, 0);

      return bytes;
    }

    public static bool operator == (IndexFileEntry x, IndexFileEntry y)
    {
      return x.field0 == y.field0 && x.field1 == y.field1;
    }

    public static bool operator != (IndexFileEntry x, IndexFileEntry y)
    {
      return x.field0 != y.field0 || x.field1 != y.field1;
    }

    public override bool Equals(object other)
    {
      if (other is IndexFileEntry)
        return Equals((IndexFileEntry)other);
      else
        return false;
    }

    public bool Equals(IndexFileEntry other)
    {
      return this == other;
    }

    public override int GetHashCode()
    {
      return field0.GetHashCode() ^ field1.GetHashCode();
    }

    public override string ToString()
    {
      return string.Format("{{FrameOffset=0x{0:x12}({1}x4096), FrameSize=0x{2:x8}({3}x4096), PrecedentAudioSampleCount={4}, AudioSampleCount={5}, EncodingQuality={6}}}",
                           FrameOffset,
                           frameOffset,
                           FrameSize,
                           frameSize,
                           precedentAudioSampleCount,
                           audioSampleCount,
                           EncodingQuality);
    }
  }
}
