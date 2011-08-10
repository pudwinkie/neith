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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Smdn.Formats.Ini {
  public class IniDocument : IEnumerable<IniSection> {
    public static IEqualityComparer<string> DefaultComparer = StringComparer.OrdinalIgnoreCase;

    public IniSection this[string name] {
      get
      {
        if (name == null)
          // null as default section
          name = string.Empty;

        IniSection sec;

        if (sections.TryGetValue(name, out sec))
          return sec;
        else
          return AppendSection(name);
      }
    }

    public IniSection DefaultSection {
      get { return this[string.Empty]; }
    }

    public IEnumerable<IniSection> Sections {
      get { return sections.Values; }
    }

    public IEqualityComparer<string> Comparer {
      get { return sections.Comparer; }
    }

    public IniDocument()
      : this(DefaultComparer)
    {
    }

    public IniDocument(IEqualityComparer<string> comparer)
    {
      sections = new Dictionary<string, IniSection>(comparer);

      // append default section
      AppendSection(string.Empty);
    }

    public static IniDocument Load(string path)
    {
      return Load(path, DefaultComparer);
    }

    public static IniDocument Load(string path, IEqualityComparer<string> comparer)
    {
      return Load(path, Encoding.Default, comparer);
    }

    public static IniDocument Load(string path, Encoding encoding)
    {
      return Load(path, encoding, DefaultComparer);
    }

    public static IniDocument Load(string path, Encoding encoding, IEqualityComparer<string> comparer)
    {
      using (var stream = File.OpenRead(path)) {
        return Load(stream, encoding, comparer);
      }
    }

    public static IniDocument Load(Stream stream)
    {
      return Load(stream, DefaultComparer);
    }

    public static IniDocument Load(Stream stream, IEqualityComparer<string> comparer)
    {
      return Load(stream, Encoding.Default, comparer);
    }

    public static IniDocument Load(Stream stream, Encoding encoding)
    {
      return Load(stream, encoding, DefaultComparer);
    }

    public static IniDocument Load(Stream stream, Encoding encoding, IEqualityComparer<string> comparer)
    {
      return Load(new StreamReader(stream, encoding), comparer);
    }

    public static IniDocument Load(TextReader reader)
    {
      return Load(reader, DefaultComparer);
    }

    public static IniDocument Load(TextReader reader, IEqualityComparer<string> comparer)
    {
      return Parser.Parse(reader, comparer);
    }

    public void Save(string path)
    {
      Save(path, Encoding.Default);
    }

    public void Save(string path, Encoding encoding)
    {
      using (var stream = File.OpenWrite(path)) {
        stream.SetLength(0L);

        Save(stream, encoding);
      }
    }

    public void Save(Stream stream)
    {
      Save(stream, Encoding.Default);
    }

    public void Save(Stream stream, Encoding encoding)
    {
      Save(new StreamWriter(stream, encoding));
    }

    public void Save(TextWriter writer)
    {
      Formatter.Format(this, writer);

      writer.Flush();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<IniSection> GetEnumerator()
    {
      return sections.Values.GetEnumerator();
    }

    private IniSection AppendSection(string name)
    {
      var section = new IniSection(name, Comparer);

      sections[section.Name] = section;

      return section;
    }

    public IniSection Find(string section)
    {
      if (string.IsNullOrEmpty(section))
        return DefaultSection;

      IniSection sec;

      if (sections.TryGetValue(section, out sec))
        return sec;
      else
        return null;
    }

    public IniSection Find(Predicate<IniSection> match)
    {
      if (match == null)
        throw new ArgumentNullException("match");

      foreach (var section in sections.Values) {
        if (match(section))
          return section;
      }

      return null;
    }

    public bool Exists(string section)
    {
      if (string.IsNullOrEmpty(section))
        return true;
      else
        return sections.ContainsKey(section);
    }

    public bool Exists(Predicate<IniSection> match)
    {
      return Find(match) != null;
    }

    public void Remove(string section)
    {
      if(string.IsNullOrEmpty(section))
        return;

      sections.Remove(section);
    }

    public override string ToString()
    {
      return string.Format("{{Sections={0}}}", sections.Count);
    }

    private Dictionary<string, IniSection> sections;
  }
}
