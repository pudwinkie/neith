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

namespace Smdn.Net.Imap4 {
  public class ImapMessageFlagSet : ImapStringEnumSet<ImapMessageFlag>, IImapMessageFlagSet {
    public bool ContainNonApplicableFlags {
      get { return Overlaps(ImapMessageFlag.NonApplicableFlags); }
    }

    public ImapMessageFlagSet()
      : base(false)
    {
    }

    public ImapMessageFlagSet(IEnumerable<ImapMessageFlag> flags)
      : base(false, flags)
    {
    }

    public ImapMessageFlagSet(params ImapMessageFlag[] flags)
      : base(false, flags)
    {
    }

    public ImapMessageFlagSet(string[] keywords, params ImapMessageFlag[] flags)
      : base(false, flags)
    {
      if (keywords == null)
        throw new ArgumentNullException("keywords");

      AddRange(Array.ConvertAll(keywords, delegate(string keyword) {
        return new ImapMessageFlag(keyword);
      }));
    }

    internal ImapMessageFlagSet(bool readOnly, IEnumerable<ImapMessageFlag> flags)
      : base(readOnly, flags)
    {
    }

    private ImapMessageFlagSet(bool readOnly)
      : base(readOnly)
    {
    }

    public static ImapMessageFlagSet CreateReadOnlyEmpty()
    {
      return new ImapMessageFlagSet(true);
    }

    public IImapMessageFlagSet GetNonApplicableFlagsRemoved()
    {
      var list = new ImapMessageFlagSet(this);

      list.ExceptWith(ImapMessageFlag.NonApplicableFlags);

      return list;
    }

    public ImapMessageFlagSet AsReadOnly()
    {
      if (IsReadOnly)
        return this;
      else
        return new ImapMessageFlagSet(true, this);
    }
  }
}