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
using System.Collections.ObjectModel;

namespace Smdn.Net.Imap4.Client {
  internal sealed class ImapMessageInfoCollection :
    KeyedCollection<long, ImapMessageInfo>,
    IImapMessageAttributeCollection<ImapMessageDynamicAttribute>
  {
    protected override long GetKeyForItem(ImapMessageInfo item)
    {
      return item.Uid;
    }

    public ImapMessageInfo Find(long sequence)
    {
      foreach (var item in Items) {
        if (item.Sequence == sequence)
          return item;
      }

      return null;
    }

    ImapMessageDynamicAttribute IImapMessageAttributeCollection<ImapMessageDynamicAttribute>.Find(long sequence)
    {
      var item = Find(sequence);

      if (item == null)
        return null;
      else
        return item.GetDynamicAttribute();
    }

    public void Add(ImapMessageDynamicAttribute @value)
    {
      foreach (var item in Items) {
        if (item.Sequence == @value.Sequence)
          item.DynamicAttribute = @value;
      }
    }
  }
}
