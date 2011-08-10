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
using System.Collections.Generic;

namespace Smdn.Formats.IsoBaseMediaFile.Standards.Iso {
  [BoxType("meta")]
  [BoxContainer("moov", "trak")]
  [BoxDescription("The Meta box (ISO/IEC 14496-12:2005(E) 8.44.1)")]
  public class MetaBox : FullBox, IBoxContainer {
    [FieldLayout(0, 0)] public HandlerBox Handler;
    [FieldLayout(1, 0)] public readonly BoxList Boxes;

    IEnumerable<Box> IBoxContainer.Boxes {
      get
      {
        yield return Handler;
        foreach (var box in Boxes) {
          yield return box;
        }
      }
    }

    public MetaBox()
    {
      this.Boxes = new BoxList(this);
    }
  }
}