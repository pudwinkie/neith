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

using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Server;
using Smdn.Net.Imap4.Server.Transaction;
using Smdn.Net.Imap4.Server.Transaction.BuiltIn;

namespace Smdn.Net.Imap4.Server.Session {
  public class ImapSession : IDisposable {
    /*
     * properties
     */
    internal int Id {
      get { return connection.Id; }
    }

    public int SendTimeout {
      get
      {
        CheckDisposed();
        return connection.Socket.SendTimeout;
      }
      set
      {
        CheckDisposed();
        connection.Socket.SendTimeout = value;
      }
    }

    public int ReceiveTimeout {
      get
      {
        CheckDisposed();
        return connection.Socket.SendTimeout;
      }
      set
      {
        CheckDisposed();
        connection.Socket.SendTimeout = value;
      }
    }

    public int TransactionTimeout {
      get
      {
        CheckDisposed();
        return transactionTimeout;
      }
      set
      {
        CheckDisposed();
        if (value < -1)
          throw ExceptionUtils.CreateArgumentMustBeGreaterThanOrEqualTo(-1, "TransactionTimeout", value);
        transactionTimeout = value;
      }
    }

    public bool IsTransactionProceeding {
      get
      {
        if (!transactionProceedingSemaphore.WaitOne(0)) {
          return true;
        }
        else {
          transactionProceedingSemaphore.Release();
          return false;
        }
      }
    }

    public ImapSession()
    {
      commandProcMap = new Dictionary<ImapString, RespondProc>() {
        /* any state */
        {"CAPABILITY",    RespondCapability},
        {"NOOP",          RespondNoOp},
        {"LOGOUT",        RespondLogout},
        /* non authenticated state */
        {"STARTTLS",      null},
        {"AUTHENTICATE",  null},
        {"LOGIN",         RespondLogin},
        /* authenticated state */
        {"SELECT",        RespondSelect},
        {"EXAMINE",       RespondExamine},
        {"CREATE",        null},
        {"DELETE",        null},
        {"RENAME",        null},
        {"SUBSCRIBE",     null},
        {"UNSUBSCRIBE",   null},
        {"LIST",          RespondList},
        {"LSUB",          RespondLsub},
        {"STATUS",        RespondStatus},
        {"APPEND",        null},
        /* selected state */
        {"CHECK",         null},
        {"CLOSE",         RespondClose},
        {"EXPUNGE",       null},
        {"SEARCH",        null},
        {"FETCH",         null},
        {"STORE",         null},
        {"COPY",          null},
      };
    }

    internal void StartSession(IImapServer server, ImapConnection connection)
    {
      this.server = server;
      this.connection = connection;

      //EnqueueTransaction(new GreetingTransaction(connection));
      RespondGreeting();

      transactionThread = new Thread(TransactionThreadProc);
      transactionThread.IsBackground = true;
      transactionThread.Start();
    }

    public override string ToString()
    {
      CheckDisposed();

      return string.Format("{{Connection={0}, State={0}}}",
                           connection,
                           state);
    }

    private const int DefaultTransactionTimeout = Timeout.Infinite;

    private int transactionTimeout = DefaultTransactionTimeout;
    private Semaphore transactionProceedingSemaphore = new Semaphore(1, 1);

    /*
     * session state properties
     */
    internal IImapServer Server {
      get { return server; }
    }

    internal ImapConnection Connection {
      get { return connection; }
    }

    public bool IsIdling {
      get
      {
        CheckDisposed();
        return connection.IsIdling;
      }
    }

    public ImapSessionState State {
      get
      {
        CheckDisposed();
        return state;
      }
    }

    protected ImapMailbox SelectedMailbox {
      get
      {
        CheckDisposed();
        return selectedMailbox;
      }
    }

    private IImapServer server = null;
    private ImapConnection connection = null;
    private ImapSessionState state = ImapSessionState.NotConnected;
    private ImapMailbox selectedMailbox = null;

    /*
     * thread for responding
     */
    private void EnqueueTransaction(IImapTransaction t)
    {
      lock ((transactionQueue as System.Collections.ICollection).SyncRoot) {
        transactionQueue.Enqueue(t);
      }
    }

