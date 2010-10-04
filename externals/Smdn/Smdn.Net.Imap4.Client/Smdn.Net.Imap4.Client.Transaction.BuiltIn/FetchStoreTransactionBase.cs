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

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Transaction.BuiltIn {
  internal abstract class FetchStoreTransactionBase<TMessageAttribute> :
    ImapTransactionBase<ImapCommandResult<TMessageAttribute[]>>
    where TMessageAttribute : ImapMessageAttributeBase
  {
    public FetchStoreTransactionBase(ImapConnection connection)
      : base(connection)
    {
    }

    protected override void OnDataResponseReceived(ImapDataResponse data)
    {
      if (data.Type == ImapDataResponseType.Fetch)
        ImapDataResponseConverter.FromFetch(messageAttributes, data);

      base.OnDataResponseReceived(data);
    }

    protected ImapCommandResult<TMessageAttribute[]> CreateResult(ImapTaggedStatusResponse tagged)
    {
      return new ImapCommandResult<TMessageAttribute[]>(messageAttributes.ToArray(),
                                                        tagged.ResponseText);
    }

    private MessageAttributeCollection messageAttributes = new MessageAttributeCollection();

    private class MessageAttributeCollection :
      Dictionary<long, TMessageAttribute>,
      IImapMessageAttributeCollection<TMessageAttribute>
    {
      public TMessageAttribute Find(long sequence)
      {
        TMessageAttribute messageAttribute;

        if (TryGetValue(sequence, out messageAttribute))
          return messageAttribute;
        else
          return null;
      }

      public void Add(TMessageAttribute @value)
      {
        Add(@value.Sequence, @value);
      }

      public TMessageAttribute[] ToArray()
      {
        var ret = Values.ToArray();

        Clear();

        return ret;
      }
    }
  }
}
