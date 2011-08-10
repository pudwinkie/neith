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
using Smdn.Mathematics;

namespace Smdn.Formats.Earthsoft.PV4 {
  // ファイルフォーマット資料
  // http://earthsoft.jp/PV/tech-file.html

  // 1. ストリームファイル (*.dv)
  //   2. 音声・映像フレームデータ
  public sealed class StreamFileFrameData {
    /*
     * オフセット  バイト数  説明
     * 0  6   先頭からひとつ前の「音声・映像フレーム」までの積算音声フレーム数 (※1)
     * 6  2   音声フレーム数 (※2)
     * 8  4   音声サンプリング周波数
     * 12   244   予約
     * 256  2   映像表示比率・水平方向
     * 258  2   映像表示比率・垂直方向
     * 260  1   エンコード品質 (※3)
     * 261  123   予約
     * 384  4   映像領域0 のデータサイズ (32の倍数)
     * 388  4   映像領域1 のデータサイズ (32の倍数)
     * 392  4   映像領域2 のデータサイズ (32の倍数・インターレース映像のみ)
     * 396  4   映像領域3 のデータサイズ (32の倍数・インターレース映像のみ)
     * 400  112   予約
     * 512  可変  音声データ
     * => 2ch 16ビット
     * 4096×n0  可変  映像領域0 のデータ
     * 32×n1  可変  映像領域1 のデータ
     * 32×n2  可変  映像領域2 のデータ (インターレース映像のみ)
     * 32×n3  可変  映像領域3 のデータ (インターレース映像のみ)
     * 4096×n4  -   次の「音声・映像フレームデータ」の始まり
     */
    public static readonly int FixedDataSize = 512;

    private StreamFileAudioData audio;

    public StreamFileAudioData Audio {
      get { return audio; }
    }

    private StreamFileVideoData video;

    public StreamFileVideoData Video {
      get { return video; }
    }

    internal StreamFileFrameData(StreamFileAudioData audio, StreamFileVideoData video)
    {
      this.audio = audio;
      this.video = video;
    }

    public void SetAudio(long precedentSampleCount, int sampleCount, long samplingFrequency)
    {
      // TODO: check arguments

      audio = new StreamFileAudioData();

      audio.PrecedentSampleCountValue = (UInt48)precedentSampleCount;
      audio.SampleCountValue = (ushort)sampleCount;
      audio.SamplingFrequencyValue = (uint)samplingFrequency;

      var dataLength = (long)(sampleCount << 2); // 2ch * 16bit

      audio.AlignedDataLength = (dataLength <= 3584L) ? 3584L : 3584L + ((dataLength - 512L) % 4096L); // XXX
      audio.Data = new byte[audio.AlignedDataLength];
    }

    public void SetAudio(StreamFileAudioData refAudioData)
    {
      audio = refAudioData.Clone();
    }

    public override string ToString()
    {
      return string.Format("{{Audio={0}, Video={1}}}", audio, video);
    }
  }
}
