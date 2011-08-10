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

namespace Smdn.Formats.IsoBaseMediaFile.Standards.Iso {
  public class VisualSampleEntry : SampleEntry {
    [FieldLayout(0, 16)] public ushort pre_defined = 0;
    [FieldLayout(1, 16)] public ushort reserved2 = 0;
    [FieldLayout(2, 32, Count = 3)] public uint[] pre_defined2 = new uint[] {0, 0, 0};
    [FieldLayout(3, 16)] public ushort Width;
    [FieldLayout(4, 16)] public ushort Height;
    [FieldLayout(5, 32), FieldDataType(FieldDataType.FixedPointUnsigned1616)] public decimal HorizResolution = 72.0m; // 0x00480000, 72 dpi
    [FieldLayout(6, 32), FieldDataType(FieldDataType.FixedPointUnsigned1616)] public decimal VertResolution = 72.0m; // 0x00480000, 72 dpi
    [FieldLayout(7, 32)] public uint reserved3 = 0;
    [FieldLayout(8, 16)] public ushort FrameCount = 1;
    [FieldLayout(9, 8 * 32), FieldDataType(FieldDataType.StringLengthStored8)] public string CompressorName;
    [FieldLayout(10, 16)] public ushort Depth = 0x0018;
    [FieldLayout(11, 16)] public short pre_defined3 = -1;
  }
}