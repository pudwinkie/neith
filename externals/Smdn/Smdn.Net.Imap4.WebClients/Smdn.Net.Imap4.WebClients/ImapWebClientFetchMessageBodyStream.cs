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
using System.Net;

using Smdn.Net.Imap4.Client.Session;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.WebClients {
  internal sealed class ImapWebClientFetchMessageBodyStream : ImapFetchMessageBodyStream {
    internal ImapWebClientFetchMessageBodyStream(ImapSession session,
                                                 bool keepAlive,
                                                 bool peek,
                                                 ImapSequenceSet fetchUidSet,
                                                 int fetchBlockSize,
                                                 int readWriteTimeout)
      : this(session, keepAlive, peek, fetchUidSet, null, null, fetchBlockSize, readWriteTimeout)
    {
    }

    internal ImapWebClientFetchMessageBodyStream(ImapSession session,
                                                 bool keepAlive,
                                                 bool peek,
                                                 ImapSequenceSet fetchUidSet,
                                                 string fetchSection,
                                                 ImapPartialRange? fetchRange,
                                                 int fetchBlockSize,
                                                 int readWriteTimeout)
      : base(session, peek, fetchUidSet, fetchSection, fetchRange, fetchBlockSize)
    {
      this.keepAlive = keepAlive;

      ReadTimeout   = readWriteTimeout;
      WriteTimeout  = readWriteTimeout;
    }

    protected override void Dispose(bool disposing)
    {
      if (!keepAlive && Session != null)
        ImapWebRequest.CloseSession(Session);

      base.Dispose(disposing);
    }

    internal new ImapCommandResult Prepare(ImapFetchDataItem fetchDataItemMacro, out IImapMessageAttribute messageAttr)
    {
      var result = base.Prepare(fetchDataItemMacro, out messageAttr);

      if (result.Failed)
        // not to close session by this instance
        DetachFromSession();

      return result;
    }

    protected override Exception GetNoSuchMessageException(ImapCommandResult result)
    {
      return new WebException("no such message or expunged", null, WebExceptionStatus.Success, new ImapWebResponse(result));
    }

    protected override Exception GetFetchFailureException(ImapCommandResult result)
    {
      return new WebException(result.ResultText, WebExceptionStatus.ReceiveFailure);
    }

    protected override Exception GetTimeoutException(TimeoutException ex)
    {
      return new WebException("timed out", ex, WebExceptionStatus.Timeout, null);
    }

    protected override Exception GetUnexpectedException(ImapException ex)
    {
      return new WebException("unexpected error", ex, WebExceptionStatus.UnknownError, null);
    }

    private readonly bool keepAlive;
  }
}
