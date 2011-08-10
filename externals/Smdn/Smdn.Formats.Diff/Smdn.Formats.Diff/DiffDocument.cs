// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2010-2011 smdn
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
using System.Text;

namespace Smdn.Formats.Diff {
  public class DiffDocument : IEnumerable<DiffEntry>, IDiffEntity {
    public string Difference {
      get; set;
    }

    public DiffFormat Format {
      get; set;
    }

    public IList<DiffEntry> Entries {
      get { return entries; }
    }

    public DiffDocument()
      : this(DiffFormat.Unified) // XXX
    {
    }

    public DiffDocument(DiffFormat format)
    {
      Format = format;

      entries = new List<DiffEntry>();
    }

    public static DiffDocument Load(string file, DiffFormat format)
    {
      return Load(file, Encoding.Default, format);
    }

    public static DiffDocument Load(string file, Encoding encoding, DiffFormat format)
    {
      if (file == null)
        throw new ArgumentNullException("file");

      using (var stream = File.OpenRead(file)) {
        return Load(stream, encoding, format);
      }
    }

    public static DiffDocument Load(Stream stream, DiffFormat format)
    {
      return Load(stream, Encoding.Default, format);
    }

    public static DiffDocument Load(Stream stream, Encoding encoding, DiffFormat format)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");
      if (encoding == null)
        throw new ArgumentNullException("encoding");

      return Load(new StreamReader(stream, encoding), format);
    }

    public static DiffDocument Load(TextReader reader, DiffFormat format)
    {
      return Parser.Parse(reader, format);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<DiffEntry> GetEnumerator()
    {
      return entries.GetEnumerator();
    }

    private List<DiffEntry> entries;
  }
}
