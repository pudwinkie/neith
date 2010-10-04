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

using Smdn.Net.Pop3.Protocol;
using Smdn.Net.Pop3.Protocol.Client;

namespace Smdn.Net.Pop3.Client.Transaction {
  internal abstract class PopTransactionBase : PopTransactionBase<PopCommandResult> {
    protected PopTransactionBase(PopConnection connection)
      : base(connection)
    {
    }
  }

  internal abstract class PopTransactionBase<TResult> : IPopTransaction, IDisposable
    where TResult : PopCommandResult, new()
  {
    protected delegate void ProcessTransactionDelegate();

    public PopConnection Connection {
      get
      {
#if DEBUG
        CheckDisposed();
#endif
        return connection;
      }
    }

    public IDictionary<string, string> RequestArguments {
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

    PopCommandResult IPopTransaction.Result {
      get { return this.Result; }
    }

    public bool IsResponseMultiline {
      get; protected set;
    }

    protected PopStatusResponse StatusResponse {
      get; private set;
    }

    protected PopTransactionBase(PopConnection connection)
    {
      if (connection == null)
        throw new ArgumentNullException("connection");

      this.connection = connection;
      this.requestArguments = new Dictionary<string, string>(StringComparer.Ordinal);
      this.result = null;
      this.receivedResponses = new Queue<PopResponse>();
      this.IsResponseMultiline = false;
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

    private PopConnection connection;
    private Dictionary<string, string> requestArguments;
    private TResult result;
    private bool disposed = false;

#region "processing transaction / state transition"
    protected abstract ProcessTransactionDelegate Reset();

    void IPopTransaction.Process()
    {
#if DEBUG
      CheckDisposed();
      RejectFinished();
#endif

      currentProcess = Reset();

      connection.HandleResponseAsMultiline = false;

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
      catch (PopMalformedTextException ex) {
        if (response is PopStatusResponse) {
          // error in response data, abort transaction
          throw;
        }
        else {
          // error in response data, continue processing
          Trace.Log(ex);
        }
      }
    }

    protected void Transit(ProcessTransactionDelegate process)
    {
#if DEBUG
      if (isFinished)
        throw new InvalidOperationException("transaction already finished");
      if (process == null)
        throw new ArgumentNullException("process");
#endif

      currentProcess = process;
    }

    private TResult CreateExceptionResult(Exception exception)
    {
      if (exception is TimeoutException)
        return new TResult() {Exception = exception, Code = PopCommandResultCode.SocketTimeout, Description = "timed out"};
      else if (exception is PopUpgradeConnectionException)
        return new TResult() {Exception = exception, Code = PopCommandResultCode.UpgradeError, Description = "upgrade connection failed"};
      else if (exception is PopConnectionException)
        return new TResult() {Exception = exception, Code = PopCommandResultCode.ConnectionError, Description = "connection error"};
      else
        return new TResult() {Exception = exception, Code = PopCommandResultCode.InternalError, Description = "internal error"};
    }

    protected void FinishError(PopStatusResponse status)
    {
      switch (status.Status) {
        case PopStatusIndicator.Negative:
          Finish(new TResult() {Code = PopCommandResultCode.Error, ResponseText = status.Text});
          break;
        default: // positive or unexpected
          throw new InvalidOperationException("status is not -ERR");
      }
    }

    protected void FinishError(PopCommandResultCode code, string description)
    {
#if DEBUG
      if ((int)code < 300)
        throw new ArgumentException("status is not error", "status");
#endif

      Finish(new TResult() {Code = code, Description = description});
    }

    protected void FinishOk(PopStatusResponse status)
    {
#if DEBUG
      switch (status.Status) {
        case PopStatusIndicator.Positive:
#endif
          Finish(new TResult() {Code = PopCommandResultCode.Ok, ResponseText = status.Text});
#if DEBUG
          break;
        default: // negative or unexpected
          throw new InvalidOperationException("status is not +OK");
      }
#endif
    }

    protected internal void Finish(TResult result)
    {
#if DEBUG
      RejectFinished();
#endif

      this.result = result;
      this.result.ReceivedResponses = receivedResponses;
      this.result.StatusResponse = StatusResponse;

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
    protected PopResponse Receive()
    {
#if DEBUG
      RejectFinished();
#endif

      try {
        var resp = Connection.TryReceiveResponse();

        if (resp != null)
          receivedResponses.Enqueue(resp);

        return resp;
      }
      catch (PopMalformedResponseException) {
        // protocol error, abort transaction
        throw;
      }
    }

    protected void SendCommand(string command, ProcessTransactionDelegate nextProcess, params string[] arguments)
    {
#if DEBUG
      RejectFinished();
#endif

      Connection.SendCommand(new PopCommand(command, arguments));

      Transit(nextProcess);
    }

    protected void SendContinuation(params string[] arguments)
    {
#if DEBUG
      RejectFinished();
#endif

      Connection.SendCommand(new PopCommand(null, arguments));
    }

    private Queue<PopResponse> receivedResponses;
#endregion

#region "response processing"
    private void OnResponseReceived(PopResponse response)
    {
      if (response is PopFollowingResponse)
        OnFollowingResponseReceived(response as PopFollowingResponse);
      else if (response is PopStatusResponse)
        OnStatusResponseReceived(response as PopStatusResponse);
      else if (response is PopTerminationResponse)
        OnTerminationResponse(response as PopTerminationResponse);
      else if (response is PopContinuationRequest)
        OnContinuationRequestReceived(response as PopContinuationRequest);
    }

    protected virtual void OnFollowingResponseReceived(PopFollowingResponse following)
    {
    }

    protected virtual void OnStatusResponseReceived(PopStatusResponse status)
    {
      if (IsResponseMultiline) {
        if (status.Status == PopStatusIndicator.Positive) {
          if (StatusResponse == null)
            Connection.HandleResponseAsMultiline = true;
          StatusResponse = status;
        }
        else {
          StatusResponse = status;

          FinishError(status);
        }
      }
      else {
        StatusResponse = status;

        if (status.Status == PopStatusIndicator.Positive)
          FinishOk(status);
        else
          FinishError(status);
      }
    }

    protected virtual void OnTerminationResponse(PopTerminationResponse termination)
    {
      if (IsResponseMultiline)
        FinishOk(StatusResponse);
    }

    protected virtual void OnContinuationRequestReceived(PopContinuationRequest continuationRequest)
    {
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
