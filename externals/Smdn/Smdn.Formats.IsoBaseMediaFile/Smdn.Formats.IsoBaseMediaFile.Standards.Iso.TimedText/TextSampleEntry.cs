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

namespace Smdn.Formats.IsoBaseMediaFile.Standards.Iso.TimedText {
  [BoxType("tx3g")]
  [BoxContainer("stsd")]
  [BoxDescription("Text Sample Sample Entry (ISO/IEC 14496-17)")]
  public class TextSampleEntry : SampleEntry, IBoxContainer {
    [FieldLayout(0, 32)] public DisplayFlag DisplayFlags;
    [FieldLayout(1, 8)] public Justification HorizontalJustification;
    [FieldLayout(2, 8)] public Justification VerticalJustification;
    [FieldLayout(3, 32)] public Rgba BackgroundColor;
    [FieldLayout(4, 0)] public BoxRecord DefaultTextBox;
    [FieldLayout(5, 0)] public StyleRecord DefaultStyle;
    [FieldLayout(6, 0)] public FontTableBox FontTable;

    IEnumerable<Box> IBoxContainer.Boxes {
      get { yield return FontTable; }
    }
  }
}