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
using System.Net;

using Smdn.Formats;
using Smdn.Net.Pop3.Protocol.Client;
using Smdn.Security.Authentication.Sasl;
using Smdn.Security.Authentication.Sasl.Client;

namespace Smdn.Net.Pop3.Client.Transaction.BuiltIn {
  /*
   * http://tools.ietf.org/html/rfc5034
   * RFC 5034 - The Post Office Protocol (POP3) Simple Authentication and Security Layer (SASL) Authentication Mechanism
   */
  internal sealed class AuthTransaction : PopTransactionBase, IPopExtension {
    PopCapability IPopExtension.RequiredCapability {
      get { return requiredCapability; }
    }

    public AuthTransaction(PopConnection connection, NetworkCredential credential)
      : base(connection)
    {
      if (credential == null)
        throw new ArgumentNullException("credential");

      this.credential = credential;
      this.requiredCapability = PopCapability.Sasl;
      this.disposeMechanism = true;
    }

    public AuthTransaction(PopConnection connection, SaslClientMechanism authMechanism)
      : base(connection)
    {
      if (authMechanism == null)
        throw new ArgumentNullException("authMechanism");

      this.saslMechanism = authMechanism;
      this.requiredCapability = null; // do not check capability
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
        if (!RequestArguments.ContainsKey("mechanism"))
          return ProcessArgumentNotSetted;
#endif
        try {
          saslMechanism = SaslClientMechanism.Create(RequestArguments["mechanism"]);
          saslMechanism.Credential = credential;

          // The service name specified by this protocol's profile of SASL
          // is "pop".
          saslMechanism.ServiceName = "pop";

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

        RequestArguments["mechanism"] = saslMechanism.Name;
      }

      /*
       * The optional initial-response argument to the AUTH command is
       * used to save a round trip when using authentication mechanisms
       * that support an initial client response.
       */
      if (saslMechanism.ClientFirst) {
        byte[] initialClientResponse;

        if (saslMechanism.GetInitialResponse(out initialClientResponse) == SaslExchangeStatus.Failed || initialClientResponse == null)
          return ProcessInitialResponseError;

        RequestArguments["initial-response"] = Base64.GetEncodedString(initialClientResponse);
      }

      return ProcessAuth;
    }

#if DEBUG
    private void ProcessArgumentNotSetted()
    {
      FinishError(PopCommandResultCode.RequestError, "arguments 'mechanism' must be setted");
    }
#endif

    private void ProcessUnsupported()
    {
      FinishError(PopCommandResultCode.RequestError, "unsupported authentication mechanism");
    }

    private void ProcessInitialResponseError()
    {
      FinishError(PopCommandResultCode.RequestError, "can't send initial client response");
    }

    /*
     * 4. The AUTH Command
     * 
     *    AUTH mechanism [initial-response]
     * 
     *       Arguments:
     * 
     *          mechanism: A string identifying a SASL authentication
     *          mechanism.
     * 
     *          initial-response: An optional initial client response, as
     *          defined in Section 3 of [RFC4422].  If present, this response
     *          MUST be encoded as Base64 (specified in Section 4 of
     *          [RFC4648]), or consist only of the single character "=", which
     *          represents an empty initial response.
     * 
     *       Restrictions:
     * 
     *          After an AUTH command has been successfully completed, no more
     *          AUTH commands may be issued in the same session.  After a
     *          successful AUTH command completes, a server MUST reject any
     *          further AUTH commands with an -ERR reply.
     * 
     *          The AUTH command may only be given during the AUTHORIZATION
     *          state.
     */
    private void ProcessAuth()
    {
      string initialResponse;

      if (RequestArguments.TryGetValue("initial-response", out initialResponse))
        SendCommand("AUTH",
                    ProcessReceiveResponse,
                    RequestArguments["mechanism"],
                    initialResponse);
      else
        SendCommand("AUTH",
                    ProcessReceiveResponse,
                    RequestArguments["mechanism"]);
    }

    protected override void OnContinuationRequestReceived(PopContinuationRequest continuationRequest)
    {
      /*
       * A client response consists of a line containing a string
       * encoded as Base64.  If the client wishes to cancel the
       * authentication exchange, it issues a line with a single "*".
       * If the server receives such a response, it MUST reject the AUTH
       * command by sending an -ERR reply.
       * 
       * The optional initial-response argument to the AUTH command is
       * used to save a round trip when using authentication mechanisms
       * that support an initial client response.  If the initial
       * response argument is omitted and the chosen mechanism requires
       * an initial client response, the server MUST proceed by issuing
       * an empty challenge, as defined in Section 3 of [RFC4422].  In
       * POP3, an empty server challenge is defined as a line with only
       * a "+", followed by a single space.  It MUST NOT contain any
       * other data.
       */
      var serverChallenge = Base64.Decode(continuationRequest.Base64Text.ByteArray);
      byte[] clientResponse;
      var status = saslMechanism.Exchange(serverChallenge, out clientResponse);

      if (status == SaslExchangeStatus.Failed || clientResponse == null)
        SendContinuation("*");
      else
        SendContinuation(Base64.GetEncodedString(clientResponse));
    }

    private NetworkCredential credential;
    private SaslClientMechanism saslMechanism;
    private PopCapability requiredCapability;
    private readonly bool disposeMechanism;
  }
}
