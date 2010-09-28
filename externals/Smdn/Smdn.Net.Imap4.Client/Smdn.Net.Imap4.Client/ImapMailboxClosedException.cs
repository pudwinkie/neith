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
using System.Runtime.Serialization;

namespace Smdn.Net.Imap4.Client {
  // Exceptions:
  //   System.Excpetion
  //     System.SystemException
  //       Smdn.Net.Imap4.ImapExcetion
  //         Smdn.Net.Imap4.ImapInvalidOperationException
  //           Smdn.Net.Imap4.ImapProtocolViolationException
  //             Smdn.Net.Imap4.Client.ImapMailboxClosedException

  [Serializable]
  public class ImapMailboxClosedException : ImapProtocolViolationException {
    public string Mailbox {
      get; private set;
    }

    public ImapMailboxClosedException(string mailboxName)
      : this(string.Format("mailbox '{0}' has been closed or another mailbox is opened", mailboxName), mailboxName)
    {
    }

    public ImapMailboxClosedException(string message, string mailboxName)
      : base(message)
    {
      Mailbox = mailboxName;
    }

    protected ImapMailboxClosedException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Mailbox = info.GetString("Mailbox");
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);

      info.AddValue("Mailbox", Mailbox);
    }
  }
}
