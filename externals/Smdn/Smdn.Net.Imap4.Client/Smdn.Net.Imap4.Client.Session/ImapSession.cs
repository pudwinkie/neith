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
using System.Threading;
#if NET_3_5
using System.Linq;
#endif

using Smdn.Collections;
using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;
using Smdn.Net.Imap4.Client.Transaction;
using Smdn.Net.Imap4.Client.Transaction.BuiltIn;

namespace Smdn.Net.Imap4.Client.Session {
  // Layers:
  // * Session.ImapSession
  //   Transaction.IImapTransaction
  //   Protocol.ImapConnection/Protocol.ImapCommand/Protocol.ImapResponse
  //   (IMAP4)

  public sealed partial class ImapSession : IDisposable {
    /*
     * properties
     */
    internal int? Id {
      get { return (connection == null) ? (int?)null : connection.Id; }
    }

    public bool IsDisposed {
      get { return disposed; }
    }

    public bool HandlesReferralAsException {
      get { return handlesReferralAsException; }
      set { handlesReferralAsException = value; }
    }

    public bool HandlesIncapableAsException {
      get { return handlesIncapableAsException; }
      set { handlesIncapableAsException = value; }
    }

    public int SendTimeout {
      get { CheckDisposed(); return connection.SendTimeout; }
      set { CheckDisposed(); RejectTransactionProceeding(); connection.SendTimeout = value; }
    }

    public int ReceiveTimeout {
      get { CheckDisposed(); return connection.ReceiveTimeout; }
      set { CheckDisposed(); RejectTransactionProceeding(); connection.ReceiveTimeout = value; }
    }

    public bool IsSecureConnection {
      get { CheckDisposed(); return connection.IsSecurePortConnection || connection.IsSecureConnection; }
    }

