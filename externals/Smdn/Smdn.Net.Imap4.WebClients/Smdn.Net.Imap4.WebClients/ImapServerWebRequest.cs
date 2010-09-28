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

using Smdn.Net.Imap4.Client.Session;
using Smdn.Net.Imap4.Client.Transaction;

namespace Smdn.Net.Imap4.WebClients {
  // TODO: [Serializable]
  internal sealed class ImapServerWebRequest : ImapWebRequest {
    public ImapServerWebRequest(Uri requestUri, ImapSessionManager sessionManager)
      : base(requestUri, ImapWebRequestDefaults.Subscription ? ImapWebRequestMethods.Lsub : ImapWebRequestMethods.List, sessionManager)
    {
    }

    protected ImapServerWebRequest(SerializationInfo serializationInfo, StreamingContext streamingContext)
      : base(serializationInfo, streamingContext)
    {
    }

    protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
      base.GetObjectData(serializationInfo, streamingContext);
    }

    protected override ImapWebResponse InternalGetResponse()
    {
      switch (Method) {
        case ImapWebRequestMethods.Lsub: return GetListLsubResponse(false);
        case ImapWebRequestMethods.List: return GetListLsubResponse(true);
        case ImapWebRequestMethods.XList: return GetXListResponse();
        default: throw new NotSupportedException(string.Format("unsupported request method: {0}", Method));
      }
    }

    private ImapWebResponse GetListLsubResponse(bool sendList)
    {
      ImapMailbox[] mailboxes;

      var response = new ImapWebResponse(sendList ? Session.List(out mailboxes) : Session.Lsub(out mailboxes));

      if (response.Result.Succeeded)
        response.Mailboxes = mailboxes;

      return response;
    }

    private ImapWebResponse GetXListResponse()
    {
      ImapMailbox[] mailboxes;

      var response = new ImapWebResponse(Session.XList(out mailboxes));

      if (response.Result.Succeeded)
        response.Mailboxes = mailboxes;

      return response;
    }
  }
}
