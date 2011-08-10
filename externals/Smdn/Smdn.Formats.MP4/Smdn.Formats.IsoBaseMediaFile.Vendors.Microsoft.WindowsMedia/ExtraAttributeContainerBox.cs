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
using System.Text;

using Smdn.Formats.IsoBaseMediaFile.IO;

namespace Smdn.Formats.IsoBaseMediaFile.Vendors.Microsoft.WindowsMedia {
  [BoxType("Xtra")]
  [BoxContainer("meta")]
  [BoxDescription("Microsoft Windows Media ASF Attribute Container Box")]
  public class ExtraAttributeContainerBox : Box {
    [FieldLayout(0, 0)] public readonly List<Attribute> Attributes;

    [FieldStructure]
    public class Attribute {
      [FieldLayout(0, 32)] public uint AttributeLength;
      [FieldLayout(1, 16)] public ushort unknown1; // StreamNum or LanguageIndex?
      [FieldLayout(2, 16)] public ushort NameLength;
      [FieldLayout(3, 0), FieldHandler("ReadName", "WriteName")] public string Name;
      [FieldLayout(4, 16)] public ushort unknown2; // or wchar null termination?
      [FieldLayout(5, 16)] public ushort unknown3; // StreamNum or LanguageIndex?
      [FieldLayout(6, 32)] public uint DataLength;
      [FieldLayout(7, 16)] public ushort DataType;
      [FieldLayout(8, 0), FieldHandler("ReadData", "WriteData")] public DataBlock Data;

#pragma warning disable 169 // = 'member is never used'
      private static void ReadName(BoxFieldReader reader, FieldHandlingContext context)
      {
        var attr = context.Instance as Attribute;

        attr.Name = Encoding.ASCII.GetString(reader.ReadBytes(attr.NameLength));
      }

      private static void WriteName(BoxFieldWriter writer, FieldHandlingContext context)
      {
        writer.Write(Encoding.ASCII.GetBytes((context.Instance as Attribute).Name));
      }

      private static void ReadData(BoxFieldReader reader, FieldHandlingContext context)
      {
        var attr = context.Instance as Attribute;

        attr.Data = reader.ReadDataBlock(attr.DataLength - 6); // DataLength(4) + DataType(2)
      }

      private static void WriteData(BoxFieldWriter writer, FieldHandlingContext context)
      {
        writer.Write((context.Instance as Attribute).Data);
      }
#pragma warning restore 169
    }

    public ExtraAttributeContainerBox()
    {
      this.Attributes = new List<Attribute>();
    }
  }
}