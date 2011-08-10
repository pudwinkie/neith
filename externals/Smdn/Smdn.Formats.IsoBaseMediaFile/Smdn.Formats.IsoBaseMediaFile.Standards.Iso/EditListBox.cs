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
  [BoxType("elst")]
  [BoxContainer("edts")]
  [BoxDescription("Edit List Box (ISO/IEC 14496-12:2005(E) 8.26)")]
  public class EditListBox : FullBox {
    [FieldLayout(0, 32)] public uint EntryCount;
    [FieldLayout(1, 0, Count = "EntryCount")] public Entry[] Entries = new Entry[] {};

    [FieldStructure]
    public struct Entry {
      [FieldVersionSpecificLayout(0, 64, Version = 1),
       FieldVersionSpecificLayout(0, 32, Version = 0)]
      public ulong SegmentDuration;

      [FieldVersionSpecificLayout(1, 64, Version = 1),
       FieldVersionSpecificLayout(1, 32, Version = 0)]
      public long MediaTime;

      [FieldLayout(2, 16)] public short MediaRateInteger;
      [FieldLayout(3, 16)] public short MediaRateFraction;
    }
  }
}