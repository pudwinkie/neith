// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2010-2011 smdn
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

using Smdn.Net;
using Smdn.Net.Pop3.Protocol;
using Smdn.Net.Pop3.Protocol.Client;
using Smdn.Net.Pop3.Client.Transaction.BuiltIn;
using Smdn.Security.Authentication.Sasl.Client;

namespace Smdn.Net.Pop3.Client.Session {
  partial class PopSession {
    /*
     * transaction methods : authorization state
     */
    private string lastUserCommandArgument = null; // used by USER and PASS command

    /// <summary>sends STLS command</summary>
    /// <remarks>valid in authorization state</remarks>
    public PopCommandResult Stls(UpgradeConnectionStreamCallback createAuthenticatedStreamCallback)
    {
      return Stls(createAuthenticatedStreamCallback, false);
    }

    /// <summary>sends STLS command</summary>
    /// <remarks>valid in authorization state</remarks>
    public PopCommandResult Stls(UpgradeConnectionStreamCallback createAuthenticatedStreamCallback, bool reissueCapability)
    {
      RejectNonAuthorizationState();

      if (createAuthenticatedStreamCallback == null)
        throw new ArgumentNullException("createAuthenticatedStreamCallback");

      PopCommandResult result;

      using (var t = new StlsTransaction(connection, createAuthenticatedStreamCallback)) {
        if ((result = ProcessTransaction(t)).Succeeded)
          // RFC 2595 Using TLS with IMAP, POP3 and ACAP
          // http://tools.ietf.org/html/rfc2595
          // 4. POP3 STARTTLS extension
          //              Once TLS has been started, the client MUST discard cached
          //              information about server capabilities and SHOULD re-issue
          //              the CAPA command.  This is necessary to protect against
          //              man-in-the-middle attacks which alter the capabilities list
          //              prior to STLS.  The server MAY advertise different
          //              capabilities after STLS.
          SetServerCapabilities(null);
        else
          return result;
      }

      if (reissueCapability)
        Capa();

      return result;
    }

    /// <summary>sends USER command</summary>
    /// <remarks>valid in authorization state</remarks>
    public PopCommandResult User(ICredentialsByHost credentials)
    {
      if (credentials == null)
        throw new ArgumentNullException("credentials");

      var credential = credentials.LookupCredential(connection, null, null);

      if (credential == null)
        return new PopCommandResult(PopCommandResultCode.RequestError,
                                    string.Format("credential not found for {0}:{1}", connection.Host, connection.Port));

      return User(credential.UserName);
    }

    public PopCommandResult User(string username)
    {
      RejectNonConnectedState();

      if (state != PopSessionState.Authorization)
        return new PopCommandResult(PopCommandResultCode.RequestDone,
                                    "already authenticated");

      if (string.IsNullOrEmpty(username))
        return new PopCommandResult(PopCommandResultCode.RequestError,
                                    "username is empty");

      using (var t = new UserTransaction(connection)) {
        t.RequestArguments["name"] = username;

        if (ProcessTransaction(t).Succeeded)
          lastUserCommandArgument = username;
        else
          lastUserCommandArgument = null;

        return t.Result;
      }
    }

    /// <summary>sends PASS command</summary>
    /// <remarks>valid in authorization state</remarks>
    public PopCommandResult Pass(ICredentialsByHost credentials)
    {
      if (credentials == null)
        throw new ArgumentNullException("credentials");

      return PassCore(credentials, null);
    }

    /// <summary>sends PASS command</summary>
    /// <remarks>valid in authorization state</remarks>
    public PopCommandResult Pass(string password)
    {
      return PassCore(null, password);
    }

    private PopCommandResult PassCore(ICredentialsByHost credentials, string password)
    {
      var ret = RejectNonConnectedOrGetAuthenticatedResult();

      if (ret != null)
        return ret;

      if (lastUserCommandArgument == null)
        throw new PopProtocolViolationException("issue USER command first");

      if (password == null) {
        var credential = credentials.LookupCredential(connection, lastUserCommandArgument, null);

        if (credential == null)
          return new PopCommandResult(PopCommandResultCode.RequestError,
                                      string.Format("credential not found for {0}@{1}:{2}", lastUserCommandArgument, connection.Host, connection.Port));

        password = credential.Password;
      }

      if (string.IsNullOrEmpty(password))
        return new PopCommandResult(PopCommandResultCode.RequestError,
                                    "password is empty");

      using (var t = new PassTransaction(connection)) {
        t.RequestArguments["string"] = password;

        if (ProcessTransaction(t).Succeeded) {
          UpdateAuthority(lastUserCommandArgument, null);
          TransitStateTo(PopSessionState.Transaction);
        }

        lastUserCommandArgument = null;

        return t.Result;
      }
    }

    /// <summary>sends USER and PASS command</summary>
    /// <remarks>valid in authorization state</remarks>
    public PopCommandResult Login(ICredentialsByHost credentials)
    {
      return Login(credentials, null);
    }

    /// <summary>sends USER and PASS command</summary>
    /// <remarks>valid in authorization state</remarks>
    public PopCommandResult Login(ICredentialsByHost credentials, string username)
    {
      RejectNonConnectedState();

      if (credentials == null)
        throw new ArgumentNullException("credentials");

      var credential = credentials.LookupCredential(connection, username, null);

      if (credential == null)
        return new PopCommandResult(PopCommandResultCode.RequestError,
                                    string.Format("credential not found for {0}@{1}:{2}", username, connection.Host, connection.Port));

      return Login(credential.UserName, credential.Password);
    }

