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
using System.Threading;

using Smdn.Net.Pop3.Protocol.Client;
using Smdn.Net.Pop3.Client.Transaction;

namespace Smdn.Net.Pop3.Client.Session {
  partial class PopSession {
    /*
     * methods for processing transaction
     */

    private class TransactionAsyncResult : IAsyncResult {
      object IAsyncResult.AsyncState {
        get { return processTransactionAsyncResult.AsyncState; }
      }

      System.Threading.WaitHandle IAsyncResult.AsyncWaitHandle {
        get { return processTransactionAsyncResult.AsyncWaitHandle; }
      }

      bool IAsyncResult.CompletedSynchronously {
        get { return processTransactionAsyncResult.CompletedSynchronously; }
      }

      bool IAsyncResult.IsCompleted {
        get { return processTransactionAsyncResult.IsCompleted; }
      }

      public IPopTransaction Transaction { get { return transaction; } }
      public IAsyncResult ProcessTransactionAsyncResult { get { return processTransactionAsyncResult; } }

      public TransactionAsyncResult(IPopTransaction transaction, IAsyncResult processTransactionAsyncResult)
      {
        this.transaction = transaction;
        this.processTransactionAsyncResult = processTransactionAsyncResult;
      }

      private /*readonly*/ IPopTransaction transaction;
      private /*readonly*/ IAsyncResult processTransactionAsyncResult;
    }

    private PopCommandResult ProcessTransaction(IPopTransaction t)
    {
      if (transactionTimeout == Timeout.Infinite) {
        // no timeout
        PreProcessTransaction(t, handlesIncapableAsException);

        ProcessTransactionInternal(t);

        return PostProcessTransaction(t);
      }
      else {
        var async = BeginProcessTransaction(t, handlesIncapableAsException);

        if (async.AsyncWaitHandle.WaitOne(transactionTimeout, false)) {
          return EndProcessTransaction(async);
        }
        else {
          CloseConnection();
          throw new TimeoutException(string.Format("transaction timeout ({0})", t.GetType().FullName));
        }
      }
    }

    private IAsyncResult BeginProcessTransaction(IPopTransaction t, bool exceptionIfIncapable)
    {
      PreProcessTransaction(t, exceptionIfIncapable);

      var processTransaction = new ProcessTransactionDelegate(ProcessTransactionInternal);

      return new TransactionAsyncResult(t, processTransaction.BeginInvoke(t, null, null));
    }

    private PopCommandResult EndProcessTransaction(IAsyncResult asyncResult)
    {
      var ar = asyncResult as TransactionAsyncResult;

      if (ar == null)
        throw new ArgumentException("invalid IAsyncResult", "asyncResult");

      var ptar = ar.ProcessTransactionAsyncResult as System.Runtime.Remoting.Messaging.AsyncResult;

      if (ptar.EndInvokeCalled)
        throw new InvalidOperationException("EndProcessTransaction already called");

      var processTransaction = ptar.AsyncDelegate as ProcessTransactionDelegate;

      return PostProcessTransaction(processTransaction.EndInvoke(ptar));
    }

    private delegate IPopTransaction ProcessTransactionDelegate(IPopTransaction t);

    private IPopTransaction ProcessTransactionInternal(IPopTransaction t)
    {
      lock (transactionLockObject) {
        Trace.LogRequest(t);

        t.Process();

        Trace.LogResponse(t);

        return t;
      }
    }

    private void PreProcessTransaction(IPopTransaction t, bool exceptionIfIncapable)
    {
      if (IsTransactionProceeding)
        throw new InvalidOperationException("another transaction proceesing");

      // check capability
      if (!exceptionIfIncapable)
        return;

      CheckServerCapability(t as IPopExtension);

      /*
      foreach (var arg in t.Request.Arguments.Values) {
        if (arg is IPopExtension)
          CheckServerCapability(arg as IPopExtension);
      }
      */
    }

    private PopCommandResult PostProcessTransaction(IPopTransaction t)
    {
      switch (t.Result.Code) {
        /*
         * disconnect without processing responses
         */
        case PopCommandResultCode.InternalError:
          CloseConnection();
          throw new PopException(t.Result.Description, t.Result.Exception);

        case PopCommandResultCode.SocketTimeout:
          CloseConnection();
          throw new TimeoutException(string.Format("socket timeout in {0}", t.GetType().FullName), t.Result.Exception);

        case PopCommandResultCode.ConnectionError:
        case PopCommandResultCode.UpgradeError:
          CloseConnection();
          throw t.Result.Exception;
      }

      return t.Result;
    }
  }
}
