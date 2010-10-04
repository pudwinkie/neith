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

using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Transaction.BuiltIn {
  internal sealed class StartTlsTransaction : ImapTransactionBase, IImapExtension {
    ImapCapability IImapExtension.RequiredCapability {
      get { return ImapCapability.StartTls; }
    }

    public StartTlsTransaction(ImapConnection connection, UpgradeConnectionStreamCallback createAuthenticatedStreamCallback)
      : base(connection)
    {
      if (createAuthenticatedStreamCallback == null)
        throw new ArgumentNullException("createAuthenticatedStreamCallback");

      this.createAuthenticatedStreamCallback = createAuthenticatedStreamCallback;
    }

    protected override ProcessTransactionDelegate Reset()
    {
      return ProcessStartTls;
    }

    // 6.2.1. STARTTLS Command
    //    Arguments:  none
    //    Responses:  no specific response for this command
    //    Result:     OK - starttls completed, begin TLS negotiation
    //                BAD - command unknown or arguments invalid
    private void ProcessStartTls()
    {
      SendCommand("STARTTLS", ProcessReceiveResponse);
    }

    protected override void OnTaggedStatusResponseReceived(ImapTaggedStatusResponse tagged)
    {
      if (tagged.Condition == ImapResponseCondition.Ok) {
        // 6.2.1. STARTTLS Command
        //       A [TLS] negotiation begins immediately after the CRLF at the end
        //       of the tagged OK response from the server.  Once a client issues a
        //       STARTTLS command, it MUST NOT issue further commands until a
        //       server response is seen and the [TLS] negotiation is complete.
        try {
          Connection.UpgradeStream(createAuthenticatedStreamCallback);
        }
        catch (ImapUpgradeConnectionException ex) {
          throw new ImapSecureConnectionException(ex.Message, ex);
        }

        Connection.SetIsSecureConnection(true);

        FinishOk(tagged);
      }
      else {
        base.OnTaggedStatusResponseReceived(tagged);
      }
    }

    private /*readonly*/ UpgradeConnectionStreamCallback createAuthenticatedStreamCallback;
  }
}