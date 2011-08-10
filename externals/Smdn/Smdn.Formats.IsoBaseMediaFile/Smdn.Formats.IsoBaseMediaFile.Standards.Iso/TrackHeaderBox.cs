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
  [BoxType("tkhd")]
  [BoxContainer("trak")]
  [BoxDescription("Track Header Box (ISO/IEC 14496-12:2005(E) 8.5)")]
  public class TrackHeaderBox : FullBox {
    [FieldVersionSpecificLayout(0, 64, Version = 1),
     FieldVersionSpecificLayout(0, 32, Version = 0),
     FieldDataType(FieldDataType.IsoDateTime)]
    public DateTime CreationTime;

    [FieldVersionSpecificLayout(1, 64, Version = 1),
     FieldVersionSpecificLayout(1, 32, Version = 0),
     FieldDataType(FieldDataType.IsoDateTime)]
    public DateTime ModificationTime;

    [FieldLayout(2, 32)] public uint TrackId;
    [FieldLayout(3, 32)] public uint reserved1 = 0;

    [FieldVersionSpecificLayout(4, 64, Version = 1),
     FieldVersionSpecificLayout(4, 32, Version = 0)]
    public ulong Duration;

    [FieldLayout(5, 32, Count = 2)] public uint[] reserved2 = new uint[] {0, 0};
    [FieldLayout(6, 16)] public short Layer = 0;
    [FieldLayout(7, 16)] public short AlternateGroup = 0;
    [FieldLayout(8, 16), FieldDataType(FieldDataType.FixedPointSigned88)] public decimal Volume = 1.0m; // 0x0100
    [FieldLayout(9, 16)] public ushort reserved3 = 0;
    [FieldLayout(10, 288)] public Matrix Matrix = Matrix.Unity;
    [FieldLayout(11, 32), FieldDataType(FieldDataType.FixedPointUnsigned1616)] public decimal Width = 0m;
    [FieldLayout(12, 32), FieldDataType(FieldDataType.FixedPointUnsigned1616)] public decimal Height = 0m;
  }
}