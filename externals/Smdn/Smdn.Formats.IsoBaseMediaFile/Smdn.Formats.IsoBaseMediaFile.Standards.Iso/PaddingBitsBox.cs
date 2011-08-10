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

using Smdn.Formats.IsoBaseMediaFile.IO;

namespace Smdn.Formats.IsoBaseMediaFile.Standards.Iso {
  [BoxType("padb")]
  [BoxContainer("stbl")]
  [BoxDescription("Padding Bits Box (ISO/IEC 14496-12:2005(E) 8.23)")]
  public class PaddingBitsBox : FullBox {
    [FieldLayout(0, 32)] public uint SampleCount;

    [FieldLayout(1, 0),
     FieldHandler("ReadPadding", "WritePadding")]
    public DataBlock Padding;

    protected override void Dispose(bool disposing)
    {
      if (disposing) {
        if (Padding != null) {
          Padding.Dispose();
          Padding = null;
        }
      }

      base.Dispose(disposing);
    }

#pragma warning disable 169 // = 'member is never used'
    private static void ReadPadding(BoxFieldReader reader, FieldHandlingContext context)
    {
      var padb = (context.Instance as PaddingBitsBox);

      padb.Padding = reader.ReadDataBlock((padb.SampleCount + 1) / 2);
    }

    private static void WritePadding(BoxFieldWriter writer, FieldHandlingContext context)
    {
      var padb = (context.Instance as PaddingBitsBox);

      writer.WriteZero((padb.SampleCount + 1) / 2);
    }
#pragma warning restore 169
  }
}