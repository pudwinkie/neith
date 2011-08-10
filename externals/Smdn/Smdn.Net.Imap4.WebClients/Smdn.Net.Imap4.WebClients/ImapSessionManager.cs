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
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Client.Session;

namespace Smdn.Net.Imap4.WebClients {
  public class ImapSessionManager {
    private static readonly ImapSessionManager instance = new ImapSessionManager();

    internal static ImapSessionManager Instance {
      get { return instance; }
    }

    public static RemoteCertificateValidationCallback ServerCertificateValidationCallback {
      get; set;
    }

    public static LocalCertificateSelectionCallback ClientCertificateSelectionCallback {
      get; set;
    }

    private static readonly X509Certificate2Collection clientCertificates = new X509Certificate2Collection();

    public static X509Certificate2Collection ClientCertificates {
      get { return clientCertificates; }
    }

    private static UpgradeConnectionStreamCallback createSslStreamCallback = CreateSslStream;

    public static UpgradeConnectionStreamCallback CreateSslStreamCallback {
      get { return createSslStreamCallback; }
      set
      {
        if (createSslStreamCallback == null)
          throw new ArgumentNullException("CreateSslStreamCallback");
        createSslStreamCallback = value;
      }
    }

    public static void DisconnectFrom(Uri serverUri)
    {
      if (!ImapUri.IsImap(serverUri))
        throw new ArgumentException("scheme must be imap or imaps", "serverUri");

      var port = serverUri.Port;

      if (port == -1)
        port = ImapUri.GetDefaultPortFromScheme(serverUri);

      DisconnectFrom(serverUri.Host, port);
    }

    public static void DisconnectFrom(string host, int port)
    {
      instance.InternalDisconnectFrom(host, port);
    }

    private static Stream CreateSslStream(ConnectionBase connection, Stream baseStream)
    {
      return ConnectionBase.CreateClientSslStream(connection,
                                                  baseStream,
                                                  clientCertificates,
                                                  ServerCertificateValidationCallback,
                                                  ClientCertificateSelectionCallback);
    }

    internal static ImapSession CreateSession(IImapSessionProfile profile)
    {
      try {
        ImapSession session = null;

        var result = ImapSessionCreator.CreateSession(profile, null, createSslStreamCallback, out session);

        if (result.Failed)
          throw new WebException(result.ResultText, null, WebExceptionStatus.ProtocolError, new ImapWebResponse(result));

        // update server info
        if (session.ServerCapabilities.Contains(ImapCapability.Namespace))
          session.Namespace();

        if (session.ServerCapabilities.Contains(ImapCapability.ID))
          session.ID(ImapWebRequestDefaults.ClientID);

        return session;
      }
      catch (ImapSecureConnectionException ex) {
        throw new WebException(ex.Message, ex.InnerException, WebExceptionStatus.SecureChannelFailure, null);
      }
      catch (ImapConnectionException ex) {
        throw new WebException("connection error", ex, WebExceptionStatus.ConnectFailure, null);
      }
      catch (ImapIncapableException ex) {
        throw new WebException(ex.Message, ex, WebExceptionStatus.RequestCanceled, null);
      }
      catch (ImapException ex) {
        if (ex.InnerException == null)
          throw new WebException(ex.Message, ex, WebExceptionStatus.RequestCanceled, null);
        else
          throw new WebException("internal error", ex, WebExceptionStatus.UnknownError, null);
      }
      catch (TimeoutException ex) {
        throw new WebException("timed out", ex, WebExceptionStatus.Timeout, null);
      }
      catch (WebException ex) {
        throw ex;
      }
    }

    /*
     * instance members
     */
    private ImapSessionManager()
    {
    }

    internal ImapSession GetExistSession(Uri requestUri)
    {
      ImapSession existSession;

      if (sessions.TryGetValue(ImapStyleUriParser.GetAuthority(requestUri), out existSession))
        return existSession;
      else
        return null;
    }

    internal void RegisterSession(Uri requestUri, ImapSession session)
    {
      var authority = ImapStyleUriParser.GetAuthority(requestUri);

      ImapSession existSession = null;

      lock ((sessions as System.Collections.ICollection).SyncRoot) {
        if (sessions.TryGetValue(authority, out existSession))
          sessions.Remove(authority);

        sessions.Add(authority, session);
      }

      if (existSession != null)
        existSession.Disconnect(true);
    }

    internal void UnregisterSession(Uri requestUri)
    {
      UnregisterSession(ImapStyleUriParser.GetAuthority(requestUri));
    }

    private void UnregisterSession(string authority)
    {
      lock ((sessions as System.Collections.ICollection).SyncRoot) {
        if (!sessions.ContainsKey(authority))
          return;

        sessions.Remove(authority);
      }
    }

    private void InternalDisconnectFrom(string host, int port)
    {
      if (host == null)
        throw new ArgumentNullException("host");
      if (port < IPEndPoint.MinPort || IPEndPoint.MaxPort < port)
        throw ExceptionUtils.CreateArgumentMustBeInRange(IPEndPoint.MinPort, IPEndPoint.MaxPort, "port", port);

      var keys = new List<string>();

      foreach (var pair in sessions) {
        var session = pair.Value;

        if (session.IsDisposed || session.State == ImapSessionState.NotConnected) {
          keys.Add(pair.Key);
        }
        else if (string.Equals(session.ConnectionInfo.Host, host, StringComparison.OrdinalIgnoreCase) &&
                 session.ConnectionInfo.Port == port) {
          keys.Add(pair.Key);

          session.Disconnect(true);
        }
      }

      foreach (var key in keys) {
        UnregisterSession(key);
      }
    }

    private Dictionary<string, ImapSession> sessions = new Dictionary<string, ImapSession>(StringComparer.OrdinalIgnoreCase);
  }
}