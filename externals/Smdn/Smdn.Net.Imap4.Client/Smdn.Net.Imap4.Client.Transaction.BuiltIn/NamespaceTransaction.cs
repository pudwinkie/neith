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

using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Transaction.BuiltIn {
  internal sealed class NamespaceTransaction : ImapTransactionBase<ImapCommandResult<ImapNamespace>>, IImapExtension {
    IEnumerable<ImapCapability> IImapExtension.RequiredCapabilities {
      get { yield return ImapCapability.Namespace; }
    }

    public NamespaceTransaction(ImapConnection connection)
      : base(connection)
    {
    }

    // 5. NAMESPACE Command
    //    Arguments: none
    //    Response:  an untagged NAMESPACE response that contains the prefix
    //                  and hierarchy delimiter to the server's Personal
    //                  Namespace(s), Other Users' Namespace(s), and Shared
    //                  Namespace(s) that the server wishes to expose. The
    //                  response will contain a NIL for any namespace class
    //                  that is not available. Namespace_Response_Extensions
    //                  MAY be included in the response.
    //                  Namespace_Response_Extensions which are not on the IETF
    //                  standards track, MUST be prefixed with an "X-".
    //    Result:    OK - Command completed
    //                  NO - Error: Can't complete command
    //                  BAD - argument invalid
    protected override ImapCommand PrepareCommand()
    {
      return Connection.CreateCommand("NAMESPACE");
    }

    protected override void OnDataResponseReceived(ImapDataResponse data)
    {
      if (data.Type == ImapDataResponseType.Namespace)
        namespaces = ImapDataResponseConverter.FromNamespace(data);

      base.OnDataResponseReceived(data);
    }

    protected override void OnTaggedStatusResponseReceived(ImapTaggedStatusResponse tagged)
    {
      if (tagged.Condition == ImapResponseCondition.Ok)
        Finish(new ImapCommandResult<ImapNamespace>(namespaces, tagged.ResponseText));
      else
        base.OnTaggedStatusResponseReceived(tagged);
    }

    private ImapNamespace namespaces = new ImapNamespace();
  }
}
