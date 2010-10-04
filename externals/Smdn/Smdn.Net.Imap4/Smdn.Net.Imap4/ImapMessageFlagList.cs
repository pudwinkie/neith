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

namespace Smdn.Net.Imap4 {
  public class ImapMessageFlagList : ImapStringEnumList<ImapMessageFlag>, IImapMessageFlagSet {
    public bool ContainNonApplicableFlags {
      get { return Has(ImapMessageFlag.Recent) || Has(ImapMessageFlag.AllowedCreateKeywords); }
    }

    public ImapMessageFlagList()
      : base(false)
    {
    }

    public ImapMessageFlagList(IEnumerable<ImapMessageFlag> flags)
      : base(false, flags)
    {
    }

    public ImapMessageFlagList(params ImapMessageFlag[] flags)
      : base(false, flags)
    {
    }

    public ImapMessageFlagList(string[] keywords, params ImapMessageFlag[] flags)
      : base(false, flags)
    {
      if (keywords == null)
        throw new ArgumentNullException("keywords");

      AddRange(Array.ConvertAll(keywords, delegate(string keyword) {
        return new ImapMessageFlag(keyword);
      }));
    }

    internal ImapMessageFlagList(bool readOnly, IEnumerable<ImapMessageFlag> flags)
      : base(readOnly, flags)
    {
    }

    public static ImapMessageFlagList CreateReadOnlyEmpty()
    {
      return new ImapMessageFlagList(true, new ImapMessageFlag[0]);
    }

    public IImapMessageFlagSet GetNonApplicableFlagsRemoved()
    {
      if (ContainNonApplicableFlags) {
        var list = new ImapMessageFlagList(this);

        list.Remove(ImapMessageFlag.Recent);
        list.Remove(ImapMessageFlag.AllowedCreateKeywords);

        return list;
      }
      else {
        return this;
      }
    }

    public IImapMessageFlagSet AsReadOnly()
    {
      if (IsReadOnly)
        return this;
      else
        return new ImapMessageFlagList(true, this);
    }
  }
}