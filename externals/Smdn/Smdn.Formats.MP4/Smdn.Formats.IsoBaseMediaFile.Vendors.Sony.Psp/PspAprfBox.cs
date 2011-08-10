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

namespace Smdn.Formats.IsoBaseMediaFile.Vendors.Sony.Psp {
  [BoxType("APRF")]
  [BoxContainer("uuid", UserType = "464f5250-d221-ce4f-bb88-695cfac9c740")]
  [BoxDescription("PSP APRF Box")]
  public class PspAprfBox : Box {
    [FieldLayout(0, 32)] public uint unknown1;
    [FieldLayout(1, 32)] public uint unknown2;
    [FieldLayout(2, 32)] public FourCC Codec;
    [FieldLayout(3, 32)] public uint unknown3;
    [FieldLayout(4, 32)] public uint unknown4;
    [FieldLayout(5, 32)] public uint MaxBitrate;
    [FieldLayout(6, 32)] public uint AvgBitrate;
    [FieldLayout(7, 32)] public uint FrameRate;
    [FieldLayout(8, 32)] public uint Channels;
  }
}