    public int TransactionTimeout {
      get { CheckDisposed(); return transactionTimeout; }
      set
      {
        CheckDisposed();
        RejectTransactionProceeding();

        if (value < -1)
          throw ExceptionUtils.CreateArgumentMustBeGreaterThanOrEqualTo(-1, "TransactionTimeout", value);
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
    public ImapSession(string host)
      : this(host,
             ImapDefaultPorts.Imap,
             DefaultTransactionTimeout,
             DefaultSendTimeout,
             DefaultReceiveTimeout,
             false,
             null)
    {
    }

    public ImapSession(string host,
                       int port)
      : this(host,
             port,
             DefaultTransactionTimeout,
             DefaultSendTimeout,
             DefaultReceiveTimeout,
             false,
             null)
    {
    }

    public ImapSession(string host,
                       int port,
                       int transactionTimeout)
      : this(host,
             port,
             transactionTimeout,
             DefaultSendTimeout,
             DefaultReceiveTimeout,
             false,
             null)
    {
    }

    public ImapSession(string host,
                       int port,
                       UpgradeConnectionStreamCallback createAuthenticatedStreamCallback)
      : this(host,
             port,
             DefaultTransactionTimeout,
             DefaultSendTimeout,
             DefaultReceiveTimeout,
             false,
             createAuthenticatedStreamCallback)
    {
    }

    public ImapSession(string host,
                       int port,
                       int transactionTimeout,
                       UpgradeConnectionStreamCallback createAuthenticatedStreamCallback)
      : this(host,
             port,
             transactionTimeout,
             DefaultSendTimeout,
             DefaultReceiveTimeout,
             false,
             createAuthenticatedStreamCallback)
    {
    }

    public ImapSession(string host,
                       int port,
                       bool handlesReferralAsException)
      : this(host,
             port,
             DefaultTransactionTimeout,
             DefaultSendTimeout,
             DefaultReceiveTimeout,
             handlesReferralAsException,
             null)
    {
    }

    public ImapSession(string host,
                       int port,
                       bool handlesReferralAsException,
                       UpgradeConnectionStreamCallback createAuthenticatedStreamCallback)
      : this(host,
             port,
             DefaultTransactionTimeout,
             DefaultSendTimeout,
             DefaultReceiveTimeout,
             handlesReferralAsException,
             createAuthenticatedStreamCallback)
    {
    }

    public ImapSession(string host,
                       int port,
                       int transactionTimeout,
                       int sendTimeout,
                       int receiveTimeout,
                       UpgradeConnectionStreamCallback createAuthenticatedStreamCallback)
      : this(host,
             port,
             transactionTimeout,
             sendTimeout,
             receiveTimeout,
             false,
             createAuthenticatedStreamCallback)
    {
    }

    public ImapSession(string host,
                       int port,
                       int transactionTimeout,
                       int sendTimeout,
                       int receiveTimeout,
                       bool handlesReferralAsException,
                       UpgradeConnectionStreamCallback createAuthenticatedStreamCallback)
    {
      if (transactionTimeout < -1)
        throw ExceptionUtils.CreateArgumentMustBeGreaterThanOrEqualTo(-1, "transactionTimeout", transactionTimeout);

      this.handlesReferralAsException = handlesReferralAsException;
      this.transactionTimeout = transactionTimeout;

      this.readonlyHierarchyDelimiters = hierarchyDelimiters.AsReadOnly();

      Connect(host,
              port,
              transactionTimeout,
              sendTimeout,
              receiveTimeout,
              createAuthenticatedStreamCallback);
    }

    public override string ToString()
    {
      CheckDisposed();

      return string.Format("{{Connection={0}, State={0}}}",
                           connection,
                           state);
    }

    private const int DefaultTransactionTimeout = Timeout.Infinite;
    private const int DefaultSendTimeout = Timeout.Infinite;
    private const int DefaultReceiveTimeout = Timeout.Infinite;

    private bool disposed = false;
    private bool handlesReferralAsException;
    private bool handlesIncapableAsException = false;
    private int transactionTimeout = DefaultTransactionTimeout;
    private object transactionLockObject = new object();

    /*
     * session state properties
     */
    public IConnectionInfo ConnectionInfo {
      get { CheckDisposed(); return connection; }
    }

    public bool IsIdling {
      get { CheckDisposed(); return connection.IsIdling; }
    }

    public ImapSessionState State {
      get { CheckDisposed(); return state; }
    }

    public ImapMailbox SelectedMailbox {
      get { CheckDisposed(); return selectedMailbox; }
    }

    internal bool UpdateSelectedMailboxSizeAndStatus {
      get { return updateSelectedMailboxSizeAndStatus; }
      set { updateSelectedMailboxSizeAndStatus = value; }
    }

    public Uri Authority {
      get { CheckDisposed(); return authority == null ? null : authority.Uri; }
    }

    internal ImapUriBuilder AuthorityBuilder {
      get { return authority; }
    }

    public ImapCapabilitySet ServerCapabilities {
      get { CheckDisposed(); return serverCapabilities; }
    }

    public IDictionary<string, string> HierarchyDelimiters {
      get { CheckDisposed(); return readonlyHierarchyDelimiters; }
    }

    public ImapNamespace Namespaces {
      get { CheckDisposed(); return namespaces; }
    }

    public IDictionary<string, string> ServerID {
      get { CheckDisposed(); return serverID; }
    }

    public string SelectedLanguage {
      get { CheckDisposed(); return selectedLanguage; }
    }

    public ImapCollationAlgorithm ActiveComparator {
      get { CheckDisposed(); return activeComparator; }
    }

    private ImapConnection connection = null;
    private ImapMailboxManager mailboxManager = null;
    private ImapSessionState state = ImapSessionState.NotConnected;
    private ImapCommandResult lastTransactionResult = null;
    private ImapMailbox selectedMailbox = null;
    private bool updateSelectedMailboxSizeAndStatus = true;
    private ImapUriBuilder authority = new ImapUriBuilder();
    private ImapCapabilitySet serverCapabilities;
    private Dictionary<string, string> hierarchyDelimiters = new Dictionary<string, string>(StringComparer.Ordinal);
    private ImapNamespace namespaces = new ImapNamespace();
    private IDictionary<string, string> serverID = (new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)).AsReadOnly();
    private string selectedLanguage = null;
    private ImapCollationAlgorithm activeComparator = ImapCollationAlgorithm.Default;

    private IDictionary<string, string> readonlyHierarchyDelimiters;

    /*
     * transaction methods : connect/disconnect
     */
    private void Connect(string host,
                         int port,
                         int connectTimeout,
                         int sendTimeout,
                         int receiveTimeout,
                         UpgradeConnectionStreamCallback createAuthenticatedStreamCallback)
    {
      TraceInfo("connecting");

      this.connection = new ImapConnection(host,
                                           port,
                                           connectTimeout,
                                           createAuthenticatedStreamCallback);

      this.connection.SendTimeout = sendTimeout;
      this.connection.ReceiveTimeout = receiveTimeout;

      TraceInfo("connected");

      SetServerCapabilities(new ImapCapabilitySet(new[] {
        ImapCapability.Imap4Rev1 /* required by GreetingTransaction */
      }));

      ImapCommandResult result;

      using (var t = new GreetingTransaction(connection)) {
        try {
          result = ProcessTransaction(t);

          if (result.Code == ImapCommandResultCode.Bye) {
            var refferalResponseCode = result.GetResponseCode(ImapResponseCode.Referral);

            if (refferalResponseCode != null) {
              // RFC 2221 IMAP4 Login Referrals
              // http://tools.ietf.org/html/rfc2221
              // 4.2. BYE at connection startup referral
              //    An IMAP4 server MAY respond with an untagged BYE and a REFERRAL
              //    response code that contains an IMAP URL to a home server if it is not
              //    willing to accept connections and wishes to direct the client to
              //    another IMAP4 server.
              var referToUri = ImapResponseTextConverter.FromReferral(refferalResponseCode.ResponseText)[0];

              if (handlesReferralAsException) {
                throw new ImapLoginReferralException(string.Format("try another server: '{0}'", refferalResponseCode.ResponseText.Text),
                                                     referToUri);
              }
              else {
                TraceInfo("login referral: '{0}'", refferalResponseCode.ResponseText.Text);
                TraceInfo(string.Concat("  try to connect to ", referToUri));
              }
            }
          }

          if (result.Failed)
            throw new ImapConnectionException(string.Concat("connection refused or establishment failed: ", result.ResultText));
        }
        catch (TimeoutException ex) {
          throw new ImapConnectionException("connection timed out", ex);
        }

        if (result.Code == ImapCommandResultCode.PreAuth) {
          UpdateAuthority(null, null);
          TransitStateTo(ImapSessionState.Authenticated);
        }
        else {
          TransitStateTo(ImapSessionState.NotAuthenticated);
        }

        // 7.1. Server Responses - Status Responses
        //       CAPABILITY
        //          Followed by a list of capabilities.  This can appear in the
        //          initial OK or PREAUTH response to transmit an initial
        //          capabilities list.  This makes it unnecessary for a client to
        //          send a separate CAPABILITY command if it recognizes this
        //          response.
        var capabilityResponseCode = result.GetResponseCode(ImapResponseCode.Capability);

        if (capabilityResponseCode == null)
          // clear server capabilities
          SetServerCapabilities(null);
        else
          SetServerCapabilities(ImapResponseTextConverter.FromCapability(capabilityResponseCode.ResponseText));
      }
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
        if (state == ImapSessionState.Selected)
          Close();
        if (state == ImapSessionState.Authenticated)
          Logout();
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
        TransitStateTo(ImapSessionState.NotConnected);

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
     * transaction methods : generic use, exntension, etc.
     */

    /// <summary>sends generic/undefined command</summary>
    public ImapCommandResult GenericCommand(string command, params ImapString[] arguments)
    {
      ImapDataResponse[] discard;

      return GenericCommand(command, out discard, arguments);
    }

    /// <summary>sends generic/undefined command</summary>
    public ImapCommandResult GenericCommand(string command, out ImapDataResponse[] dataResponses, params ImapString[] arguments)
    {
      RejectNonConnectedState();

      if (command == null)
        throw new ArgumentNullException("command");
      if (command.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("command");

      dataResponses = null;

      using (var t = new GenericCommandTransaction(connection, command)) {
        if (arguments != null && 0 < arguments.Length)
          t.RequestArguments["arguments"] = new ImapStringList(arguments);

        if (ProcessTransaction(t).Succeeded)
          dataResponses = t.Result.Value;

        return t.Result;
      }
    }

    /*
     * methods for internal state management
     */
    internal void SetServerCapabilities(ImapCapabilitySet newCapabilities)
    {
      if (newCapabilities == null)
        serverCapabilities = ImapCapabilitySet.CreateReadOnlyEmpty();
      else
        serverCapabilities = new ImapCapabilitySet(true, newCapabilities);

#if false
      defaultLiteralSynchronizationMode = serverCapabilities.Has(ImapCapability.LiteralNonSync)
        ? ImapLiteralOptions.NonSynchronizing
        : ImapLiteralOptions.Synchronizing;
#endif
    }

    private void TransitStateTo(ImapSessionState newState)
    {
      if (newState == state)
        return;

      switch (newState) {
        case ImapSessionState.NotConnected:
          authority = null;
          selectedMailbox = null;
          if (mailboxManager != null) {
            mailboxManager.DetachFromSession();
            mailboxManager = null;
          }
          TraceInfo("now in non-connected state");
          break;

        case ImapSessionState.NotAuthenticated:
          UpdateAuthority(null, null);
          if (state == ImapSessionState.Selected || state == ImapSessionState.Authenticated) {
            mailboxManager.DetachFromSession();
            mailboxManager = null;
            TraceInfo("logged out");
          }
          TraceInfo("now in non-authenticated state");
          break;

        case ImapSessionState.Authenticated:
          TraceInfo("now in authenticated state");
          if (state == ImapSessionState.NotAuthenticated || state == ImapSessionState.NotConnected) {
            mailboxManager = new ImapMailboxManager(this);
            TraceInfo("logged in");
          }
          break;

        case ImapSessionState.Selected:
          TraceInfo("now in selected state (selected '{0}')", selectedMailbox.Name);
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

      if (state == ImapSessionState.NotConnected)
        throw new ImapProtocolViolationException("session has been disconnected");
    }

    private void RejectNonAuthenticatedState()
    {
      RejectNonConnectedState();

      if (state == ImapSessionState.NotAuthenticated)
        throw new ImapProtocolViolationException("session is not in authenticated state");
    }

    private void RejectNonSelectedState()
    {
      RejectNonConnectedState();

      if (state != ImapSessionState.Selected)
        throw new ImapProtocolViolationException("session is not in selected state");
    }

    private void RejectIdling()
    {
      if (IsIdling)
        throw new ImapProtocolViolationException("session is idling now");
    }

    private void ValidateMailboxRelationship(ImapMailbox mailbox)
    {
      if (mailbox == null)
        throw new ArgumentNullException("mailbox");

      if (mailbox.Session != this)
        throw new ArgumentException("mailbox is not attached to current session", "mailbox");
    }

    private void CheckServerCapability(IImapExtension extension)
    {
      if (extension == null)
        return;

      if (!serverCapabilities.IsCapable(extension))
        throw new ImapIncapableException(extension.RequiredCapabilities.ToArray());
    }

    private void CheckServerCapability(ImapCapability capability)
    {
      if (capability != null && !serverCapabilities.Contains(capability))
        throw new ImapIncapableException(capability);
    }

    private void RejectTransactionProceeding()
    {
      if (IsTransactionProceeding)
        throw new InvalidOperationException("transaction proceeding");
    }

    /*
     * tracing
     */
    [System.Diagnostics.Conditional("TRACE")]
    private void TraceInfo(string message)
    {
      Trace.Log(this, message);
    }

    [System.Diagnostics.Conditional("TRACE")]
    private void TraceInfo(string format, params object[] arguments)
    {
      Trace.Log(this, format, arguments);
    }
  }
}
