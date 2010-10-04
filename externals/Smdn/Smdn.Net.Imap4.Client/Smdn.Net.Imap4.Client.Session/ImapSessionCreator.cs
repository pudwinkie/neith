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

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif
using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;
using Smdn.Security.Authentication.Sasl;
using Smdn.Security.Authentication.Sasl.Client;

namespace Smdn.Net.Imap4.Client.Session {
  public static class ImapSessionCreator {
    public static ImapSession CreateSession(IImapSessionProfile profile,
                                            SaslClientMechanism authMechanismSpecified,
                                            UpgradeConnectionStreamCallback createSslStreamCallback)
    {
      ImapSession session;

      var result = CreateSession(profile, authMechanismSpecified, createSslStreamCallback, out session);

      if (result.Succeeded)
        return session;
      else
        throw new ImapAuthenticationException(result);
    }

    public static ImapCommandResult CreateSession(IImapSessionProfile profile,
                                                  SaslClientMechanism authMechanismSpecified,
                                                  UpgradeConnectionStreamCallback createSslStreamCallback,
                                                  out ImapSession session)
    {
      if (profile == null)
        throw new ArgumentNullException("profile");

      var authority = profile.Authority;
      var securePort = string.Equals(authority.Scheme, ImapUri.UriSchemeImaps, StringComparison.OrdinalIgnoreCase);

      if (securePort && createSslStreamCallback == null)
        throw new ArgumentNullException("createSslStreamCallback");

      ImapCommandResult result;
      session = null;

      session = new ImapSession(authority.Host,
                                authority.Port,
                                true,
                                profile.Timeout,
                                securePort
                                  ? createSslStreamCallback
                                  : null);

      session.HandlesIncapableAsException = false;
      session.HandlesReferralAsException = false;
      session.TransactionTimeout  = profile.Timeout;
      session.SendTimeout         = profile.SendTimeout;
      session.ReceiveTimeout      = profile.ReceiveTimeout;

      if (session.ServerCapabilities.Count == 0)
        // try querying server capability (ignore error)
        session.Capability();

      if (!session.ServerCapabilities.Has(ImapCapability.Imap4Rev1))
        throw new ImapIncapableException(ImapCapability.Imap4Rev1);

      if (profile.UseTlsIfAvailable && session.ServerCapabilities.Has(ImapCapability.StartTls) && !session.IsSecureConnection) {
        var r = session.StartTls(createSslStreamCallback, true);

        if (r.Failed)
          throw new ImapSecureConnectionException(r.ResultText);
        else if (!session.ServerCapabilities.Has(ImapCapability.Imap4Rev1))
          throw new ImapIncapableException(ImapCapability.Imap4Rev1);
      }

      if (profile.UseDeflateIfAvailable && session.ServerCapabilities.Has(ImapCapability.CompressDeflate)) {
        var r = session.Compress(ImapCompressionMechanism.Deflate);

        if (r.Failed)
          throw new WebException(r.ResultText, null, WebExceptionStatus.RequestCanceled, null);
      }

      if (authMechanismSpecified == null)
        result = Authenticate(session, profile);
      else
        result = session.Authenticate(authMechanismSpecified);

      if (result == null) {
        throw new ImapAuthenticationException("appropriate authentication mechanism not found");
      }
      else if (result.Failed) {
        try {
          try {
            session.Disconnect(false);
          }
          catch (ImapConnectionException) {
            // ignore
          }
        }
        finally {
          session = null;
        }
      }

      return result;
    }

    private static ImapCommandResult Authenticate(ImapSession session, IImapSessionProfile profile)
    {
      var authority = profile.Authority;
      var username = ImapStyleUriParser.GetUser(authority);
      var authMechanism = ImapStyleUriParser.GetAuthType(authority);

      /*
       * http://tools.ietf.org/html/rfc5092
       * 3.2. IMAP User Name and Authentication Mechanism
       * 
       *    An authentication mechanism (as used by the IMAP AUTHENTICATE
       *    command) can be expressed by adding ";AUTH=<enc-auth-type>" to the
       *    end of the user name in an IMAP URL.  When such an <enc-auth-type> is
       *    indicated, the client SHOULD request appropriate credentials from
       *    that mechanism and use the "AUTHENTICATE" command instead of the
       *    "LOGIN" command.  If no user name is specified, one MUST be obtained
       *    from the mechanism or requested from the user/configuration as
       *    appropriate.
       *
       *    If a user name is included with no authentication mechanism, then
       *    ";AUTH=*" is assumed.
       */

      // TODO: URLAUTH
      /*
      if (!session.ServerCapabilities.Has(ImapCapability.UrlAuth)
        throw new ImapWebException("URLAUTH incapable", WebExceptionStatus.ProtocolError);
      */
      var canFallback = false;

      if (authMechanism == null) {
        if (string.IsNullOrEmpty(username)) {
          authMechanism = ImapAuthenticationMechanism.Anonymous;
          canFallback = true;
        }
        else {
          authMechanism = ImapAuthenticationMechanism.SelectAppropriate;
        }
      }

      if (authMechanism == ImapAuthenticationMechanism.SelectAppropriate) {
        var allowPlainTextMechanism = session.IsSecureConnection || profile.AllowInsecureLogin;

        return AuthenticateWithAppropriateMechanism(session,
                                                    allowPlainTextMechanism,
                                                    profile.Credentials,
                                                    username,
                                                    GetUsingSaslMechanisms(profile.UsingSaslMechanisms ?? new string[0]));
      }
      else if (authMechanism == ImapAuthenticationMechanism.Anonymous) {
        return AuthenticateAsAnonymous(session,
                                       username,
                                       canFallback);
      }
      else {
        return AuthenticateWithSuppliedMechanism(session,
                                                 profile.Credentials,
                                                 username,
                                                 authMechanism);
      }
    }

