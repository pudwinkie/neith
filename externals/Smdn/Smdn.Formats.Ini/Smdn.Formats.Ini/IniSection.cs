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

namespace Smdn.Formats.Ini {
  public class IniSection : IEnumerable<KeyValuePair<string, string>> {
    public string this[string entry] {
      get
      {
        string val;

        if (entries.TryGetValue(entry, out val))
          return val;
        else
          return null;
      }
      set
      {
        entries[entry] = value;
      }
    }

    public string Name {
      get; private set;
    }

    public bool IsDefaultSection {
      get { return string.Empty.Equals(Name); }
    }

    public Dictionary<string, string> Entries {
      get { return entries; }
    }

    internal IniSection(string name, IEqualityComparer<string> comparer)
    {
      if (name == null)
        throw new ArgumentNullException("name");

      Name = name;
      entries = new Dictionary<string, string>(comparer);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
      return entries.GetEnumerator();
    }

    public string Get(string entry)
    {
      return Get(entry, null);
    }

    public string Get(string entry, string defaultValue)
    {
      if (entry == null)
        throw new ArgumentNullException("entry");

      string val;

      if (entries.TryGetValue(entry, out val))
        return val;
      else
        return defaultValue;
    }

    public TValue Get<TValue>(string entry, Converter<string, TValue> convert)
    {
      return Get(entry, default(TValue), convert);
    }

    public TValue Get<TValue>(string entry, TValue defaultValue, Converter<string, TValue> convert)
    {
      if (entry == null)
        throw new ArgumentNullException("entry");
      if (convert == null)
        throw new ArgumentNullException("convert");

      try {
        string val;

        if (entries.TryGetValue(entry, out val))
          return convert(val);
        else
          return defaultValue;
      }
      catch {
        // ignore any exceptions
        return defaultValue;
      }
    }

    public TValue GetThrowException<TValue>(string entry, Converter<string, TValue> convert)
    {
      return GetThrowException(entry,  default(TValue), convert);
    }

    public TValue GetThrowException<TValue>(string entry, TValue defaultValue, Converter<string, TValue> convert)
    {
      if (entry == null)
        throw new ArgumentNullException("entry");
      if (convert == null)
        throw new ArgumentNullException("convert");

      string val;

      if (entries.TryGetValue(entry, out val))
        return convert(val);
      else
        return defaultValue;
    }

    public override string ToString()
    {
      return string.Format("{{Name={0}, Entries={1}}}", Name, entries.Count);
    }

    private Dictionary<string, string> entries;
  }
}