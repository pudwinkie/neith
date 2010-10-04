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
using System.Net;
using System.Text;

using Smdn.Formats;
using Smdn.Net.Imap4.Protocol.Client;
using Smdn.Security.Authentication.Sasl;
using Smdn.Security.Authentication.Sasl.Client;

namespace Smdn.Net.Imap4.Client.Transaction.BuiltIn {
  internal sealed class AuthenticateTransaction : ImapTransactionBase {
    public AuthenticateTransaction(ImapConnection connection, NetworkCredential credential, bool sendInitialResponse)
      : base(connection)
    {
      if (credential == null)
        throw new ArgumentNullException("credential");

      this.credential = credential;
      this.sendInitialResponse = sendInitialResponse;
      this.disposeMechanism = true;
    }

    public AuthenticateTransaction(ImapConnection connection, SaslClientMechanism authMechanism, bool sendInitialResponse)
      : base(connection)
    {
      if (authMechanism == null)
        throw new ArgumentNullException("authMechanism");

      this.saslMechanism = authMechanism;
      this.sendInitialResponse = sendInitialResponse;
      this.disposeMechanism = false;
    }

    public override void Dispose()
    {
      if (saslMechanism != null) {
        if (disposeMechanism)
          saslMechanism.Dispose();

        saslMechanism = null;
      }

      base.Dispose();
    }

    protected override ProcessTransactionDelegate Reset()
    {
      if (saslMechanism == null) {
        /*
         * create mechanism
         */
#if DEBUG
        if (!RequestArguments.ContainsKey("authentication mechanism name"))
          return ProcessArgumentNotSetted;
#endif
        try {
          saslMechanism = SaslClientMechanism.Create((string)RequestArguments["authentication mechanism name"]);
          saslMechanism.Credential = credential;

          // The service name specified by this protocol's profile of [SASL] is
          // "imap".
          saslMechanism.ServiceName = "imap";

          if (saslMechanism is NTLMMechanism)
            (saslMechanism as NTLMMechanism).TargetHost = Connection.Host;
        }
        catch (SaslMechanismNotSupportedException) {
          return ProcessUnsupported;
        }
      }
      else {
        /*
         * use supplied mechanism
         */
        saslMechanism.Initialize();

        RequestArguments["authentication mechanism name"] = saslMechanism.Name;
      }

      /*
       * http://tools.ietf.org/html/rfc4959
       * RFC 4959 - IMAP Extension for Simple Authentication and Security Layer (SASL) Initial Client Response
       * 3. IMAP Changes to the IMAP AUTHENTICATE Command
       *    Note: support and use of the initial client response is optional for
       *    both clients and servers.  Servers that implement this extension MUST
       *    support clients that omit the initial client response, and clients
       *    that implement this extension MUST NOT send an initial client
       *    response to servers that do not advertise the SASL-IR capability.
       */
      if (sendInitialResponse && saslMechanism.ClientFirst) {
        byte[] initialClientResponse;

        if (saslMechanism.GetInitialResponse(out initialClientResponse) == SaslExchangeStatus.Failed || initialClientResponse == null)
          return ProcessInitialResponseError;

        /*
         * http://tools.ietf.org/html/rfc4959
         * RFC 4959 - IMAP Extension for Simple Authentication and Security Layer (SASL) Initial Client Response
         * 3. IMAP Changes to the IMAP AUTHENTICATE Command
         *    This extension adds an optional second argument to the AUTHENTICATE
         *    command that is defined in Section 6.2.2 of [RFC3501].  If this
         *    second argument is present, it represents the contents of the
         *    "initial client response" defined in Section 5.1 of [RFC4422].
         */
        RequestArguments["initial client response"] = Base64.GetEncodedString(initialClientResponse);
      }

      return ProcessAuthenticate;
    }

#if DEBUG
    private void ProcessArgumentNotSetted()
    {
      FinishError(ImapCommandResultCode.RequestError, "arguments 'authentication mechanism name' must be setted");
    }
#endif

    private void ProcessInitialResponseError()
    {
      FinishError(ImapCommandResultCode.RequestError, "can't send initial client response");
    }

    private void ProcessUnsupported()
    {
      FinishError(ImapCommandResultCode.RequestError, "unsupported authentication mechanism");
    }

    // 6.2.2. AUTHENTICATE Command
    //    Arguments:  authentication mechanism name
    //    Responses:  continuation data can be requested
    //    Result:     OK - authenticate completed, now in authenticated state
    //                NO - authenticate failure: unsupported authentication
    //                     mechanism, credentials rejected
    //                BAD - command unknown or arguments invalid,
    //                     authentication exchange cancelled
    private void ProcessAuthenticate()
    {
      if (RequestArguments.ContainsKey("initial client response"))
        SendCommand("AUTHENTICATE",
                    ProcessReceiveResponse,
                    RequestArguments["authentication mechanism name"],
                    RequestArguments["initial client response"]);
      else
        SendCommand("AUTHENTICATE",
                    ProcessReceiveResponse,
                    RequestArguments["authentication mechanism name"]);
    }

    protected override void OnCommandContinuationRequestReceived(ImapCommandContinuationRequest continuationRequest)
    {
      // The authentication protocol exchange consists of a series of
      // server challenges and client responses that are specific to the
      // authentication mechanism.  A server challenge consists of a
      // command continuation request response with the "+" token followed
      // by a BASE64 encoded string.  The client response consists of a
      // single line consisting of a BASE64 encoded string.  If the client
      // wishes to cancel an authentication exchange, it issues a line
      // consisting of a single "*".  If the server receives such a
      // response, it MUST reject the AUTHENTICATE command by sending a
      // tagged BAD response.
      var serverChallenge = Base64.Decode(continuationRequest.Text);
      byte[] clientResponse;
      var status = saslMechanism.Exchange(serverChallenge, out clientResponse);

      if (status == SaslExchangeStatus.Failed || clientResponse == null)
        SendContinuation("*");
      else
        SendContinuation(Base64.GetEncodedString(clientResponse));
    }

    private /*readonly*/ NetworkCredential credential;
    private readonly bool sendInitialResponse;
    private /*readonly*/ SaslClientMechanism saslMechanism;
    private readonly bool disposeMechanism;
  }
}
