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

using Smdn.Formats.IsoBaseMediaFile.IO;

namespace Smdn.Formats.IsoBaseMediaFile.Vendors.Apple.iTunes {
  public abstract class MetaDataAtomBase : Box, IBoxContainer {
    [FieldLayout(0, 0), FieldHandler("ReadData", "WriteData")] public readonly BoxList Boxes;

    IEnumerable<Box> IBoxContainer.Boxes {
      get { return Boxes; }
    }

    protected MetaDataAtomBase()
    {
      this.Boxes = new BoxList(this);
    }

#pragma warning disable 169 // = 'member is never used'
    protected static void ReadData(BoxFieldReader reader, FieldHandlingContext ctx)
    {
      var context = ctx as CollectionFieldHandlingContext;

      if (context.IsElementOfCollection) {
        context.ElementCount = 0L;
        return;
      }

      var atom = context.Instance as MetaDataAtomBase;

      for (;;) {
        var box = reader.ReadBox(atom, null);

        if (box == null)
          break;

        if (box is DataAtom) {
          var dataAtom = box as DataAtom;
          var flags = (UInt24)(dataAtom.Flags & 0x0000ff);

          if (flags == TextDataAtom.ClassValue)
            box = new TextDataAtom(dataAtom);
          else if (flags == JpegDataAtom.ClassValue)
            box = new JpegDataAtom(dataAtom);
          else if (flags == PngDataAtom.ClassValue)
            box = new PngDataAtom(dataAtom);
          else if (flags == BpmDataAtom.ClassValue)
            box = new BpmDataAtom(dataAtom);
          else if (flags == IntDataAtom.ClassValue)
            box = new IntDataAtom(dataAtom);
        }

        atom.Boxes.Add(box);
      }
    }

    protected static void WriteData(BoxFieldWriter writer, FieldHandlingContext ctx)
    {
      var context = ctx as CollectionFieldHandlingContext;

      if (context.IsElementOfCollection) {
        context.ElementCount = 0L;
        return;
      }

      var atom = context.Instance as MetaDataAtomBase;

      foreach (var box in atom.Boxes) {
        if (box is DataAtomBase) {
          var dataAtom = box as DataAtomBase;

          if (dataAtom is TextDataAtom)
            dataAtom.Flags = (int)TextDataAtom.ClassValue;
          else if (dataAtom is JpegDataAtom)
            dataAtom.Flags = (int)JpegDataAtom.ClassValue;
          else if (dataAtom is PngDataAtom)
            dataAtom.Flags = (int)PngDataAtom.ClassValue;
          else if (dataAtom is BpmDataAtom)
            dataAtom.Flags = (int)BpmDataAtom.ClassValue;
          else if (dataAtom is IntDataAtom)
            dataAtom.Flags = (int)IntDataAtom.ClassValue;
        }

        writer.Write(box);
      }
    }
#pragma warning restore 169
  }
}