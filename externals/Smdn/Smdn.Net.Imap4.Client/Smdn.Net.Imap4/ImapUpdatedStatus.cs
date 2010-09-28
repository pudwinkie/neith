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

using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4 {
  // 7.2. Server Responses - Server and Mailbox Status
  // 7.2.6. FLAGS Response
  // 7.3. Server Responses - Mailbox Size
  // 7.3.1. EXISTS Response
  // 7.3.2. RECENT Response
  // 7.4. Server Responses - Message Status
  // 7.4.1. EXPUNGE Response
  // 7.4.2. FETCH Response
  public sealed class ImapUpdatedStatus {
    public IImapMessageFlagSet ApplicableFlags {
      get { return applicableFlags; }
    }

    [CLSCompliant(false)]
    public IImapMessageDynamicAttribute MessageAttribute {
      get { return messageAttribute; }
    }

    public long? Expunge {
      get { return expunge; }
    }

    public long? Exists {
      get { return exists; }
    }

    public long? Recent {
      get { return recent; }
    }

    public static ImapUpdatedStatus CreateFrom(ImapDataResponse data)
    {
      if (data == null)
        throw new ArgumentNullException("data");

      if (data.Type == ImapDataResponseType.Fetch)
        return new ImapUpdatedStatus(null, ImapDataResponseConverter.FromFetch<ImapMessageDynamicAttribute>(data), null, null, null);
      else if (data.Type == ImapDataResponseType.Expunge)
        return new ImapUpdatedStatus(null, null, ImapDataResponseConverter.FromExpunge(data), null, null);
      else if (data.Type == ImapDataResponseType.Exists)
        return new ImapUpdatedStatus(null, null, null, ImapDataResponseConverter.FromExists(data), null);
      else if (data.Type == ImapDataResponseType.Recent)
        return new ImapUpdatedStatus(null, null, null, null, ImapDataResponseConverter.FromRecent(data));
      else if (data.Type == ImapDataResponseType.Flags)
        return new ImapUpdatedStatus(ImapDataResponseConverter.FromFlags(data), null, null, null, null);
      else
        return null;
    }

    public ImapUpdatedStatus()
      : this(null, null, null, null, null)
    {
    }

    [CLSCompliant(false)]
    public ImapUpdatedStatus(IImapMessageFlagSet applicableFlags,
                             IImapMessageDynamicAttribute messageAttribute,
                             long? expunge,
                             long? exists,
                             long? recent)
    {
      this.applicableFlags = applicableFlags;
      this.messageAttribute = messageAttribute;
      this.expunge = expunge;
      this.exists = exists;
      this.recent = recent;
    }

    public override string ToString()
    {
      return string.Format("{{MessageAttribute={0}, Expunge={1}, Exists={2}, Recent={3}, ApplicableFlags={4}}}",
                           messageAttribute,
                           expunge,
                           exists,
                           recent,
                           applicableFlags);
    }

    private /*readonly*/ IImapMessageFlagSet applicableFlags;
    private /*readonly*/ IImapMessageDynamicAttribute messageAttribute;
    private readonly long? expunge;
    private readonly long? exists;
    private readonly long? recent;
  }
}
