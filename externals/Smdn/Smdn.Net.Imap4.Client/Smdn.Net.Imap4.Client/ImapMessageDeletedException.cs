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
  //             Smdn.Net.Imap4.Client.ImapMessageDeletedException

  [Serializable]
  public class ImapMessageDeletedException : ImapProtocolViolationException {
    [NonSerialized]
    private ImapMessageInfo deletedMessage;

    public ImapMessageInfo DeletedMessage {
      get { return deletedMessage; }
    }

    public ImapMessageDeletedException(ImapMessageInfo deletedMessage)
      : this(string.Format("message has been deleted (UID: {0})", deletedMessage.Uid), deletedMessage)
    {
    }

    public ImapMessageDeletedException(string message, ImapMessageInfo deletedMessage)
      : base(message)
    {
      if (deletedMessage == null)
        throw new ArgumentNullException("deletedMessage");

      this.deletedMessage = deletedMessage;
    }

    protected ImapMessageDeletedException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      // TODO
      //DeletedMessage = info.GetValue("DeletedMessage", typeof(ImapMessageInfo));
    }

    /*
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);

      // TODO
      //info.AddValue("DeletedMessage", DeletedMessage);
    }
    */
  }
}
