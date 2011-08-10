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
using System.Text;

using Smdn.Formats.IsoBaseMediaFile.IO;

namespace Smdn.Formats.IsoBaseMediaFile.Vendors.Sony.Psp {
  [BoxType("MTDT")]
  [BoxContainer("uuid", UserType = "544d5355-d221-ce4f-bb88-695cfac9c740")]
  [BoxDescription("PSP MTDT Box")]
  public class PspMtdtBox : Box {
    [FieldLayout(0, 16)] public ushort EntryCount;

    [FieldLayout(1, 0, Count = "EntryCount"),
     FieldHandler("ReadEntries", "WriteEntries")]
    public Entry[] Entries = new Entry[] {};

    public abstract class Entry {
      [FieldLayout(0, 16)] public ushort Size;
      [FieldLayout(1, 32)] public uint Type;
      [FieldLayout(2, 1)] public byte pad;
      [FieldLayout(3, 15)] public LanguageCode Language;
      [FieldLayout(4, 16)] public ushort DataType;
    }

    [FieldStructure]
    public class StringEntry : Entry {
      [FieldLayout(0, 0)] public string Data;
    }

    [FieldStructure]
    public class DataEntry : Entry {
      [FieldLayout(0, 8)] public DataBlock Data;
    }

#pragma warning disable 169 // = 'member is never used'
    private static void ReadEntries(BoxFieldReader reader, FieldHandlingContext ctx)
    {
      var context = ctx as CollectionFieldHandlingContext;
      var mtdt = context.Instance as PspMtdtBox;

      if (context.IsElementOfCollection) {
        Entry entry;

        var size = reader.ReadUInt16();
        var type = reader.ReadUInt32();
        var pad = reader.ReadBits(1);
        var lang = new LanguageCode(reader.ReadBits(5), reader.ReadBits(5), reader.ReadBits(5));
        var dataType = reader.ReadUInt16();
        var dataLength = (long)size - 10;

        switch (dataType) {
          case 0x00000001: {// string?
            entry = new StringEntry();
            (entry as StringEntry).Data = Encoding.BigEndianUnicode.GetString(reader.ReadBytes(dataLength)).TrimEnd('\0');
            break;
          }

          default: { // short[]?
            entry = new DataEntry();
            (entry as DataEntry).Data = new DataBlock(reader.ReadBytes(dataLength));
            break;
          }
        }

        entry.Size = size;
        entry.Type = type;
        entry.pad = pad;
        entry.Language = lang;
        entry.DataType = dataType;

        mtdt.Entries[context.ElementIndex] = entry;
      }
      else {
        mtdt.Entries = new Entry[context.ElementCount.Value];
      }
    }

    private static void WriteEntries(BoxFieldWriter writer, FieldHandlingContext ctx)
    {
      var context = ctx as CollectionFieldHandlingContext;

      if (!context.IsElementOfCollection)
        return;

      var entry = (context.Instance as PspMtdtBox).Entries[context.ElementIndex];

      ushort size = 10;

      if (entry is StringEntry)
        size += (ushort)Encoding.BigEndianUnicode.GetByteCount((entry as StringEntry).Data);
      else if (entry is DataEntry)
        size += (ushort)(entry as DataEntry).Data.Length;

      writer.Write(size);
      writer.Write(entry.Type);
      writer.WriteBits(entry.pad, 1);
      writer.Write(entry.Language);
      writer.Write(entry.DataType);

      if (entry is StringEntry)
        writer.Write(Encoding.BigEndianUnicode.GetBytes((entry as StringEntry).Data));
      else if (entry is DataEntry)
        writer.Write((entry as DataEntry).Data);
    }
#pragma warning restore 169
  }
}