    private void TransactionThreadProc()
    {
      try {
        TraceVerbose("transaction thread started");

        for (;;) {
          /*
          IImapTransaction transaction = null;

          lock ((transactionQueue as System.Collections.ICollection).SyncRoot) {
            if (0 < transactionQueue.Count)
              transaction = transactionQueue.Dequeue();
          }

          if (transaction != null) {
            try {
              ProcessTransaction(transaction);
            }
            catch (ImapConnectionException ex) {
              Trace.Log(ex);
              if (ex.InnerException == null) {
                Trace.Log(connection, "client closed?");
                return;
              }
            }

            if (transaction is DisconnectTransaction || transaction is LogoutTransaction) {
              CloseConnection();
              return;
            }

            continue;
          }
          */

          try {
            var command = ReceiveCommand();
            RespondProc respondCommandProc = null;

            if (commandProcMap.TryGetValue(command.Command, out respondCommandProc) && respondCommandProc != null)
              respondCommandProc(command);
            else
              RespondUnknownCommand(command);

            if (state == ImapSessionState.NotConnected)
              return;
            /*
            Type commandTransactionType = null;

            if (commandTransactionMap.TryGetValue(command.Command, out commandTransactionType) && commandTransactionType != null)
              EnqueueTransaction((IImapTransaction)Activator.CreateInstance(commandTransactionMap[command.Command], connection, command));
            else
              EnqueueTransaction(new InvalidCommandTransaction(connection, command, "invalid or incapable command"));
            */
          }
          catch (ImapConnectionException ex) {
            Trace.Log(ex);
            if (ex.InnerException == null) {
              Trace.Log(connection, "client closed?");
              return;
            }
          }
          catch (ImapMalformedCommandException ex) {
            Trace.Log(ex);

            RespondInvalidCommand();
            //EnqueueTransaction(new InvalidCommandTransaction(connection, null, ex.Message));
          }
        }
      }
      finally {
        TraceVerbose("transaction thread finished");
      }
    }

    private ImapCommand ReceiveCommand()
    {
      for (;;) {
        var resp = connection.TryReceiveCommand();

        if (resp == null)
          continue;
        else
          return resp;
      }
    }

    private void ProcessTransaction(IImapTransaction t)
    {
      try {
        transactionProceedingSemaphore.WaitOne();

        t.Start();

        for (;;) {
          if (t.State == ImapTransactionState.Finished) {
            //Trace.LogResponse(t);

            if (t is IDisposable)
              (t as IDisposable).Dispose();

            return;
          }

          t.Process();
        }
      }
      finally {
        transactionProceedingSemaphore.Release();
      }
    }

    private /*readonly*/ Queue<IImapTransaction> transactionQueue = new Queue<IImapTransaction>();
    private Thread transactionThread;
    private delegate void RespondProc(ImapCommand command);
    private Dictionary<ImapString, RespondProc> commandProcMap;
    private Dictionary<ImapString, Type> commandTransactionMap = new Dictionary<ImapString, Type>() {
      /* any state */
      {"CAPABILITY",    typeof(CapabilityTransaction)},
      {"NOOP",          typeof(NoOpTransaction)},
      {"LOGOUT",        typeof(LogoutTransaction)},
      /* non authenticated state */
      {"STARTTLS",      null},
      {"AUTHENTICATE",  null},
      {"LOGIN",         null},
      /* authenticated state */
      {"SELECT",        null},
      {"EXAMINE",       null},
      {"CREATE",        null},
      {"DELETE",        null},
      {"RENAME",        null},
      {"SUBSCRIBE",     null},
      {"UNSUBSCRIBE",   null},
      {"LIST",          null},
      {"LSUB",          null},
      {"STATUS",        null},
      {"APPEND",        null},
      /* selected state */
      {"CHECK",         null},
      {"CLOSE",         null},
      {"EXPUNGE",       null},
      {"SEARCH",        null},
      {"FETCH",         null},
      {"STORE",         null},
      {"COPY",          null},
    };

