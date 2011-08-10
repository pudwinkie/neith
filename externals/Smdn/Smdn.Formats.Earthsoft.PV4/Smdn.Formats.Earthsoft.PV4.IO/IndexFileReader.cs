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
using System.IO;

namespace Smdn.Formats.Earthsoft.PV4.IO {
  public class IndexFileReader : Smdn.IO.BigEndianBinaryReader {
    public IndexFileReader(string file)
      : this(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
    {
    }

    public IndexFileReader(Stream stream)
      : base(stream, false, IndexFileEntry.Size)
    {
    }

    public IndexFileEntry ReadEntry()
    {
      IndexFileEntry ret;

      ReadEntry(out ret);

      return ret;
    }

    public bool ReadEntry(out IndexFileEntry entry)
    {
      if (ReadBytesUnchecked(Storage, 0, IndexFileEntry.Size, false) < IndexFileEntry.Size) {
        // reading is attempted past the end of a stream
        entry = IndexFileEntry.Empty;
        return false;
      }
      else {
        entry = new IndexFileEntry(Storage, 0);
        return true;
      }
    }

    public IEnumerable<IndexFileEntry> ReadAllEntry()
    {
      List<IndexFileEntry> entries;

      if (BaseStream.CanSeek) {
        BaseStream.Seek(0L, SeekOrigin.Begin);

        entries = new List<IndexFileEntry>((int)(BaseStream.Length / IndexFileEntry.Size));
      }
      else {
        entries = new List<IndexFileEntry>();
      }
   
      IndexFileEntry entry;

      for (;;) {
        if (!ReadEntry(out entry))
          break;

        entries.Add(entry);
      }

      return entries;
    }
  }
}