    /// <summary>sends USER and PASS command</summary>
    /// <remarks>valid in authorization state</remarks>
    public PopCommandResult Login(string username, string password)
    {
      var result = User(username);

      if (result.Failed)
        return result;
      else
        return Pass(password);
    }

    /// <summary>sends APOP command</summary>
    /// <remarks>valid in authorization state</remarks>
    public PopCommandResult Apop(ICredentialsByHost credentials)
    {
      return Apop(credentials, null);
    }

    /// <summary>sends APOP command</summary>
    /// <remarks>valid in authorization state</remarks>
    public PopCommandResult Apop(ICredentialsByHost credentials, string username)
    {
      if (credentials == null)
        throw new ArgumentNullException("credentials");

      var ret = RejectNonConnectedOrGetAuthenticatedResult();

      if (ret != null)
        return ret;

      if (!ApopAvailable)
        throw new PopIncapableException("server does not support APOP");

      NetworkCredential credential;

      ret = LookupAppropriateCredential(credentials, username, PopAuthenticationMechanism.Apop, out credential);

      if (ret != null)
        return ret;
      else if (string.IsNullOrEmpty(credential.UserName))
        return new PopCommandResult(PopCommandResultCode.RequestError,
                                    "username of credential is empty");
      else if (string.IsNullOrEmpty(credential.Password))
        return new PopCommandResult(PopCommandResultCode.RequestError,
                                    "password of credential is empty");

      using (var t = new ApopTransaction(connection)) {
        t.RequestArguments["name"] = credential.UserName;
        t.RequestArguments["digest"] = ApopDigest.Calculate(timestamp, credential.Password);

        if (ProcessTransaction(t).Succeeded) {
          UpdateAuthority(credential.UserName, PopAuthenticationMechanism.Apop);
          TransitStateTo(PopSessionState.Transaction);
        }

        return t.Result;
      }
    }

    /// <summary>sends AUTH command</summary>
    /// <remarks>valid in authorization state</remarks>
    public PopCommandResult Auth(ICredentialsByHost credentials, PopAuthenticationMechanism authenticationMechanism)
    {
      return Auth(credentials, null, authenticationMechanism);
    }

    /// <summary>sends AUTH command</summary>
    /// <remarks>valid in authorization state</remarks>
    public PopCommandResult Auth(ICredentialsByHost credentials, string username, PopAuthenticationMechanism authenticationMechanism)
    {
      if (credentials == null)
        throw new ArgumentNullException("credentials");
      if (authenticationMechanism == null)
        throw new ArgumentNullException("authenticationMechanism");

      var ret = RejectNonConnectedOrGetAuthenticatedResult();

      if (ret != null)
        return ret;

      // TODO: check Request.Arguments, not here
      if (handlesIncapableAsException)
        CheckServerCapability(authenticationMechanism);

      NetworkCredential credential;

      ret = LookupAppropriateCredential(credentials, username, authenticationMechanism, out credential);

      if (ret != null)
        return ret;

      using (var t = new AuthTransaction(connection, credential)) {
        t.RequestArguments["mechanism"] = (string)authenticationMechanism;

        if (ProcessTransaction(t).Succeeded) {
          UpdateAuthority(credential.UserName, authenticationMechanism);
          TransitStateTo(PopSessionState.Transaction);
        }

        return t.Result;
      }
    }

    /// <summary>sends AUTH command</summary>
    /// <remarks>valid in authorization state</remarks>
    public PopCommandResult Auth(SaslClientMechanism specificAuthenticationMechanism)
    {
      if (specificAuthenticationMechanism == null)
        throw new ArgumentNullException("specificAuthenticationMechanism");

      var ret = RejectNonConnectedOrGetAuthenticatedResult();

      if (ret != null)
        return ret;

      using (var t = new AuthTransaction(connection, specificAuthenticationMechanism)) {
        if (ProcessTransaction(t).Succeeded) {
          var authMechanism = PopAuthenticationMechanism.GetKnownOrCreate(specificAuthenticationMechanism.Name);
          var username = specificAuthenticationMechanism.Credential == null
            ? null
            : specificAuthenticationMechanism.Credential.UserName;

          UpdateAuthority(username, authMechanism);
          TransitStateTo(PopSessionState.Transaction);
        }

        return t.Result;
      }
    }

    private PopCommandResult RejectNonConnectedOrGetAuthenticatedResult()
    {
      RejectNonConnectedState();

      if (state == PopSessionState.Authorization)
        return null;
      else
        return new PopCommandResult(PopCommandResultCode.RequestDone,
                                    "already authenticated");
    }

    private PopCommandResult LookupAppropriateCredential(ICredentialsByHost credentials,
                                                         string username,
                                                         PopAuthenticationMechanism authenticationMechanism,
                                                         out NetworkCredential credential)
    {
      credential = credentials.LookupCredential(connection, username, authenticationMechanism);

      if (credential == null)
        return new PopCommandResult(PopCommandResultCode.RequestError,
                                    string.Format("credential not found for {0};AUTH={1}@{2}:{3}", username, authenticationMechanism, connection.Host, connection.Port));
      else
        return null;
    }

    internal void UpdateAuthority(string username, PopAuthenticationMechanism authType)
    {
      authority.Scheme    = connection.IsSecurePortConnection ? PopUri.UriSchemePops : PopUri.UriSchemePop;
      authority.Host      = connection.Host;
      authority.Port      = connection.Port;
      authority.UserName  = username;
      authority.AuthType  = authType;

      TraceInfo(string.Concat("authority: ", authority));
    }
  }
}
