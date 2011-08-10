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

namespace Smdn.Formats.IsoBaseMediaFile.Vendors.Apple.iTunes {
  [BoxDescription("Apple iTunes PNG Data Atom")]
  public class PngDataAtom : DataAtomBase {
    [FieldLayout(0, 0)] public DataBlock PngImage;

    internal static readonly UInt24 ClassValue = (UInt24)0x00000e;

    public PngDataAtom()
      : base(ClassValue)
    {
    }

    internal PngDataAtom(DataAtom data)
      : base(data.Offset, ClassValue)
    {
      this.PngImage = data.Data;
    }

    internal PngDataAtom(MemoryStream pngStream)
      : this()
    {
      this.PngImage = new DataBlock(pngStream);
    }
  }
}