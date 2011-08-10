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

namespace Smdn.Media.Earthsoft.PV4.Codec.Simple {
  internal sealed class MacroBlockArrangement {
    public int VideoBlockIndex {
      get; private set;
    }

    public bool Interlaced {
      get; private set;
    }

    public int MacroBlockCount {
      get; private set;
    }

    public int MacroBlockPerLineCount {
      get; private set;
    }

    public int MacroBlockPaddingX {
      get; private set;
    }

    public int MacroBlockPaddingY {
      get; private set;
    }

    public int MacroBlock32x8LineY {
      get; private set;
    }

    private MacroBlockArrangement()
    {
    }

    internal static MacroBlockArrangement[] Create(DV dv)
    {
      var videoBlockCount = (dv.FrameScanning == FrameScanning.Progressive) ? 2 : 4;
      var arrangements = new MacroBlockArrangement[videoBlockCount];

      var macroBlockTotalCount   = (dv.PixelsHorizontal * dv.PixelsVertical) / (16 * 16);
      var macroBlockPerLineCount =  dv.PixelsHorizontal / 16;

      for (var index = 0; index < videoBlockCount; index++) {
        arrangements[index] = new MacroBlockArrangement();
        arrangements[index].Interlaced             = (dv.FrameScanning == FrameScanning.Interlaced);
        arrangements[index].VideoBlockIndex        = index;
        arrangements[index].MacroBlockCount        = macroBlockTotalCount / videoBlockCount;
        arrangements[index].MacroBlockPerLineCount = macroBlockPerLineCount;
        arrangements[index].MacroBlock32x8LineY    = -1;
      }

      switch (macroBlockTotalCount % videoBlockCount) {
        case 1:
          arrangements[1].MacroBlockCount++;
          break;

        case 2:
          arrangements[1].MacroBlockCount++;
          arrangements[3].MacroBlockCount++;
          break;

        case 3:
          arrangements[1].MacroBlockCount++;
          arrangements[2].MacroBlockCount++;
          arrangements[3].MacroBlockCount++;
          break;
      }

      if (dv.PixelsVertical % 16 != 0)
        // 32x8 dct block
        arrangements[videoBlockCount - 1].MacroBlock32x8LineY = dv.PixelsVertical / 16;

      // arrangement locations
      var paddingStartY = (dv.PixelsVertical / (16 * videoBlockCount)) * videoBlockCount;
      var paddingCount = 0;

      for (var i = 0; i < videoBlockCount; i++) {
        if (i == 0) {
          arrangements[i].MacroBlockPaddingX = 0;
          arrangements[i].MacroBlockPaddingY = paddingStartY;
        }
        else {
          arrangements[i].MacroBlockPaddingX = paddingCount % macroBlockPerLineCount;
          arrangements[i].MacroBlockPaddingY = paddingCount / macroBlockPerLineCount + paddingStartY;
        }

        paddingCount += arrangements[i].MacroBlockCount -
                        (paddingStartY / videoBlockCount) * macroBlockPerLineCount;
      }

      return arrangements;
    }
  }
}
