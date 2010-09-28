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

using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Transaction {
  // Layers:
  //   Session.ImapSession
  // * Transaction.IImapTransaction
  //   Protocol.ImapConnection/Protocol.ImapCommand/Protocol.ImapResponse
  //   (IMAP4)

  internal abstract class ImapTransactionBase : ImapTransactionBase<ImapCommandResult> {
    protected ImapTransactionBase(ImapConnection connection)
      : base(connection)
    {
    }
  }

  internal abstract class ImapTransactionBase<TResult> : IImapTransaction, IDisposable
    where TResult : ImapCommandResult, new()
  {
    protected delegate void ProcessTransactionDelegate();

    public ImapConnection Connection {
      get
      {
#if DEBUG
        CheckDisposed();
#endif
        return connection;
      }
    }

    public IDictionary<string, ImapString> RequestArguments {
      get
      {
#if DEBUG
        CheckDisposed();
#endif
        return requestArguments;
      }
    }

    public TResult Result {
      get
      {
#if DEBUG
        CheckDisposed();
#endif
        return result;
      }
    }

    ImapCommandResult IImapTransaction.Result {
      get { return this.Result; }
    }

    protected string LastCommandTag {
      get { return lastCommandTag; }
    }

    protected bool IsFinished {
      get { return isFinished; }
    }

    protected ImapTransactionBase(ImapConnection connection)
    {
      if (connection == null)
        throw new ArgumentNullException("connection");

      this.connection = connection;
      this.requestArguments = new Dictionary<string, ImapString>(StringComparer.Ordinal);
      this.result = null;
      this.receivedResponses = new Queue<ImapResponse>();
    }

    public virtual void Dispose()
    {
      if (disposed)
        return;

      // nothing to do in this class
      connection = null;
      requestArguments = null;
      result = null;
      receivedResponses = null;

      disposed = true;
    }

    private ImapConnection connection;
    private Dictionary<string, ImapString> requestArguments;
    private TResult result;
    private bool disposed = false;

#region "processing transaction / state transition"
    protected abstract ProcessTransactionDelegate Reset();

    void IImapTransaction.Process()
    {
#if DEBUG
      CheckDisposed();
      RejectFinished();
#endif

      currentProcess = Reset();

      for (;;) {
        try {
          if (currentProcess != null)
            currentProcess();
        }
        catch (Exception ex) {
          Finish(CreateExceptionResult(ex));
          Trace.Log(ex);
        }

        if (isFinished)
          return;
      }
    }

    /// <remarks>do not call directly</remarks>
    protected void ProcessReceiveResponse()
    {
      var response = Receive();

      if (response == null)
        return;

      try {
        OnResponseReceived(response);
      }
      catch (ImapMalformedDataException ex) {
        if (response is ImapTaggedStatusResponse) {
          // error in response data, abort transaction
          throw;
        }
        else {
          // error in response data, continue processing
          Trace.Log(ex);
        }
      }
    }

    private TResult CreateExceptionResult(Exception exception)
    {
      if (exception is TimeoutException)
        return new TResult() {Exception = exception, Code = ImapCommandResultCode.SocketTimeout, Description = "timed out"};
      else if (exception is ImapUpgradeConnectionException)
        return new TResult() {Exception = exception, Code = ImapCommandResultCode.UpgradeError, Description = "upgrade connection failed"};
      else if (exception is ImapConnectionException)
        return new TResult() {Exception = exception, Code = ImapCommandResultCode.ConnectionError, Description = "connection error"};
      else
        return new TResult() {Exception = exception, Code = ImapCommandResultCode.InternalError, Description = "internal error"};
    }

    protected void FinishError(ImapTaggedStatusResponse tagged)
    {
      switch (tagged.Condition) {
        case ImapResponseCondition.No:
          Finish(new TResult() {Code = ImapCommandResultCode.No, ResponseText = tagged.ResponseText.Text});
          break;
        case ImapResponseCondition.Bad:
          Finish(new TResult() {Code = ImapCommandResultCode.Bad, ResponseText = tagged.ResponseText.Text});
          break;
        case ImapResponseCondition.Bye:
          Finish(new TResult() {Code = ImapCommandResultCode.Bye, ResponseText = tagged.ResponseText.Text});
          break;
        default: // OK, PREAUTH
          throw new InvalidOperationException("status is not NO/BAD/BYE");
      }
    }

    protected void FinishError(ImapCommandResultCode code, string description)
    {
      if ((int)code < 300)
        throw new ArgumentException("status is not error", "status");

      Finish(new TResult() {Code = code, Description = description});
    }

    protected void FinishOk(ImapTaggedStatusResponse tagged)
    {
      switch (tagged.Condition) {
        case ImapResponseCondition.Ok:
          Finish(new TResult() {Code = ImapCommandResultCode.Ok, ResponseText = tagged.ResponseText.Text});
          break;
        case ImapResponseCondition.PreAuth:
          Finish(new TResult() {Code = ImapCommandResultCode.PreAuth, ResponseText = tagged.ResponseText.Text});
          break;
        default: // NO, BAD, BYE
          throw new InvalidOperationException("status is not OK/PREAUTH");
      }
    }

    protected internal void Finish(TResult result)
    {
#if DEBUG
      RejectFinished();
#endif

      this.result = result;
      this.result.ReceivedResponses = receivedResponses;
      this.result.TaggedStatusResponse = taggedStatusResponse;

      currentProcess = null;

      isFinished = true;
    }

    private void RejectFinished()
    {
      if (isFinished)
        throw new InvalidOperationException("transaction already finished");
    }

    private bool isFinished;
    private ProcessTransactionDelegate currentProcess;
#endregion

#region "sending / receiving"
    protected ImapResponse Receive()
    {
#if DEBUG
      RejectFinished();
#endif

      try {
        var resp = Connection.TryReceiveResponse();

        if (resp != null) {
          receivedResponses.Enqueue(resp);

          var tagged = resp as ImapTaggedStatusResponse;

          if (tagged != null)
            taggedStatusResponse = tagged;
        }

        return resp;
      }
      catch (ImapMalformedResponseException) {
        // protocol error, abort transaction
        throw;
      }
    }

    protected void SendCommand(string command, ProcessTransactionDelegate nextProcess, params ImapString[] arguments)
    {
#if DEBUG
      RejectFinished();

      if (nextProcess == null)
        throw new ArgumentNullException("nextProcess");
#endif

      var comm = Connection.CreateCommand(command, arguments);

      commandContinuationContext = Connection.SendCommand(comm);

      lastCommandTag = comm.Tag;

      currentProcess = nextProcess;
    }

    protected void SendContinuation(params ImapString[] arguments)
    {
#if DEBUG
      RejectFinished();
#endif

      commandContinuationContext = Connection.SendCommand(Connection.CreateContinuingCommand(arguments));
    }

    protected bool SendContinuation()
    {
#if DEBUG
      RejectFinished();
#endif

      if (commandContinuationContext == null)
        return false;

      commandContinuationContext = Connection.SendCommand(commandContinuationContext);

      return true;
    }

    private string lastCommandTag = null;
    private ImapConnection.ICommandContinuationContext commandContinuationContext = null;
    private Queue<ImapResponse> receivedResponses;
    private ImapTaggedStatusResponse taggedStatusResponse = null;
#endregion

#region "response processing"
    private void OnResponseReceived(ImapResponse response)
    {
      var data = response as ImapDataResponse;

      if (data == null) {
        var status = response as ImapStatusResponse;

        if (status == null) {
          if (response is ImapCommandContinuationRequest)
            OnCommandContinuationRequestReceived(response as ImapCommandContinuationRequest);
        }
        else {
          OnStatusResponseReceived(status);
        }
      }
      else {
        OnDataResponseReceived(data);
      }
    }

    protected virtual void OnDataResponseReceived(ImapDataResponse data)
    {
    }

    protected virtual void OnCommandContinuationRequestReceived(ImapCommandContinuationRequest continuationRequest)
    {
      if (!SendContinuation())
        throw new ImapException("unexpected command continuation or sending literal failed");
    }

    protected virtual void OnStatusResponseReceived(ImapStatusResponse status)
    {
      if (status.ResponseText.Code == ImapResponseCode.Alert)
        // ALERT
        Trace.Info(status.ResponseText.Text);
      else if (status.ResponseText.Code == ImapResponseCode.ClientBug)
        // CLIENTBUG
        Trace.Info(status.ResponseText.Text);

      var tagged = status as ImapTaggedStatusResponse;

      if (tagged == null) {
        OnUntaggedStatusResponseReceived(status as ImapUntaggedStatusResponse);
      }
      else {
        if (tagged.Tag == lastCommandTag)
          OnTaggedStatusResponseReceived(tagged);
        else
          // TODO: '5.5. Multiple Commands in Progress' or not matched tag
          throw new ImapMalformedResponseException(string.Format("command tag not matched: expected {0} but was {1}",
                                                                 lastCommandTag,
                                                                 tagged.Tag));
      }
    }

    protected virtual void OnUntaggedStatusResponseReceived(ImapUntaggedStatusResponse untagged)
    {
      if (untagged.Condition == ImapResponseCondition.Bye)
        // The BYE response is always untagged
        Finish(new TResult() {Code = ImapCommandResultCode.Bye, ResponseText = untagged.ResponseText.Text});
    }

    protected virtual void OnTaggedStatusResponseReceived(ImapTaggedStatusResponse tagged)
    {
      if (tagged.Condition == ImapResponseCondition.Ok || tagged.Condition == ImapResponseCondition.PreAuth)
        FinishOk(tagged);
      else
        FinishError(tagged);
    }
#endregion

    public override string ToString()
    {
      return string.Format("{{Connection={0}, RequestArguments={1}, Result={2}}}", connection, requestArguments, result);
    }

    private void CheckDisposed()
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);
    }
  }
}
