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
using System.Threading;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif
using Smdn.Net.Pop3.Protocol;
using Smdn.Net.Pop3.Protocol.Client;
using Smdn.Net.Pop3.Client.Transaction.BuiltIn;

namespace Smdn.Net.Pop3.Client.Session {
  public sealed partial class PopSession : IDisposable {
    /*
     * properties
     */
    internal int? Id {
      get { return (connection == null) ? (int?)null : connection.Id; }
    }

    public bool IsDisposed {
      get { return disposed; }
    }

    public bool HandlesIncapableAsException {
      get { return handlesIncapableAsException; }
      set { handlesIncapableAsException = value; }
    }

    public int SendTimeout {
      get { CheckDisposed(); return connection.SendTimeout; }
      set { CheckDisposed(); connection.SendTimeout = value; }
    }

    public int ReceiveTimeout {
      get { CheckDisposed(); return connection.ReceiveTimeout; }
      set { CheckDisposed();  connection.ReceiveTimeout = value; }
    }

    public bool IsSecureConnection {
      get { CheckDisposed(); return connection.IsSecurePortConnection || connection.IsSecureConnection; }
    }

    public int TransactionTimeout {
      get { CheckDisposed(); return transactionTimeout; }
      set
      {
        CheckDisposed();
        if (value < -1)
          throw new ArgumentOutOfRangeException("TransactionTimeout", value, "must be greater than or equals to -1");
        transactionTimeout = value;
      }
    }

    public bool IsTransactionProceeding {
      get
      {
        if (Monitor.TryEnter(transactionLockObject, 0)) {
          Monitor.Exit(transactionLockObject);
          return false;
        }
        else {
          return true;
        }
      }
    }

    /*
     * construction / destruction
     */
    public PopSession(string host)
      : this(host, PopDefaultPorts.Pop, DefaultTransactionTimeout, null)
    {
    }

    public PopSession(string host, int port)
      : this(host, port, DefaultTransactionTimeout, null)
    {
    }

    public PopSession(string host, int port, int transactionTimeout)
      : this(host, port, transactionTimeout, null)
    {
    }

    public PopSession(string host, int port, UpgradeConnectionStreamCallback createAuthenticatedStreamCallback)
      : this(host, port, DefaultTransactionTimeout, createAuthenticatedStreamCallback)
    {
    }

    public PopSession(string host, int port, int transactionTimeout, UpgradeConnectionStreamCallback createAuthenticatedStreamCallback)
    {
      if (transactionTimeout < -1)
        throw new ArgumentOutOfRangeException("transactionTimeout", transactionTimeout, "must be greater than or equals to -1");

      this.transactionTimeout = transactionTimeout;

      Connect(host, port, createAuthenticatedStreamCallback);
    }

    public override string ToString()
    {
      CheckDisposed();

      return string.Format("{{Connection={0}, State={0}}}",
                           connection,
                           state);
    }

    private const int DefaultTransactionTimeout = Timeout.Infinite;

    private bool disposed = false;
    private int transactionTimeout = DefaultTransactionTimeout;
    private bool handlesIncapableAsException = false;
    private object transactionLockObject = new object();

    /*
     * session state properties
     */
    public IConnectionInfo ConnectionInfo {
      get { CheckDisposed(); return connection; }
    }

    public PopSessionState State {
      get { CheckDisposed(); return state; }
    }

    public Uri Authority {
      get { CheckDisposed(); return authority == null ? null : authority.Uri; }
    }

    public PopCapabilityList ServerCapabilities {
      get { CheckDisposed(); return serverCapabilities; }
    }

    /// <summary>timestamp in server banner greeting</summary>
    public string Timestamp {
      get { CheckDisposed(); return timestamp; }
    }

    public bool ApopAvailable {
      get { return !string.IsNullOrEmpty(Timestamp); }
    }

    private PopConnection connection = null;
    private PopSessionState state = PopSessionState.NotConnected;
    private PopUriBuilder authority = new PopUriBuilder();
    private PopCapabilityList serverCapabilities = new PopCapabilityList(true, new PopCapability[0]);
    private string timestamp = null;

    /*
     * transaction methods : connect/disconnect
     */
    private void Connect(string host, int port, UpgradeConnectionStreamCallback createAuthenticatedStreamCallback)
    {
      TraceInfo("connecting");

      this.connection = new PopConnection(host, port, createAuthenticatedStreamCallback);

      TraceInfo("connected");

      using (var t = new GreetingTransaction(connection)) {
        try {
          if (ProcessTransaction(t).Failed)
            throw new PopConnectionException(string.Concat("connection refused or establishment failed: ", t.Result.ResultText));
        }
        catch (TimeoutException ex) {
          throw new PopConnectionException("connection timed out", ex);
        }

        timestamp = t.Result.Value;

        TransitStateTo(PopSessionState.Authorization);
      }

      if (ApopAvailable)
        TraceInfo("APOP is available, timestamp is {0}", timestamp);
    }

    /// <summary>disconnects session</summary>
    /// <remarks>this method sends CLOSE and LOGOUT before disconnect the session.</remarks>
    public void Disconnect()
    {
      Disconnect(true);
    }

    /// <summary>disconnects session</summary>
    public void Disconnect(bool logout)
    {
      CheckDisposed();

      if (logout) {
        if (state == PopSessionState.Transaction || state == PopSessionState.Authorization)
          Quit();
      }

      (this as IDisposable).Dispose();
    }

