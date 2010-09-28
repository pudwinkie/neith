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
using System.Text;
using System.Security.Cryptography;

using Smdn.Net.Pop3.Protocol;
using Smdn.Net.Pop3.Protocol.Client;

namespace Smdn.Net.Pop3.Client.Transaction.BuiltIn {
  /*
   * RFC 2595 Using TLS with IMAP, POP3 and ACAP
   * http://tools.ietf.org/html/rfc2595
   */
  internal sealed class StlsTransaction : PopTransactionBase, IPopExtension {
    PopCapability IPopExtension.RequiredCapability {
      get { return PopCapability.Stls; }
    }

    public StlsTransaction(PopConnection connection, UpgradeConnectionStreamCallback createAuthenticatedStreamCallback)
      : base(connection)
    {
      if (createAuthenticatedStreamCallback == null)
        throw new ArgumentNullException("createAuthenticatedStreamCallback");

      this.createAuthenticatedStreamCallback = createAuthenticatedStreamCallback;
    }

    protected override ProcessTransactionDelegate Reset()
    {
      return ProcessStls;
    }

    // 4. POP3 STARTTLS extension
    //     STLS
    //        Arguments: none
    //        Restrictions:
    //            Only permitted in AUTHORIZATION state.
    //        Possible Responses:
    //            +OK -ERR
    private void ProcessStls()
    {
      SendCommand("STLS", ProcessReceiveResponse);
    }

    protected override void OnStatusResponseReceived(PopStatusResponse status)
    {
      if (status.Status == PopStatusIndicator.Positive) {
        // 4. POP3 STARTTLS extension
        //              A TLS negotiation begins immediately after the CRLF at the
        //              end of the +OK response from the server.  A -ERR response
        //              MAY result if a security layer is already active.  Once a
        //              client issues a STLS command, it MUST NOT issue further
        //              commands until a server response is seen and the TLS
        //              negotiation is complete.
        Connection.UpgradeStream(createAuthenticatedStreamCallback);

        Connection.SetIsSecureConnection(true);
      }

      base.OnStatusResponseReceived(status);
    }

    private UpgradeConnectionStreamCallback createAuthenticatedStreamCallback;
  }
}