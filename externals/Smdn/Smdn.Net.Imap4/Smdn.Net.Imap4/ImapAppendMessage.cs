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
using System.IO;

namespace Smdn.Net.Imap4 {
  public class ImapAppendMessage : IImapAppendMessage {
    public DateTimeOffset? InternalDate {
      get; set;
    }

    public ImapMessageFlagSet Flags {
      get; private set;
    }

    IImapMessageFlagSet IImapAppendMessage.Flags {
      get { return Flags; }
    }

    public ImapAppendMessage(byte[] message)
      : this(new MemoryStream(message, false), null, null)
    {
    }

    public ImapAppendMessage(byte[] message, DateTimeOffset internalDate)
      : this(new MemoryStream(message, false), internalDate, null)
    {
    }

    public ImapAppendMessage(byte[] message, IImapMessageFlagSet flags)
      : this(new MemoryStream(message, false), null, flags)
    {
    }

    public ImapAppendMessage(byte[] message, DateTimeOffset internalDate, IImapMessageFlagSet flags)
      : this(new MemoryStream(message, false), (DateTimeOffset?)internalDate, flags)
    {
    }

    public ImapAppendMessage(Stream messageStream)
      : this(messageStream, null, null)
    {
    }

    public ImapAppendMessage(Stream messageStream, DateTimeOffset internalDate)
      : this(messageStream, internalDate, null)
    {
    }

    public ImapAppendMessage(Stream messageStream, IImapMessageFlagSet flags)
      : this(messageStream, null, flags)
    {
    }

    public ImapAppendMessage(Stream messageStream, DateTimeOffset internalDate, IImapMessageFlagSet flags)
      : this(messageStream, (DateTimeOffset?)internalDate, flags)
    {
    }

    private ImapAppendMessage(Stream messageStream, DateTimeOffset? internalDate, IImapMessageFlagSet flags)
    {
      if (messageStream == null)
        throw new ArgumentNullException("messageStream");

      this.messageStream = messageStream;
      this.InternalDate = internalDate;
      this.Flags = (flags == null) ? new ImapMessageFlagSet() : new ImapMessageFlagSet(flags);
    }

    public Stream GetMessageStream()
    {
      return messageStream;
    }

    private Stream messageStream;
  }
}
