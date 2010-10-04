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
using System.Runtime.Remoting.Messaging;
using System.Threading;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif
using Smdn.Net.Imap4.Client.Session;
using Smdn.Net.Imap4.Protocol.Client;
using Smdn.Security.Authentication.Sasl.Client;

namespace Smdn.Net.Imap4.Client {
  public partial class ImapClient : IDisposable {
    public ImapClientProfile Profile {
      get { return profile; }
    }

    public int Timeout {
      get { return (session == null) ? profile.Timeout : session.TransactionTimeout; }
      set
      {
        profile.Timeout = value;

        if (session != null)
          session.TransactionTimeout = value;
      }
    }

    public int SendTimeout {
      get { return (session == null) ? profile.SendTimeout : session.SendTimeout; }
      set
      {
        profile.SendTimeout = value;

        if (session != null)
          session.SendTimeout = value;
      }
    }

    public int ReceiveTimeout {
      get { return (session == null) ? profile.ReceiveTimeout : session.ReceiveTimeout; }
      set
      {
        profile.ReceiveTimeout = value;

        if (session != null)
          session.ReceiveTimeout = value;
      }
    }

    public bool IsConnected {
      get { return (session != null && session.State != ImapSessionState.NotConnected); }
    }

    internal ImapSession Session {
      get { ThrowIfNotConnected(); return session; }
    }

    public bool IsSecureSession {
      get { return Session.IsSecureConnection; }
    }

    public ImapCapabilityList ServerCapabilities {
      get { return Session.ServerCapabilities; }
    }

    public IDictionary<string, string> ServerID {
      get { return Session.ServerID; }
    }

    public ImapNamespace ServerNamespace {
      get { return Session.Namespaces; }
    }

    /*
     * construction
     */
    public ImapClient()
      : this(new ImapClientProfile())
    {
    }

    public ImapClient(Uri authority)
      : this(new ImapClientProfile(authority))
    {
    }

    public ImapClient(string host)
      : this(new ImapClientProfile(host))
    {
    }

    public ImapClient(string host, string userName)
      : this(new ImapClientProfile(host, userName))
    {
    }

    public ImapClient(string host, bool securePort, string userName)
      : this(new ImapClientProfile(host, securePort, userName))
    {
    }

    public ImapClient(string host, int port)
      : this(new ImapClientProfile(host, port))
    {
    }

    public ImapClient(string host, int port, string userName)
      : this(new ImapClientProfile(host, port, userName))
    {
    }

    public ImapClient(string host, int port, bool securePort)
      : this(new ImapClientProfile(host, port, securePort))
    {
    }

    public ImapClient(string host, int port, bool securePort, string userName)
      : this(new ImapClientProfile(host, port, securePort, userName))
    {
    }

    public ImapClient(string host, int port, string userName, string authType)
      : this(new ImapClientProfile(host, port, userName, authType))
    {
    }

    public ImapClient(string host, int port, bool securePort, string userName, string authType)
      : this(new ImapClientProfile(host, port, securePort, userName, authType))
    {
    }

    public ImapClient(string host, int port, bool securePort, string userName, string authType, int timeout)
      : this(new ImapClientProfile(host, port, securePort, userName, authType, timeout))
    {
    }

    public ImapClient(ImapClientProfile profile)
    {
      if (profile == null)
        throw new ArgumentNullException("profile");

      this.profile = profile;
    }

    /*
     * connect impl
     */
    private delegate ImapSession ConnectProc(ConnectParams @params);

    private static ImapSession ConnectCore(ConnectParams @params)
    {
      ImapSession session = null;

      try {
        session = ImapSessionCreator.CreateSession(@params.Profile,
                                                   @params.AuthMechanism,
                                                   @params.CreateSslStreamCallback ?? ImapConnection.CreateSslStream);
      }
      finally {
        @params.Profile.SetCredentials(null);
      }

      // update server info
      if (session.ServerCapabilities.Has(ImapCapability.Namespace))
        session.Namespace();

      if (session.ServerCapabilities.Has(ImapCapability.ID))
        session.ID(null); // TODO: client ID

      session.UpdateSelectedMailboxSizeAndStatus = false;

      return session;
    }

    internal class ConnectParams {
      public ImapClientProfile Profile;
      public SaslClientMechanism AuthMechanism;
      public UpgradeConnectionStreamCallback CreateSslStreamCallback;

      public ConnectParams(ImapClientProfile profile,
                           ICredentialsByHost credentials,
                           UpgradeConnectionStreamCallback createSslStreamCallback)
      {
        Profile = profile.Clone();
        Profile.SetCredentials(credentials);

        CreateSslStreamCallback = createSslStreamCallback;
      }

      public ConnectParams(ImapClientProfile profile,
                           SaslClientMechanism authMechanism,
                           UpgradeConnectionStreamCallback createSslStreamCallback)
      {
        Profile = profile.Clone();
        AuthMechanism = authMechanism;
        CreateSslStreamCallback = createSslStreamCallback;
      }
    }