    /*
     * transaction methods : connect/disconnect
     */
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
        //EnqueueTransaction(new DisconnectTransaction(connection));
        connection.SendResponse(new ImapUntaggedStatusResponse(ImapResponseCondition.Bye, "disconnected by server"));

        CloseConnection();

        transactionThread.Join(transactionTimeout);
      }

      (this as IDisposable).Dispose();
    }

    private void CloseConnection()
    {
      if (connection == null)
        return;

      try {
        Trace.Info("CID:{0} session closing", this.Id);

        connection.Close();

        Trace.Info("CID:{0} session closed", this.Id);
      }
      finally {
        TransitStateTo(ImapSessionState.NotConnected);

        connection = null;
      }
    }

    void IDisposable.Dispose()
    {
      if (connection == null)
        return;

      CloseConnection();

      transactionProceedingSemaphore.Close();
      transactionProceedingSemaphore = null;
    }

    protected virtual void RespondGreeting()
    {
      connection.SendResponse(new ImapUntaggedStatusResponse(ImapResponseCondition.Ok, string.Format("{0} ready.", server.Name)));

      TransitStateTo(ImapSessionState.NotAuthenticated);
    }

    /*
     * transaction methods : any state
     */
    protected virtual void RespondNoOp(ImapCommand command)
    {
      SendStatusResponse(command, ImapResponseCondition.Ok);
    }

    protected virtual void RespondCapability(ImapCommand command)
    {
      connection.SendResponse(new ImapDataResponse("CAPABILITY", ImapCapability.Imap4Rev1));

      SendStatusResponse(command, ImapResponseCondition.Ok);
    }

    protected virtual void RespondLogout(ImapCommand command)
    {
      connection.SendResponse(new ImapUntaggedStatusResponse(ImapResponseCondition.Bye, "see you"));

      TransitStateTo(ImapSessionState.NotConnected);

      SendStatusResponse(command, ImapResponseCondition.Ok);

      CloseConnection();
    }

    protected virtual void RespondUnknownCommand(ImapCommand command)
    {
      SendStatusResponse(command, ImapResponseCondition.Bad, "unknown command");
    }

    protected virtual void RespondInvalidCommand()
    {
      connection.SendResponse(new ImapUntaggedStatusResponse(ImapResponseCondition.Bad, "invalid command"));
    }

    /*
     * transaction methods : non-authenticated state
     */
    protected virtual void RespondStartTls(ImapCommand command)
    {
    }

    protected virtual void RespondAuthenticate(ImapCommand command)
    {
    }

    protected virtual void RespondLogin(ImapCommand command)
    {
      // TODO: check args
      TransitStateTo(ImapSessionState.Authenticated);

      SendStatusResponse(command, ImapResponseCondition.Ok);
    }

    /*
     * transaction methods : authenticated state
     */
    private void RespondSelect(ImapCommand command)
    {
      RespondSelectExamineCore(command, false);
    }

    private void RespondExamine(ImapCommand command)
    {
      RespondSelectExamineCore(command, true);
    }

    protected virtual void RespondSelectExamineCore(ImapCommand command, bool asReadOnly)
    {
      if (selectedMailbox != null)
        server.CloseMailbox(selectedMailbox);

      selectedMailbox = server.OpenMailbox(command.Arguments[0].Text.ToString(), asReadOnly);

      if (selectedMailbox == null) {
        SendStatusResponse(command, ImapResponseCondition.No, "non existent");
      }
      else {
        // required untagged responses
        var applicableFlags = new ImapParenthesizedString(ImapMessageFlag.Answered, ImapMessageFlag.Flagged, ImapMessageFlag.Deleted, ImapMessageFlag.Seen, ImapMessageFlag.Draft);
        var existsMessage = 0L;
        var recentMessage = 0L;

        connection.SendResponse(new ImapDataResponse("FLAGS", applicableFlags));
        connection.SendResponse(new ImapDataResponse(existsMessage.ToString(), "EXISTS"));
        connection.SendResponse(new ImapDataResponse(recentMessage.ToString(), "RESENT"));

        // required OK untagged responses
        var firstUnseen = 0L;
        var permanentFlags = new ImapParenthesizedString(ImapMessageFlag.Deleted, ImapMessageFlag.Seen, ImapMessageFlag.AllowedCreateKeywords);
        var uidNext = 1L;
        var uidValidity = 0L;

        connection.SendResponse(new ImapUntaggedStatusResponse(ImapResponseCondition.Ok, new ImapStringList("UNSEEN"), "first unseen"));
        connection.SendResponse(new ImapUntaggedStatusResponse(ImapResponseCondition.Ok, new ImapStringList("PERMANENTFLAGS", permanentFlags), "limited"));
        connection.SendResponse(new ImapUntaggedStatusResponse(ImapResponseCondition.Ok, new ImapStringList("UIDNEXT", uidNext.ToString()), "next UID"));
        connection.SendResponse(new ImapUntaggedStatusResponse(ImapResponseCondition.Ok, new ImapStringList("UIDVALIDITY", uidValidity.ToString()), "UIDs valid"));

        SendStatusResponse(command, ImapResponseCondition.Ok);

        TransitStateTo(ImapSessionState.Selected);
      }
    }

    protected virtual void RespondList(ImapCommand command)
    {
    }

    protected virtual void RespondLsub(ImapCommand command)
    {
    }

    protected virtual void RespondStatus(ImapCommand command)
    {
    }

    /*
     * transaction methods : selected state
     */
    protected virtual void RespondClose(ImapCommand command)
    {
      server.CloseMailbox(selectedMailbox);

      SendStatusResponse(command, ImapResponseCondition.Ok);
    }

    /*
     * methods for sending response
     */
    private void SendStatusResponse(ImapCommand command, ImapResponseCondition condition)
    {
      SendStatusResponse(command, condition, "completed");
    }

    private void SendStatusResponse(ImapCommand command, ImapResponseCondition condition, string text)
    {
      connection.SendResponse(new ImapTaggedStatusResponse(command.Tag, condition, text));
    }

    /*
     * methods for internal state management
     */
    private void TransitStateTo(ImapSessionState newState)
    {
      if (newState == state)
        return;

      switch (newState) {
        case ImapSessionState.NotConnected:
          TraceVerbose("now in non-connected state");
          break;

        case ImapSessionState.NotAuthenticated:
          TraceVerbose("now in non-authenticated state");
          if (state == ImapSessionState.Selected || state == ImapSessionState.Authenticated)
            Trace.Info("CID:{0} logged out", Id);
          break;

        case ImapSessionState.Authenticated:
          TraceVerbose("now in authenticated state");
          if (state == ImapSessionState.NotAuthenticated || state == ImapSessionState.NotConnected)
            Trace.Info("CID:{0} logged in", Id);
          break;

        case ImapSessionState.Selected:
          TraceVerbose("now in selected state (selected '{0}')", string.Empty /*selectedMailbox.Name*/); // TODO
          break;
      }

      state = newState;
    }

    /*
     * methods for checking and validating session status
     */
    private void CheckDisposed()
    {
      if (connection == null)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private void RejectNonConnectedState()
    {
      CheckDisposed();

      if (state == ImapSessionState.NotConnected)
        throw new InvalidOperationException("session has been disconnected");
    }

    private void RejectNonAuthenticatedState()
    {
      RejectNonConnectedState();

      if (state == ImapSessionState.NotAuthenticated)
        throw new InvalidOperationException("session is not in authenticated state");
    }

    private void RejectNonSelectedState()
    {
      RejectNonConnectedState();

      if (state != ImapSessionState.Selected)
        throw new InvalidOperationException("session is not in selected state");
    }

    private void RejectIdling()
    {
      if (IsIdling)
        throw new InvalidOperationException("session is idling now");
    }

    /*
     * tracing
     */
    [System.Diagnostics.Conditional("TRACE")]
    private void TraceVerbose(string format, params object[] arguments)
    {
      Trace.Log(this, format, arguments);
    }
  }
}
