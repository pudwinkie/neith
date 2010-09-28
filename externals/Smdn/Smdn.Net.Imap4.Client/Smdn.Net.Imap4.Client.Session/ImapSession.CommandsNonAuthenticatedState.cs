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

using Smdn.Collections;
using Smdn.Net.Imap4.Client.Transaction;
using Smdn.Net.Imap4.Client.Transaction.BuiltIn;
using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;
using Smdn.Security.Authentication.Sasl.Client;

namespace Smdn.Net.Imap4.Client.Session {
  partial class ImapSession {
    /*
     * transaction methods : non-authenticated state
     */

    /// <summary>sends STARTTLS command</summary>
    /// <remarks>valid in non-authenticated state</remarks>
    public ImapCommandResult StartTls(UpgradeConnectionStreamCallback createAuthenticatedStreamCallback)
    {
      return StartTls(createAuthenticatedStreamCallback, false);
    }

    /// <summary>sends STARTTLS command</summary>
    /// <remarks>valid in non-authenticated state</remarks>
    public ImapCommandResult StartTls(UpgradeConnectionStreamCallback createAuthenticatedStreamCallback, bool reissueCapability)
    {
      RejectNonConnectedState();

      if (state != ImapSessionState.NotAuthenticated)
        throw new ImapProtocolViolationException("already authenticated");

      if (createAuthenticatedStreamCallback == null)
        throw new ArgumentNullException("createAuthenticatedStreamCallback");

      ImapCommandResult result;

      using (var t = new StartTlsTransaction(connection, createAuthenticatedStreamCallback)) {
        if ((result = ProcessTransaction(t)).Succeeded)
          // 6.2.1. STARTTLS Command
          //       Once [TLS] has been started, the client MUST discard cached
          //       information about server capabilities and SHOULD re-issue the
          //       CAPABILITY command.
          SetServerCapabilities(new ImapCapabilityList());
        else
          return result;
      }

      if (reissueCapability)
        Capability();

      return result;
    }

    /// <summary>sends AUTHENTICATE command</summary>
    /// <remarks>valid in non-authenticated state</remarks>
    public ImapCommandResult Authenticate(ICredentialsByHost credentials,
                                          ImapAuthenticationMechanism authenticationMechanism)
    {
      return Authenticate(credentials, authenticationMechanism, false);
    }

    /// <summary>sends AUTHENTICATE command</summary>
    /// <remarks>valid in non-authenticated state</remarks>
    public ImapCommandResult Authenticate(ICredentialsByHost credentials,
                                          ImapAuthenticationMechanism authenticationMechanism,
                                          bool reissueCapability)
    {
      return Authenticate(credentials, null, authenticationMechanism, reissueCapability);
    }

    /// <summary>sends AUTHENTICATE command</summary>
    /// <remarks>valid in non-authenticated state</remarks>
    public ImapCommandResult Authenticate(ICredentialsByHost credentials,
                                          string username,
                                          ImapAuthenticationMechanism authenticationMechanism)
    {
      return Authenticate(credentials, username, authenticationMechanism, false);
    }

    /// <summary>sends AUTHENTICATE command</summary>
    /// <remarks>valid in non-authenticated state</remarks>
    public ImapCommandResult Authenticate(ICredentialsByHost credentials,
                                          string username,
                                          ImapAuthenticationMechanism authenticationMechanism,
                                          bool reissueCapability)
    {
      var ret = RejectNonConnectedOrGetAuthenticatedResult();

      if (ret != null)
        return ret;

      if (credentials == null)
        throw new ArgumentNullException("credentials");
      if (authenticationMechanism == null)
        throw new ArgumentNullException("authenticationMechanism");

      var credential = credentials.LookupCredential(connection, username, authenticationMechanism);

      if (credential == null)
        return new ImapCommandResult(ImapCommandResultCode.RequestError,
                                     string.Format("credential not found for {0};AUTH={1}@{2}:{3}", username, authenticationMechanism, connection.Host, connection.Port));

      using (var t = new AuthenticateTransaction(connection, credential, serverCapabilities.Has(ImapCapability.SaslIR))) {
        t.RequestArguments["authentication mechanism name"] = authenticationMechanism;

        return AuthenticateInternal(t, credential.UserName, authenticationMechanism, reissueCapability);
      }
    }

    /// <summary>sends AUTHENTICATE command</summary>
    /// <remarks>valid in non-authenticated state</remarks>
    public ImapCommandResult Authenticate(SaslClientMechanism specificAuthenticationMechanism)
    {
      return Authenticate(specificAuthenticationMechanism, false);
    }

    /// <summary>sends AUTHENTICATE command</summary>
    /// <remarks>valid in non-authenticated state</remarks>
    public ImapCommandResult Authenticate(SaslClientMechanism specificAuthenticationMechanism,
                                          bool reissueCapability)
    {
      var ret = RejectNonConnectedOrGetAuthenticatedResult();

      if (ret != null)
        return ret;

      if (specificAuthenticationMechanism == null)
        throw new ArgumentNullException("specificAuthenticationMechanism");

      using (var t = new AuthenticateTransaction(connection, specificAuthenticationMechanism, serverCapabilities.Has(ImapCapability.SaslIR))) {
        var authMechanism = ImapAuthenticationMechanism.GetKnownOrCreate(specificAuthenticationMechanism.Name);
        var username = specificAuthenticationMechanism.Credential == null
          ? null
          : specificAuthenticationMechanism.Credential.UserName;

        return AuthenticateInternal(t, username, authMechanism, reissueCapability);
      }
    }

