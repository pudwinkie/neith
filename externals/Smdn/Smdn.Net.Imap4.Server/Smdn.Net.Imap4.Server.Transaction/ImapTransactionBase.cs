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
using Smdn.Net.Imap4.Protocol.Server;

namespace Smdn.Net.Imap4.Server.Transaction {
  public abstract class ImapTransactionBase : IDisposable, IImapTransaction {
    protected delegate void ProcessTransactionDelegate();

    public ImapConnection Connection {
      get
      {
        CheckDisposed();
        return connection;
      }
    }

    public ImapCommand Command {
      get
      {
        CheckDisposed();
        return command;
      }
    }

    public ImapTransactionState State {
      get
      {
        CheckDisposed();
        return state;
      }
    }

    protected ImapTransactionBase(ImapConnection connection, ImapCommand command)
    {
      if (connection == null)
        throw new ArgumentNullException("connection");

      this.connection = connection;
      this.command = command;
    }

    ~ImapTransactionBase()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      connection = null;

      disposed = true;
    }

    private ImapConnection connection;
    private ImapCommand command;
    private bool disposed = false;

#region "processing transaction / state transition"
    void IImapTransaction.Start()
    {
      CheckDisposed();

      if (state == ImapTransactionState.Finished)
        throw new InvalidOperationException("transaction already finished");

      currentProcess = Reset();

      state = ImapTransactionState.Processing;
    }

    protected abstract ProcessTransactionDelegate Reset();

    void IImapTransaction.Process()
    {
      CheckDisposed();
      RejectFinished();

      try {
        if (currentProcess != null)
          currentProcess();
      }
      catch (Exception ex) {
        Finish(/*CreateResult(ImapTransactionResultCode.InternalError, ex.Message)*/);
        Trace.Log(ex);
      }
    }

    protected void Transit(ProcessTransactionDelegate process)
    {
      if (state == ImapTransactionState.Finished)
        throw new InvalidOperationException("transaction already finished");
      if (process == null)
        throw new ArgumentNullException("process");

      currentProcess = process;
    }

    protected internal void Finish(/*TResult result*/)
    {
      RejectFinished();

      /*
      this.result = result;
      this.result.ReceivedResponses = receivedResponses;
      */

      currentProcess = null;

      state = ImapTransactionState.Finished;
    }

    private void RejectFinished()
    {
      if (state == ImapTransactionState.Finished)
        throw new InvalidOperationException("transaction already finished");
    }

    private ImapTransactionState state = ImapTransactionState.NotStarted;
    private ProcessTransactionDelegate currentProcess;
#endregion

#region "sending / receiving"
    protected void SendStatusResponse(ImapResponseCondition condition, string text)
    {
      SendResponse(new ImapTaggedStatusResponse(command.Tag, condition, text), null);
    }

    protected void SendResponse(ImapResponse response, ProcessTransactionDelegate nextProcess)
    {
      RejectFinished();

      if (response == null)
        throw new ArgumentNullException("response");

      Connection.SendResponse(response);

      if (nextProcess == null)
        Finish();
      else
        Transit(nextProcess);
    }

    protected ImapCommand ReceiveCommand()
    {
      RejectFinished();

      try {
        var resp = Connection.TryReceiveCommand();

        if (resp != null)
          receivedCommands.Enqueue(resp);

        return resp;
      }
      catch (ImapMalformedCommandException ex) {
        // protocol error, abort transaction
        throw ex;
      }
    }

    private Queue<ImapCommand> receivedCommands = new Queue<ImapCommand>();
#endregion

    private void CheckDisposed()
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);
    }
  }
}