    private void CloseConnection()
    {
      if (connection == null)
        return;

      try {
        TraceInfo("disconnecting");

        connection.Close();

        TraceInfo("disconnected");
      }
      finally {
        TransitStateTo(PopSessionState.NotConnected);

        connection = null;
      }
    }

    void IDisposable.Dispose()
    {
      if (disposed)
        return;

      CloseConnection();

      disposed = true;
    }

    /*
     * transaction methods : any state
     */

    /// <summary>sends QUIT command</summary>
    /// <remarks>valid in any state</remarks>
    public PopCommandResult Quit()
    {
      CheckDisposed();

      switch (state) {
        case PopSessionState.NotConnected:
          return new PopCommandResult(PopCommandResultCode.RequestDone,
                                      "already logged out or disconnected");

        case PopSessionState.Transaction:
          TransitStateTo(PopSessionState.Update);
          break;
      }

      using (var t = new QuitTransaction(connection)) {
        try {
          return ProcessTransaction(t);
        }
        finally {
          /*
           * 6. The UPDATE State
           *    QUIT
           *       Whether the removal was successful or not, the server
           *       then releases any exclusive-access lock on the maildrop
           *       and closes the TCP connection.
           */
          CloseConnection();
        }
      }
    }

    /// <summary>sends CAPA command</summary>
    /// <remarks>valid in any state</remarks>
    public PopCommandResult Capa()
    {
      PopCapabilityList discard;

      return Capa(out discard);
    }

    /// <summary>sends CAPA command</summary>
    /// <remarks>valid in any state</remarks>
    public PopCommandResult Capa(out PopCapabilityList capabilities)
    {
      RejectNonConnectedState();

      capabilities = null;

      using (var t = new CapaTransaction(connection)) {
        if (ProcessTransaction(t).Succeeded) {
          SetServerCapabilities(t.Result.Value);
          capabilities = t.Result.Value;
        }

        return t.Result;
      }
    }

    /*
     * transaction methods : generic use, exntension, etc.
     */

    /// <summary>sends generic/undefined command</summary>
    public PopCommandResult GenericCommand(string command, params string[] arguments)
    {
      return GenericCommand(command, false, arguments);
    }

    /// <summary>sends generic/undefined command</summary>
    public PopCommandResult GenericCommand(string command, bool isResponseMultiline, params string[] arguments)
    {
      PopResponse[] discard;

      return GenericCommand(command, isResponseMultiline, out discard, arguments);
    }

    /// <summary>sends generic/undefined command</summary>
    public PopCommandResult GenericCommand(string command, out PopResponse[] responses, params string[] arguments)
    {
      return GenericCommand(command, false, out responses, arguments);
    }

    /// <summary>sends generic/undefined command</summary>
    public PopCommandResult GenericCommand(string command, bool isResponseMultiline, out PopResponse[] responses, params string[] arguments)
    {
      RejectNonConnectedState();

      responses = null;

      using (var t = new GenericCommandTransaction(connection, command, isResponseMultiline)) {
        if (arguments != null && 0 < arguments.Length)
          t.RequestArguments["arguments"] = string.Join(" ", arguments);

        if (ProcessTransaction(t).Succeeded)
          responses = t.Result.ReceivedResponses.ToArray();

        return t.Result;
      }
    }

    /*
     * methods for internal state management
     */
    internal void SetServerCapabilities(PopCapabilityList newCapabilities)
    {
      serverCapabilities = new PopCapabilityList(true, newCapabilities);
    }

    private void TransitStateTo(PopSessionState newState)
    {
      if (newState == state)
        return;

      switch (newState) {
        case PopSessionState.NotConnected:
          authority = null;
          if (state == PopSessionState.Update || state == PopSessionState.Transaction)
            TraceInfo("logged out");
          TraceInfo("now in non-connected state");
          break;

        case PopSessionState.Authorization:
          UpdateAuthority(null, null);
          TraceInfo("now in authorization state");
          break;

        case PopSessionState.Transaction:
          TraceInfo("now in transaction state");
          if (state == PopSessionState.Authorization)
            TraceInfo("logged in");
          break;

        case PopSessionState.Update:
          TraceInfo("now in update state");
          break;
      }

      state = newState;
    }

    /*
     * methods for checking and validating session status
     */
    private void CheckDisposed()
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private void RejectNonConnectedState()
    {
      CheckDisposed();

      if (state == PopSessionState.NotConnected)
        throw new PopProtocolViolationException("session has been disconnected");
    }

    private void RejectNonAuthorizationState()
    {
      RejectNonConnectedState();

      if (state != PopSessionState.Authorization)
        throw new PopProtocolViolationException("session is not in authorization state");
    }

    private void RejectNonTransactionState()
    {
      RejectNonConnectedState();

      if (state != PopSessionState.Transaction)
        throw new PopProtocolViolationException("session is not in transaction state");
    }

    private void CheckServerCapability(IPopExtension extension)
    {
      if (extension == null)
        return;

      if (!serverCapabilities.IsCapable(extension))
        throw new PopIncapableException(extension.RequiredCapability);
    }

    /*
    private void CheckServerCapability(PopCapability capability)
    {
      if (capability != null && !serverCapabilities.IsCapable(capability))
        throw new PopIncapableException(capability);
    }
    */

    /*
     * tracing
     */
    [System.Diagnostics.Conditional("TRACE")]
    private void TraceInfo(string format, params object[] arguments)
    {
      Trace.Log(this, format, arguments);
    }
  }
}