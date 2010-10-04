// 
// Author:
//       smdn <smdn@mail.invisiblefulmoon.net>
// 
// Copyright (c) 2008-2010 smdn
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
using System.Collections.ObjectModel;

namespace Smdn.Formats.Mime {
  public class MimeHeaderCollection : KeyedCollection<string, MimeHeader> {
    public MimeHeaderCollection()
      : this(new MimeHeader[] {})
    {
    }

    public MimeHeaderCollection(IEnumerable<MimeHeader> headers)
      : base(StringComparer.InvariantCultureIgnoreCase)
    {
      AddRange(headers);
    }

    public new void Add(MimeHeader item)
    {
      if (item == null)
        throw new ArgumentNullException("item");

      base.Add(UpdateIndex(item));
    }

    public void AddRange(IEnumerable<MimeHeader> headers)
    {
      if (headers == null)
        throw new ArgumentNullException("headers");

      foreach (var header in headers) {
        Add(header);
      }
    }

    public new void Insert(int index, MimeHeader item)
    {
      if (item == null)
        throw new ArgumentNullException("item");

      base.Insert(index, UpdateIndex(item));
    }

    public IEnumerable<MimeHeader> GetHeaders(string name)
    {
      var matched = new List<MimeHeader>();

      foreach (var header in Dictionary.Values) {
        if (header.IsNameEquals(name))
          matched.Add(header);
      }

      return matched;
    }

    protected override string GetKeyForItem(MimeHeader item)
    {
      if (item.Index == 0)
        return item.Name;
      else
        // use ':' as a name-index delimiter
        return string.Format("{0}{1}{2}", item.Name, MimeHeader.NameBodyDelimiter, item.Index);
    }

    private MimeHeader UpdateIndex(MimeHeader item)
    {
      while (Contains(GetKeyForItem(item))) {
        item.Index++;
      }

      return item;
    }
  }
}