    public class ConnectAsyncResult : IAsyncResult {
      public object AsyncState {
        get { return asyncState; }
      }

      public WaitHandle AsyncWaitHandle {
        get { return innerAsyncResult.AsyncWaitHandle; }
      }

      public bool CompletedSynchronously {
        get { return innerAsyncResult.CompletedSynchronously; }
      }

      public bool IsCompleted {
        get { return innerAsyncResult.IsCompleted; }
      }

      public bool EndConnectCalled {
        get { return endConnectCalled; }
      }

      internal bool OwnerDisposed {
        get; set;
      }

      internal ConnectAsyncResult() {}

      internal void BeginConnect(ConnectParams @params,
                                 AsyncCallback asyncCallback,
                                 object asyncState)
      {
        var proc = (ConnectProc)ImapClient.ConnectCore;

        this.asyncCallback = asyncCallback;
        this.asyncState = asyncState;

        innerAsyncResult = (AsyncResult)proc.BeginInvoke(@params, Callback, null);
      }

      internal ImapSession EndConnect()
      {
        endConnectCalled = true;

        innerAsyncResult.AsyncWaitHandle.WaitOne();

        lock (asyncResultLockObject) {
          if (exception != null)
            throw exception;

          return createdSession;
        }
      }

      private void Callback(IAsyncResult asyncResult)
      {
        ImapSession ret = null;

        lock (asyncResultLockObject) {
          try {
            ret = ((ConnectProc)innerAsyncResult.AsyncDelegate).EndInvoke(innerAsyncResult);

            if (!OwnerDisposed)
              createdSession = ret;
          }
          catch (Exception ex) {
            exception = ex;
          }
        }

        if (asyncCallback != null)
          asyncCallback(this);

        if (OwnerDisposed && ret != null)
          ret.Disconnect(false); // ignore exceptions
      }

      private AsyncResult innerAsyncResult;
      private AsyncCallback asyncCallback;
      private object asyncState;
      private bool endConnectCalled;
      private object asyncResultLockObject = new object();
      private Exception exception;
      private ImapSession createdSession;
    }

    /*
     * BeginConnect()/EndConnect()
     */
    public IAsyncResult BeginConnect(string password)
    {
      return BeginConnect(password, null, null, null);
    }

    public IAsyncResult BeginConnect(string password,
                                     UpgradeConnectionStreamCallback createSslStreamCallback)
    {
      return BeginConnect(password, createSslStreamCallback, null, null);
    }

    public IAsyncResult BeginConnect(string password,
                                     AsyncCallback asyncCallback,
                                     object asyncState)
    {
      return BeginConnect(password, null, asyncCallback, asyncState);
    }

    public IAsyncResult BeginConnect(string password,
                                     UpgradeConnectionStreamCallback createSslStreamCallback,
                                     AsyncCallback asyncCallback,
                                     object asyncState)
    {
      return BeginConnect(new ConnectParams(profile,
                                            new NetworkCredential(profile.UserName, password),
                                            createSslStreamCallback),
                          asyncCallback,
                          asyncState);
    }

    public IAsyncResult BeginConnect(ICredentialsByHost credentials)
    {
      return BeginConnect(credentials, null, null, null);
    }

    public IAsyncResult BeginConnect(ICredentialsByHost credentials,
                                     UpgradeConnectionStreamCallback createSslStreamCallback)
    {
      return BeginConnect(credentials, createSslStreamCallback, null, null);
    }

    public IAsyncResult BeginConnect(ICredentialsByHost credentials,
                                     AsyncCallback asyncCallback,
                                     object asyncState)
    {
      return BeginConnect(credentials, null, asyncCallback, asyncState);
    }

    public IAsyncResult BeginConnect(ICredentialsByHost credentials,
                                     UpgradeConnectionStreamCallback createSslStreamCallback,
                                     AsyncCallback asyncCallback,
                                     object asyncState)
    {
      return BeginConnect(new ConnectParams(profile,
                                            credentials,
                                            createSslStreamCallback),
                          asyncCallback,
                          asyncState);
    }

    public IAsyncResult BeginConnect(SaslClientMechanism authMechanism)
    {
      return BeginConnect(authMechanism, null, null, null);
    }

    public IAsyncResult BeginConnect(SaslClientMechanism authMechanism,
                                     UpgradeConnectionStreamCallback createSslStreamCallback)
    {
      return BeginConnect(authMechanism, createSslStreamCallback, null, null);
    }

    public IAsyncResult BeginConnect(SaslClientMechanism authMechanism,
                                     AsyncCallback asyncCallback,
                                     object asyncState)
    {
      return BeginConnect(authMechanism, null, asyncCallback, asyncState);
    }

