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
  [BoxType("mvhd")]
  [BoxContainer("moov")]
  [BoxDescription("Movie Header Box (ISO/IEC 14496-12:2005(E) 8.3)")]
  public class MovieHeaderBox : FullBox {
    [FieldVersionSpecificLayout(0, 64, Version = 1),
     FieldVersionSpecificLayout(0, 32, Version = 0),
     FieldDataType(FieldDataType.IsoDateTime)]
    public DateTime CreationTime;

    [FieldVersionSpecificLayout(1, 64, Version = 1),
     FieldVersionSpecificLayout(1, 32, Version = 0),
     FieldDataType(FieldDataType.IsoDateTime)]
    public DateTime ModificationTime;

    [FieldLayout(2, 32)] public ulong TimeScale;

    [FieldVersionSpecificLayout(3, 64, Version = 1),
     FieldVersionSpecificLayout(3, 32, Version = 0)]
    public ulong Duration;

    [FieldLayout(4, 32), FieldDataType(FieldDataType.FixedPointSigned1616)] public decimal Rate = 1.0m; // 0x00010000
    [FieldLayout(5, 16), FieldDataType(FieldDataType.FixedPointSigned88)] public decimal Volume = 1.0m; // 0x0100
    [FieldLayout(6, 16)] public short reserved1 = 0;
    [FieldLayout(7, 32, Count = 2)] public uint[] reserved2 = new uint[] {0, 0};
    [FieldLayout(8, 288)] public Matrix Matrix = Matrix.Unity;
    [FieldLayout(9, 32, Count = 6)] public int[] pre_defined = new int[] {0, 0, 0, 0, 0, 0};
    [FieldLayout(10, 32)] public uint NextTrackId;
  }
}