    private static IEnumerable<string> GetUsingSaslMechanisms(string[] usingSaslMechanisms)
    {
      var availableMechanisms = SaslClientMechanism.GetAvailableMechanisms();

      foreach (var mechanism in usingSaslMechanisms) {
        if (availableMechanisms.Any(delegate(string val) {
          return string.Equals(val, mechanism, StringComparison.OrdinalIgnoreCase);
        }))
          yield return mechanism;
      }
    }

    private static ImapCommandResult AuthenticateWithAppropriateMechanism(ImapSession session,
                                                                          bool allowPlainTextMechanism,
                                                                          ICredentialsByHost credentials,
                                                                          string username,
                                                                          IEnumerable<string> usingSaslMechanisms)
    {
      ImapCommandResult result = null;

      foreach (var mechanism in usingSaslMechanisms) {
        if (!allowPlainTextMechanism && SaslClientMechanism.IsMechanismPlainText(mechanism))
          // disallow plain text mechanism
          continue;

        if (string.Equals(mechanism, SaslMechanisms.Anonymous, StringComparison.OrdinalIgnoreCase))
          // disallow 'ANONYMOUS' mechanism
          continue;

        var authMechanism = ImapAuthenticationMechanism.GetKnownOrCreate(mechanism);

        if (session.ServerCapabilities.IsCapable(authMechanism)) {
          result = session.Authenticate(credentials, username, authMechanism, true);

          if (result.Succeeded)
            break;
        }
      }

      if ((result == null || result.Failed) &&
          allowPlainTextMechanism &&
          !session.ServerCapabilities.Has(ImapCapability.LoginDisabled))
        result = session.Login(credentials, username, true);

      return result;
    }

    private static ImapCommandResult AuthenticateWithSuppliedMechanism(ImapSession session,
                                                                       ICredentialsByHost credentials,
                                                                       string username,
                                                                       ImapAuthenticationMechanism authMechanism)
    {
      /*
       * http://tools.ietf.org/html/rfc5092
       * 3.2. IMAP User Name and Authentication Mechanism
       * 
       *    An authentication mechanism (as used by the IMAP AUTHENTICATE
       *    command) can be expressed by adding ";AUTH=<enc-auth-type>" to the
       *    end of the user name in an IMAP URL.  When such an <enc-auth-type> is
       *    indicated, the client SHOULD request appropriate credentials from
       *    that mechanism and use the "AUTHENTICATE" command instead of the
       *    "LOGIN" command.  If no user name is specified, one MUST be obtained
       *    from the mechanism or requested from the user/configuration as
       *    appropriate.
       */
      return session.Authenticate(credentials, username, authMechanism, true);
    }

    private static ImapCommandResult AuthenticateAsAnonymous(ImapSession session,
                                                             string username,
                                                             bool canFallback)
    {
      /*
       * http://tools.ietf.org/html/rfc5092
       * 3.2. IMAP User Name and Authentication Mechanism
       * 
       *    If no user name and no authentication mechanism are supplied, the
       *    client MUST authenticate as anonymous to the server.  If the server
       *    advertises AUTH=ANONYMOUS IMAP capability, the client MUST use the
       *    AUTHENTICATE command with ANONYMOUS [ANONYMOUS] SASL mechanism.  If
       *    SASL ANONYMOUS is not available, the (case-insensitive) user name
       *    "anonymous" is used with the "LOGIN" command and the Internet email
       *    address of the end user accessing the resource is supplied as the
       *    password.  The latter option is given in order to provide for
       *    interoperability with deployed servers.
       * 
       *    Note that, as described in RFC 3501, the "LOGIN" command MUST NOT be
       *    used when the IMAP server advertises the LOGINDISABLED capability.
       */
      ImapCommandResult result = null;

      if (string.IsNullOrEmpty(username))
        username = "anonymous@";

      if (session.ServerCapabilities.IsCapable(ImapAuthenticationMechanism.Anonymous))
        // try AUTHENTICATE ANONYUMOUS
        result = session.Authenticate(new NetworkCredential(username, string.Empty),
                                      null,
                                      ImapAuthenticationMechanism.Anonymous,
                                      true);

      if ((result == null || (result.Failed && canFallback)) &&
          !session.ServerCapabilities.Has(ImapCapability.LoginDisabled))
        // try anonymous LOGIN
        result = session.Login(new NetworkCredential("anonymous", username),
                               null,
                               true);

      return result;
    }
  }
}
