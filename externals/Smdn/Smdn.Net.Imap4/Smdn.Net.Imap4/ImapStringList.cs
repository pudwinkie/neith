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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Smdn.Net.Imap4 {
  // string types:
  //   ImapString
  //     => handles 'string'
  //     ImapQuotedString
  //       => handles 'quoted'
  //       ImapMailboxNameString (internal)
  //         => handles 'mailbox name'
  //     IImapLiteralString
  //       => handles 'literal'
  //     ImapNilString
  //       => handles 'NIL'
  //     ImapStringList
  //       => list of ImapString
  //       ImapParenthesizedString
  //         => handles 'parenthesized list'
  //     ImapStringEnum
  //       => string enumeration type
  //     ImapCombinableDataItem
  //       => combinable data item type

  public class ImapStringList : ImapString, ICollection, IEnumerable<ImapString> {
    public ImapString this[int index] {
      get { return values[index]; }
      set { values[index] = value; }
    }

    public int Count {
      get { return values.Length; }
    }

    bool ICollection.IsSynchronized {
      get { return values.IsSynchronized; }
    }

    object ICollection.SyncRoot {
      get { return values.SyncRoot; }
    }

    public ImapStringList(string val)
      : this(new[] {new ImapString(val)}, false)
    {
    }

    public ImapStringList(params string[] values)
      : this(Array.ConvertAll(values, delegate(string s) { return new ImapString(s); }), false)
    {
    }

    public ImapStringList(params ImapString[] values)
      : this(values, false)
    {
    }

    protected ImapStringList(string[] values, bool parenthesize)
      : this(Array.ConvertAll(values, delegate(string s) { return new ImapString(s); }), parenthesize)
    {
    }

    protected ImapStringList(ImapString[] values, bool parenthesize)
      : base()
    {
      this.values = values;
      this.parenthesize = parenthesize;
    }

    public void Traverse(Action<ImapString> action) {
      foreach (var val in values) {
        if (val is ImapStringList)
          (val as ImapStringList).Traverse(action);
        else
          action(val);
      }
    }

    public IEnumerator<ImapString> GetEnumerator()
    {
      return (values as IEnumerable<ImapString>).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return values.GetEnumerator();
    }

    void ICollection.CopyTo(Array array, int index)
    {
      values.CopyTo(array, index);
    }

    public override string ToString()
    {
      var joined = string.Join(" ", // SP
                               Array.ConvertAll(values, delegate(ImapString s) { return s.ToString(); }));

      if (parenthesize)
        return string.Format("({0})", joined);
      else
        return joined;
    }

    private /*readonly*/ ImapString[] values;
    private readonly bool parenthesize;
  }
}
