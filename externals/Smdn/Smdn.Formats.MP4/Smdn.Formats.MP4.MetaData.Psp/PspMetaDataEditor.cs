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

using Smdn.Formats.IsoBaseMediaFile;
using Smdn.Formats.IsoBaseMediaFile.Standards.Iso;
using Smdn.Formats.IsoBaseMediaFile.Vendors.Sony.Psp;

namespace Smdn.Formats.MP4.MetaData.Psp {
  public class PspMetaDataEditor : MetaDataEditor {
    public override bool HasMetaData {
      get { return FindContainer() != null; }
    }

    public string Title {
      get { return GetString(1u); }
      set { throw new NotImplementedException(); }
    }

    public string TimeStamp {
      get { return GetString(3u); }
      set { throw new NotImplementedException(); }
    }

    public string Creator {
      get { return GetString(4u); }
      set { throw new NotImplementedException(); }
    }

    public PspMetaDataEditor(string mediaFile)
      : base(mediaFile)
    {
    }

    public PspMetaDataEditor(string mediaFile, bool openAsWritable)
      : base(mediaFile, openAsWritable)
    {
    }

    public PspMetaDataEditor(MediaFile mediaFile)
      : base(mediaFile)
    {
    }

    public PspMetaDataEditor(MetaDataEditor editor)
      : base(editor)
    {
    }

    private PspMtdtBox FindContainer()
    {
      var moov = MediaFile.Find("moov") as MovieBox;

      if (moov == null)
        return null;

      var usmt = Box.Find<PspUsmtExtensionBox>(moov);

      if (usmt == null)
        return null;

      return Box.Find<PspMtdtBox>(usmt);
    }
    
    public override void RemoveMetaDataBoxes()
    {
      throw new NotImplementedException();
    }

    public override void UpdateBoxes()
    {
      throw new NotImplementedException();
    }

    private string GetString(uint type)
    {
      var mtdt = FindContainer();

      if (mtdt == null)
        return null;

      var entry = Array.Find(mtdt.Entries, (e) => e.Type == type) as PspMtdtBox.StringEntry;

      if (entry == null)
        return null;
      else
        return entry.Data;
    }
  }
}