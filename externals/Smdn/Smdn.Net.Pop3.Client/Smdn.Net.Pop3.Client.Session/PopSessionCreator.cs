// 
// Author:
//       smdn <smdn@mail.invisiblefulmoon.net>
// 
// Copyright (c) 2010 smdn
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
using Smdn.Net.Pop3.Protocol;
using Smdn.Net.Pop3.Protocol.Client;
using Smdn.Security.Authentication.Sasl;
using Smdn.Security.Authentication.Sasl.Client;

namespace Smdn.Net.Pop3.Client.Session {
  public static class PopSessionCreator {
    public static PopSession CreateSession(IPopSessionProfile profile,
                                           SaslClientMechanism authMechanismSpecified,
                                           UpgradeConnectionStreamCallback createSslStreamCallback)
    {
      PopSession session;

      var result = CreateSession(profile, authMechanismSpecified, createSslStreamCallback, out session);

      if (result.Succeeded)
        return session;
      else
        throw new PopAuthenticationException(result);
    }

    public static PopCommandResult CreateSession(IPopSessionProfile profile,
                                                 SaslClientMechanism authMechanismSpecified,
                                                 UpgradeConnectionStreamCallback createSslStreamCallback,
                                                 out PopSession session)
    {
      if (profile == null)
        throw new ArgumentNullException("profile");

      var authority = profile.Authority;
      var securePort = string.Equals(authority.Scheme, PopUri.UriSchemePops, StringComparison.OrdinalIgnoreCase);

      if (securePort && createSslStreamCallback == null)
        throw new ArgumentNullException("createSslStreamCallback");

      PopCommandResult result;
      session = null;

      session = new PopSession(authority.Host,
                               authority.Port,
                               profile.Timeout,
                               securePort
                                 ? createSslStreamCallback
                                 : null);

      session.HandlesIncapableAsException = false;
      session.TransactionTimeout  = profile.Timeout;
      session.SendTimeout         = profile.SendTimeout;
      session.ReceiveTimeout      = profile.ReceiveTimeout;

      // try querying server capability (ignore error; POP3 Extension Mechanism might not supported)
      session.Capa();

      if (profile.UseTlsIfAvailable && session.ServerCapabilities.IsCapable(PopCapability.Stls) && !session.IsSecureConnection) {
        var r = session.Stls(createSslStreamCallback, false);

        if (r.Failed)
          throw new PopUpgradeConnectionException(r.ResultText);

        // try re-querying server capability (ignore error; POP3 Extension Mechanism might not supported)
        session.Capa();
      }

      if (authMechanismSpecified == null)
        result = Authenticate(session, profile);
      else
        result = session.Auth(authMechanismSpecified);

      if (result == null) {
        throw new PopAuthenticationException("appropriate authentication mechanism not found");
      }
      else if (result.Failed) {
        try {
          try {
            session.Disconnect(false);
          }
          catch (PopConnectionException) {
            // ignore
          }
        }
        finally {
          session = null;
        }
      }

      return result;
    }

    private static PopCommandResult Authenticate(PopSession session, IPopSessionProfile profile)
    {
      var authority = profile.Authority;
      var username = PopStyleUriParser.GetUser(authority);
      var authMechanism = PopStyleUriParser.GetAuthType(authority);

      /*
       * http://tools.ietf.org/html/rfc2384
       * 4. POP User Name and Authentication Mechanism
       * 
       *    The string ";AUTH=*" indicates that the client SHOULD select an
       *    appropriate authentication mechanism.  It MAY use any mechanism
       *    supported by the POP server.
       * 
       *    If a user name is included with no authentication mechanism, then
       *    ";AUTH=*" is assumed.
       */
      var canFallback = false;

      if (authMechanism == null) {
        if (string.IsNullOrEmpty(username)) {
          authMechanism = PopAuthenticationMechanism.Anonymous;
          canFallback = true;
        }
        else {
          authMechanism = PopAuthenticationMechanism.SelectAppropriate;
        }
      }

      if (authMechanism == PopAuthenticationMechanism.SelectAppropriate) {
        var allowInsecureMechanism = session.IsSecureConnection || profile.AllowInsecureLogin;

        return AuthenticateWithAppropriateMechanism(session,
                                                    allowInsecureMechanism,
                                                    profile.Credentials,
                                                    username,
                                                    GetUsingSaslMechanisms(profile.UsingSaslMechanisms ?? new string[0]));
      }
      else if (authMechanism == PopAuthenticationMechanism.Anonymous) {
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

    private static PopCommandResult AuthenticateWithAppropriateMechanism(PopSession session,
                                                                         bool allowInsecureMechanism,
                                                                         ICredentialsByHost credentials,
                                                                         string username,
                                                                         IEnumerable<string> usingSaslMechanisms)
    {
      PopCommandResult result = null;

      foreach (var mechanism in usingSaslMechanisms) {
        if (!allowInsecureMechanism && SaslClientMechanism.IsMechanismPlainText(mechanism))
          // disallow plain text mechanism
          continue;

        if (string.Equals(mechanism, SaslMechanisms.Anonymous, StringComparison.OrdinalIgnoreCase))
          // disallow 'ANONYMOUS' mechanism
          continue;

        var authMechanism = PopAuthenticationMechanism.GetKnownOrCreate(mechanism);

        if (session.ServerCapabilities.IsCapable(authMechanism)) {
          result = session.Auth(credentials, username, authMechanism);

          if (result.Succeeded)
            break;
        }
      }

      if ((result == null || result.Failed) && allowInsecureMechanism) {
        if (session.ApopAvailable)
          result = session.Apop(credentials, username);
      }

      if ((result == null || result.Failed) && allowInsecureMechanism)
        result = session.Login(credentials, username);

      return result;
    }

    private static PopCommandResult AuthenticateWithSuppliedMechanism(PopSession session,
                                                                      ICredentialsByHost credentials,
                                                                      string username,
                                                                      PopAuthenticationMechanism authMechanism)
    {
      /*
       * http://tools.ietf.org/html/rfc2384
       * 4. POP User Name and Authentication Mechanism
       * 
       *    An authentication mechanism can be expressed by adding ";AUTH=<enc-
       *    auth-type>" to the end of the user name.  If the authentication
       *    mechanism name is not preceded by a "+", it is a SASL POP [SASL]
       *    mechanism.  If it is preceded by a "+", it is either "APOP" or an
       *    extension mechanism.
       * 
       *    When an <enc-auth-type> is specified, the client SHOULD request
       *    appropriate credentials from that mechanism and use the "AUTH",
       *    "APOP", or extension command instead of the "USER" command.  If no
       *    user name is specified, one SHOULD be obtained from the mechanism or
       *    requested from the user as appropriate.
       * 
       * 
       *    If an <enc-auth-type> other than ";AUTH=*" is specified, the client
       *    SHOULD NOT use a different mechanism without explicit user
       *    permission.
       */
      return session.Auth(credentials, username, authMechanism);
    }

    private static PopCommandResult AuthenticateAsAnonymous(PopSession session,
                                                            string username,
                                                            bool canFallback)
    {
      PopCommandResult result = null;

      if (string.IsNullOrEmpty(username))
        username = "anonymous@";

      if (session.ServerCapabilities.IsCapable(PopAuthenticationMechanism.Anonymous))
        // try AUTH ANONYUMOUS
        result = session.Auth(new NetworkCredential(username, string.Empty),
                              null,
                              PopAuthenticationMechanism.Anonymous);

      if (result == null || (result.Failed && canFallback))
        // try anonymous LOGIN
        result = session.Login(new NetworkCredential("anonymous", username),
                               null);

      return result;
    }
  }
}
