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
using System.Runtime.Remoting.Messaging;
using System.Threading;

using Smdn.Net.Pop3.Client.Session;
using Smdn.Net.Pop3.Protocol.Client;
using Smdn.Security.Authentication.Sasl.Client;

namespace Smdn.Net.Pop3.Client {
  public partial class PopClient : IDisposable {
    public PopClientProfile Profile {
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
      get { return (session != null && session.State != PopSessionState.NotConnected); }
    }

    internal PopSession Session {
      get { ThrowIfNotConnected(); return session; }
    }

    public bool IsSecureSession {
      get { return Session.IsSecureConnection; }
    }

    public PopCapabilityList ServerCapabilities {
      get { return Session.ServerCapabilities; }
    }

    /*
     * construction
     */
    public PopClient()
      : this(new PopClientProfile())
    {
    }

    public PopClient(Uri authority)
      : this(new PopClientProfile(authority))
    {
    }

    public PopClient(string host)
      : this(new PopClientProfile(host))
    {
    }

    public PopClient(string host, string userName)
      : this(new PopClientProfile(host, userName))
    {
    }

    public PopClient(string host, bool securePort, string userName)
      : this(new PopClientProfile(host, securePort, userName))
    {
    }

    public PopClient(string host, int port)
      : this(new PopClientProfile(host, port))
    {
    }

    public PopClient(string host, int port, string userName)
      : this(new PopClientProfile(host, port, userName))
    {
    }

    public PopClient(string host, int port, bool securePort)
      : this(new PopClientProfile(host, port, securePort))
    {
    }

    public PopClient(string host, int port, bool securePort, string userName)
      : this(new PopClientProfile(host, port, securePort, userName))
    {
    }

    public PopClient(string host, int port, string userName, string authType)
      : this(new PopClientProfile(host, port, userName, authType))
    {
    }

    public PopClient(string host, int port, bool securePort, string userName, string authType)
      : this(new PopClientProfile(host, port, securePort, userName, authType))
    {
    }

    public PopClient(string host, int port, bool securePort, string userName, string authType, int timeout)
      : this(new PopClientProfile(host, port, securePort, userName, authType, timeout))
    {
    }

    public PopClient(PopClientProfile profile)
    {
      if (profile == null)
        throw new ArgumentNullException("profile");

      this.profile = profile;
    }

    /*
     * connect impl
     */
    private delegate PopSession ConnectProc(ConnectParams @params);

    private static PopSession ConnectCore(ConnectParams @params)
    {
      PopSession session = null;

      try {
        session = PopSessionCreator.CreateSession(@params.Profile,
                                                  @params.AuthMechanism,
                                                  @params.CreateSslStreamCallback ?? PopConnection.CreateSslStream);
      }
      finally {
        @params.Profile.SetCredentials(null);
      }

      return session;
    }

    internal class ConnectParams {
      public PopClientProfile Profile;
      public SaslClientMechanism AuthMechanism;
      public UpgradeConnectionStreamCallback CreateSslStreamCallback;

      public ConnectParams(PopClientProfile profile,
                           ICredentialsByHost credentials,
                           UpgradeConnectionStreamCallback createSslStreamCallback)
      {
        Profile = profile.Clone();
        Profile.SetCredentials(credentials);

        CreateSslStreamCallback = createSslStreamCallback;
      }

      public ConnectParams(PopClientProfile profile,
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
        var proc = (ConnectProc)PopClient.ConnectCore;

        this.asyncCallback = asyncCallback;
        this.asyncState = asyncState;

        innerAsyncResult = (AsyncResult)proc.BeginInvoke(@params, Callback, null);
      }

      internal PopSession EndConnect()
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
        PopSession ret = null;

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
      private PopSession createdSession;
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
    //

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

    public void Connect(string password,
                        UpgradeConnectionStreamCallback createSslStreamCallback)
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

    public void Connect(ICredentialsByHost credentials,
                        UpgradeConnectionStreamCallback createSslStreamCallback)
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

    public void Connect(SaslClientMechanism authMechanism,
                        UpgradeConnectionStreamCallback createSslStreamCallback)
    {
      if (authMechanism == null)
        throw new ArgumentNullException("authMechanism");

      ThrowIfAlreadyConnectedOrAsyncConnectRunning();

      SetSession(ConnectCore(new ConnectParams(profile,
                                               authMechanism,
                                               createSslStreamCallback)));
    }

    private void SetSession(PopSession session)
    {
      this.session = session;

      lock (((System.Collections.ICollection)messages).SyncRoot) {
        messages.Clear();
      }
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

      lock (((System.Collections.ICollection)messages).SyncRoot) {
        foreach (var message in messages.Values) {
          message.Client = null;
        }

        messages.Clear();
      }
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
      return string.Format("{{PopClient: Authority='{0}', IsConnected={1}}}",
                           profile.Authority,
                           IsConnected);
    }

    private PopSession session;
    private PopClientProfile profile;
    private ConnectAsyncResult connectAsyncResult;
  }
}