    public IAsyncResult BeginConnect(SaslClientMechanism authMechanism,
                                     UpgradeConnectionStreamCallback createSslStreamCallback,
                                     AsyncCallback asyncCallback,
                                     object asyncState)
    {
      return BeginConnect(new ConnectParams(profile,
                                            authMechanism,
                                            createSslStreamCallback),
                          asyncCallback,
                          asyncState);
    }

    private IAsyncResult BeginConnect(ConnectParams @params,
                                      AsyncCallback asyncCallback,
                                      object asyncState)
    {
      ThrowIfAlreadyConnectedOrAsyncConnectRunning();

      connectAsyncResult = new ConnectAsyncResult();
      connectAsyncResult.BeginConnect(@params, asyncCallback, asyncState);

      return connectAsyncResult;
    }

    public void EndConnect(IAsyncResult asyncResult)
    {
      if (asyncResult == null)
        throw new ArgumentNullException("asyncResult");
      if (asyncResult != connectAsyncResult)
        throw new ArgumentException("invalid IAsyncResult", "asyncResult");

      if (connectAsyncResult.EndConnectCalled)
        throw new InvalidOperationException("EndConnect already called");

      try {
        SetSession(connectAsyncResult.EndConnect());
      }
      finally {
        connectAsyncResult = null;
      }
    }

    /*
     * Connect()
     */
    public void Connect(string password)
    {
      ThrowIfAlreadyConnectedOrAsyncConnectRunning();

      SetSession(ConnectCore(new ConnectParams(profile,
                                               new NetworkCredential(profile.UserName, password),
                                               null)));
    }

    public void Connect(string password, UpgradeConnectionStreamCallback createSslStreamCallback)
    {
      ThrowIfAlreadyConnectedOrAsyncConnectRunning();

      SetSession(ConnectCore(new ConnectParams(profile,
                                               new NetworkCredential(profile.UserName, password),
                                               createSslStreamCallback)));
    }

    public void Connect(ICredentialsByHost credentials)
    {
      ThrowIfAlreadyConnectedOrAsyncConnectRunning();

      SetSession(ConnectCore(new ConnectParams(profile,
                                               credentials,
                                               null)));
    }

    public void Connect(ICredentialsByHost credentials, UpgradeConnectionStreamCallback createSslStreamCallback)
    {
      ThrowIfAlreadyConnectedOrAsyncConnectRunning();

      SetSession(ConnectCore(new ConnectParams(profile,
                                               credentials,
                                               createSslStreamCallback)));
    }

    public void Connect(SaslClientMechanism authMechanism)
    {
      if (authMechanism == null)
        throw new ArgumentNullException("authMechanism");

      ThrowIfAlreadyConnectedOrAsyncConnectRunning();

      SetSession(ConnectCore(new ConnectParams(profile,
                                               authMechanism,
                                               null)));
    }

    public void Connect(SaslClientMechanism authMechanism, UpgradeConnectionStreamCallback createSslStreamCallback)
    {
      if (authMechanism == null)
        throw new ArgumentNullException("authMechanism");

      ThrowIfAlreadyConnectedOrAsyncConnectRunning();

      SetSession(ConnectCore(new ConnectParams(profile,
                                               authMechanism,
                                               createSslStreamCallback)));
    }

    private void SetSession(ImapSession session)
    {
      this.session = session;
    }

    /*
     * destruction
     */
    void IDisposable.Dispose()
    {
      Disconnect(false, false);

      if (connectAsyncResult != null)
        connectAsyncResult.OwnerDisposed = true;
    }

    public void Disconnect()
    {
      Disconnect(false, true);
    }

    public void Logout()
    {
      Disconnect(true, true);
    }

    public void Disconnect(bool logout)
    {
      Disconnect(logout, true);
    }

    private void Disconnect(bool logout, bool checkAsyncConnectRunning)
    {
      if (checkAsyncConnectRunning)
        ThrowIfAsyncConnectRunning();

      if (session != null)
        session.Disconnect(logout);

      session = null;
    }

    /*
     * methods for checking and validating internal status
     */
    private void ThrowIfAsyncConnectRunning()
    {
      if (connectAsyncResult != null)
        throw new InvalidOperationException("BeginConnect running");
    }

    private void ThrowIfAlreadyConnectedOrAsyncConnectRunning()
    {
      if (IsConnected)
        throw new InvalidOperationException("already connected");
      if (connectAsyncResult != null)
        throw new InvalidOperationException("BeginConnect running");
    }

    private void ThrowIfNotConnected()
    {
      if (!IsConnected)
        throw new InvalidOperationException("not connected or already disconnected");
    }

    public override string ToString()
    {
      return string.Format("{{ImapClient: Authority='{0}', IsConnected={1}, OpenedMailbox={2}}}",
                           profile.Authority,
                           IsConnected,
                           openedMailbox);
    }

    private ImapSession session;
    private ImapClientProfile profile;
    private ConnectAsyncResult connectAsyncResult;
  }
}
