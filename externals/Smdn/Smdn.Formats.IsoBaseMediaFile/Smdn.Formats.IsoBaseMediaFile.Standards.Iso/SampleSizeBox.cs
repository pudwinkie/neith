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
  [BoxType("stsz")]
  [BoxContainer("stbl")]
  [BoxDescription("Sample Size Box (ISO/IEC 14496-12:2005(E) 8.17.2)")]
  public class SampleSizeBox : FullBox {
    [FieldLayout(0, 32)] public uint SampleSize;
    [FieldLayout(1, 32)] public uint SampleCount;

    [FieldLayout(2, 32),
     FieldHandler("ReadEntrySizes", "WriteEntrySizes")]
    public uint[] EntrySizes = new uint[] {};

#pragma warning disable 169 // = 'member is never used'
    private static void ReadEntrySizes(BoxFieldReader reader, FieldHandlingContext ctx)
    {
      var context = ctx as CollectionFieldHandlingContext;

      if (context.IsElementOfCollection) {
        context.ElementIndex = context.ElementCount.Value;
        return;
      }
      else {
        var stsz = context.Instance as SampleSizeBox;

        if (stsz.SampleSize == 0) {
          var length = (long)stsz.SampleCount;
          var sizes = new uint[length];
          var data = reader.ReadBytes(4 * length);
          var index = 0L;
          var offset = 0;

          for (; index < length; index++, offset += 4) {
            sizes[index] = BinaryConvert.ToUInt32(data, offset, reader.Endianness);
          }

          stsz.EntrySizes = sizes;
        }
        else {
          stsz.EntrySizes = new uint[] {};
        }

        context.ElementCount = (long)stsz.SampleCount;
      }
    }

    private static void WriteEntrySizes(BoxFieldWriter writer, FieldHandlingContext ctx)
    {
      var context = ctx as CollectionFieldHandlingContext;

      if (context.IsElementOfCollection) {
        context.ElementIndex = context.ElementCount.Value;
        return;
      }
      else {
        var stsz = context.Instance as SampleSizeBox;

        if (stsz.SampleSize == 0) {
          var length = (int)stsz.SampleCount;
          var sizes = stsz.EntrySizes;
          var data = new byte[4 * length];
          var index = 0L;
          var offset = 0;

          for (; index < length; index++, offset += 4) {
            BinaryConvert.GetBytes(sizes[index], writer.Endianness, data, offset);
          }

          writer.Write(data);
        }

        context.ElementCount = 0L;
      }
    }
#pragma warning restore 169
  }
}