    /// <summary>sends LOGIN command</summary>
    /// <remarks>valid in non-authenticated state</remarks>
    public ImapCommandResult Login(ICredentialsByHost credentials)
    {
      return Login(credentials, false);
    }

    /// <summary>sends LOGIN command</summary>
    /// <remarks>valid in non-authenticated state</remarks>
    public ImapCommandResult Login(ICredentialsByHost credentials,
                                   bool reissueCapability)
    {
      return Login(credentials, null, reissueCapability);
    }

    /// <summary>sends LOGIN command</summary>
    /// <remarks>valid in non-authenticated state</remarks>
    public ImapCommandResult Login(ICredentialsByHost credentials,
                                   string username)
    {
      return Login(credentials, username, false);
    }

    /// <summary>sends LOGIN command</summary>
    /// <remarks>valid in non-authenticated state</remarks>
    public ImapCommandResult Login(ICredentialsByHost credentials,
                                   string username,
                                   bool reissueCapability)
    {
      var ret = RejectNonConnectedOrGetAuthenticatedResult();

      if (ret != null)
        return ret;

      // RFC 2595 Using TLS with IMAP, POP3 and ACAP
      // http://tools.ietf.org/html/rfc2595
      // 3.2. IMAP LOGINDISABLED capability
      //    An IMAP client which complies with this specification MUST NOT issue
      //    the LOGIN command if this capability is present.
      if (serverCapabilities.Has(Imap4.ImapCapability.LoginDisabled))
        throw new ImapIncapableException("LOGIN is disabled");

      if (credentials == null)
        throw new ArgumentNullException("credentials");

      var credential = credentials.LookupCredential(connection, username, null);

      if (credential == null)
        return new ImapCommandResult(ImapCommandResultCode.RequestError,
                                     string.Format("credential not found for {0}@{1}:{2}", username, connection.Host, connection.Port));

      using (var t = new LoginTransaction(connection, credential)) {
        return AuthenticateInternal(t, credential.UserName, null, reissueCapability);
      }
    }

    private ImapCommandResult RejectNonConnectedOrGetAuthenticatedResult()
    {
      RejectNonConnectedState();

      if (state == ImapSessionState.NotAuthenticated)
        return null;
      else
        return new ImapCommandResult(ImapCommandResultCode.RequestDone,
                                     "already authenticated");
    }

    private ImapCommandResult AuthenticateInternal(IImapTransaction t,
                                                   string username,
                                                   ImapAuthenticationMechanism authenticationMechanism,
                                                   bool reissueCapability)
    {
      var result = ProcessTransaction(t);
      var refferalResponseCode = result.GetResponseCode(ImapResponseCode.Referral);

      if (refferalResponseCode != null) {
        // RFC 2221 IMAP4 Login Referrals
        // http://tools.ietf.org/html/rfc2221
        // 4.1. LOGIN and AUTHENTICATE Referrals
        //    An IMAP4 server MAY respond to a LOGIN or AUTHENTICATE command with a
        //    home server referral if it wishes to direct the user to another IMAP4
        //    server.
        var referToUri = ImapResponseTextConverter.FromReferral(refferalResponseCode.ResponseText)[0];

        if (handlesReferralAsException) {
          throw new ImapLoginReferralException(string.Format("try another server: '{0}'", refferalResponseCode.ResponseText.Text),
                                               referToUri);
        }
        else {
          Trace.Info("login referral: '{0}'", refferalResponseCode.ResponseText.Text);

          if (result.Succeeded)
            Trace.Info("  another server available at {0}", referToUri);
          else
            Trace.Info("  try to connect to {0}", referToUri);
        }
      }

      if (result.Succeeded) {
        UpdateAuthority(username, authenticationMechanism);
        TransitStateTo(ImapSessionState.Authenticated);

        // 6.2.2. AUTHENTICATE Command
        //       A server MAY include a CAPABILITY response code in the tagged OK
        //       response of a successful AUTHENTICATE command in order to send
        //       capabilities automatically.

        // 6.2.3. LOGIN Command
        //       A server MAY include a CAPABILITY response code in the tagged OK
        //       response to a successful LOGIN command in order to send
        //       capabilities automatically.  It is unnecessary for a client to
        //       send a separate CAPABILITY command if it recognizes these
        //       automatic capabilities.
        var capabilityResponseCode = result.GetResponseCode(ImapResponseCode.Capability);

        if (capabilityResponseCode == null) {
          var capability = result.GetResponse(ImapDataResponseType.Capability);

          if (capability != null) {
            SetServerCapabilities(ImapDataResponseConverter.FromCapability(capability));
            reissueCapability = false;
          }
        }
        else {
          SetServerCapabilities(ImapResponseTextConverter.FromCapability(capabilityResponseCode.ResponseText));
          reissueCapability = false;
        }
      }
      else {
        return result;
      }

      if (reissueCapability)
        Capability();

      return result;
    }

    internal void UpdateAuthority(string username, ImapAuthenticationMechanism authType)
    {
      authority.Scheme    = connection.IsSecurePortConnection ? ImapUri.UriSchemeImaps : ImapUri.UriSchemeImap;
      authority.Host      = connection.Host;
      authority.Port      = connection.Port;
      authority.UserName  = username;
      authority.AuthType  = authType;

      TraceInfo("authority: {0}", authority);
    }
  }
}
