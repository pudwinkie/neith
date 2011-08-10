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
    protected abstract ImapCommand PrepareCommand();

    protected void SendContinuation(params ImapString[] arguments)
    {
#if DEBUG
      RejectFinished();
#endif

      commandContinuationContext = connection.SendCommand(connection.CreateContinuingCommand(arguments));
    }

    void IImapTransaction.Process()
    {
#if DEBUG
      CheckDisposed();
      RejectFinished();
#endif

      // prepare and send command
      try {
        var command = PrepareCommand();

        if (command != null) {
          commandContinuationContext = connection.SendCommand(command);

          lastCommandTag = command.Tag;
        }
      }
      catch (Exception ex) {
        Finish(CreateExceptionResult(ex));
        Trace.Log(ex);
      }

      // receive response or send continuation
      for (;;) {
        if (isFinished)
          return;

        try {
          ProcessReceiveResponse();
        }
        catch (Exception ex) {
          Finish(CreateExceptionResult(ex));
          Trace.Log(ex);
        }
      }
    }

    private void ProcessReceiveResponse()
    {
      ImapResponse response = null;

      try {
        for (;;) {
          response = connection.TryReceiveResponse();

          if (response != null) {
            receivedResponses.Enqueue(response);
            break;
          }
        }
      }
      catch (ImapMalformedResponseException) {
        // protocol error, abort transaction
        throw;
      }

      try {
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

      isFinished = true;
    }

#if DEBUG
    private void RejectFinished()
    {
      if (isFinished)
        throw new InvalidOperationException("transaction already finished");
    }
#endif

    private bool isFinished;
    private string lastCommandTag = null;
    private ImapConnection.ICommandContinuationContext commandContinuationContext = null;
    private Queue<ImapResponse> receivedResponses;
    private ImapTaggedStatusResponse taggedStatusResponse = null;
#endregion

#region "response processing"
    protected virtual void OnDataResponseReceived(ImapDataResponse data)
    {
    }

    protected virtual void OnCommandContinuationRequestReceived(ImapCommandContinuationRequest continuationRequest)
    {
      if (commandContinuationContext == null)
        throw new ImapException("unexpected command continuation or sending literal failed");

      commandContinuationContext = connection.SendCommand(commandContinuationContext);
    }

    protected virtual void OnStatusResponseReceived(ImapStatusResponse status)
    {
      if (status.ResponseText.Code == ImapResponseCode.Alert) // XXX: equality
        // ALERT
        Trace.Info(status.ResponseText.Text);
      else if (status.ResponseText.Code == ImapResponseCode.ClientBug) // XXX: equality
        // CLIENTBUG
        Trace.Info(status.ResponseText.Text);

      var tagged = status as ImapTaggedStatusResponse;

      if (tagged == null) {
        OnUntaggedStatusResponseReceived(status as ImapUntaggedStatusResponse);
      }
      else {
        taggedStatusResponse = tagged;

        if (tagged.Tag == lastCommandTag) // XXX: equality
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
