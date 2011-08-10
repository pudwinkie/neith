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
using System.Collections.Generic;

using Smdn.Formats.IsoBaseMediaFile.IO;

namespace Smdn.Formats.IsoBaseMediaFile.Standards.Iso {
  [BoxType("stsd")]
  [BoxContainer("stbl")]
  [BoxDescription("Sample Description Box (ISO/IEC 14496-12:2005(E) 8.16)")]
  public class SampleDescriptionBox : FullBox, IBoxContainer {
    [FieldLayout(0, 32)] public uint EntryCount;

    [FieldLayout(1, 0, Count = "EntryCount"),
     FieldHandler("ReadEntries", "WriteEntries")]
    public BoxList<SampleEntryBase> Entries;

    public abstract class SampleEntryBase : Box /*, IBoxContainer*/ {
      /*
      [FieldLayout(0, 0), FieldIgnoreOnReadWrite] public readonly List<Box> Extensions = new List<Box>();

      IList<Box> IBoxContainer.Boxes {
        get { return this.Extensions; }
      }
      */
    }

    IEnumerable<Box> IBoxContainer.Boxes {
      get { return this.Entries; }
    }

    public SampleDescriptionBox()
    {
      this.Entries = new BoxList<SampleEntryBase>(this);
    }

#pragma warning disable 169 // = 'member is never used'
    private static void ReadEntries(BoxFieldReader reader, FieldHandlingContext ctx)
    {
      var context = ctx as CollectionFieldHandlingContext;

      if (context.IsElementOfCollection)
        return;

      var stsd = context.Instance as SampleDescriptionBox;
      var mdia = stsd.ContainedIn; // stsd -> stbl

      for (var i = 0; i < 2; i++) { // -> minf -> mdia
        if (mdia is Box)
          mdia = (mdia as Box).ContainedIn;
        else // null or not Box
          break;
      }

      if (mdia == null)
        throw new InvalidDataException("box 'mdia' not found");

      var hdlr = Box.Find(mdia, delegate(Box box) {
        return (box is HandlerBox);
      }) as HandlerBox;

      if (hdlr == null)
        throw new InvalidDataException("box 'hdlr' not found");

      Type entryType = null;

      switch (hdlr.HandlerType.ToString()) {
        case "soun": entryType = typeof(AudioSampleEntry); break;
        case "vide": entryType = typeof(VisualSampleEntry); break;
        case "hint": entryType = typeof(HintSampleEntry); break;
        case "text": entryType = typeof(TimedText.TextSampleEntry); break;
        default: entryType = typeof(SampleEntry); break;
      }

      var length = context.ElementCount.Value;
      var entries = stsd.Entries;

      entries.Clear();

      for (var index = 0; index < length; index++) {
        entries.Insert(index, reader.ReadBox(stsd, entryType) as SampleEntryBase);
      }
    }

    private static void WriteEntries(BoxFieldWriter writer, FieldHandlingContext ctx)
    {
      var context = ctx as CollectionFieldHandlingContext;

      if (context.IsElementOfCollection)
        return;

      var length = context.ElementCount.Value;
      var entries = (context.Instance as SampleDescriptionBox).Entries;

      for (var index = 0; index < length; index++) {
        writer.Write(entries[index]);
      }
    }

    /*
    private static void ReadExtensions(BoxFieldReader reader, FieldHandlingContext context)
    {
      if (context.IsElementOfArray)
        return;

      var entry = context.Instance as SampleEntryBase;

      if (entry is HintSampleEntry)
        return; // HintSampleEntry has no extensions

      var extensions = (entry as IBoxContainer).Boxes;

      extensions.Clear();

      if (entry.UnreadFieldData != null && 8 <= entry.UnreadFieldData.Length) {
        entry.UnreadFieldData.Seek(0, System.IO.SeekOrigin.Begin);

        var boxReader = new BoxReader(entry.UnreadFieldData);

        for (;;) {
          var box = boxReader.ReadBox(entry, true, null);

          if (box == null)
            break;

          extensions.Add(box);
        }

        entry.UnreadFieldData = null;
      }
    }

    private static void WriteExtensions(BoxFieldWriter writer, FieldHandlingContext context)
    {
      if (context.IsElementOfArray)
        return;

      var entry = context.Instance as SampleEntryBase;

      if (entry is HintSampleEntry)
        return; // HintSampleEntry has no extensions

      foreach (var extension in (entry as IBoxContainer).Boxes) {
        writer.Write(extension);
      }
    }
    */
#pragma warning restore 169
  }
}