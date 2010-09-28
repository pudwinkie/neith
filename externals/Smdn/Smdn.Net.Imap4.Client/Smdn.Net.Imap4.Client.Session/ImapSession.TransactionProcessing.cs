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
using System.Threading;

using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;
using Smdn.Net.Imap4.Client.Transaction;
using Smdn.Net.Imap4.Client.Transaction.BuiltIn;

namespace Smdn.Net.Imap4.Client.Session {
  partial class ImapSession {
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

      public IImapTransaction Transaction { get { return transaction; } }
      public IAsyncResult ProcessTransactionAsyncResult { get { return processTransactionAsyncResult; } }

      public TransactionAsyncResult(IImapTransaction transaction, IAsyncResult processTransactionAsyncResult)
      {
        this.transaction = transaction;
        this.processTransactionAsyncResult = processTransactionAsyncResult;
      }

      private /*readonly*/ IImapTransaction transaction;
      private /*readonly*/ IAsyncResult processTransactionAsyncResult;
    }

    private ImapCommandResult ProcessTransaction(IImapTransaction t)
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

    private IAsyncResult BeginProcessTransaction(IImapTransaction t, bool exceptionIfIncapable)
    {
      PreProcessTransaction(t, exceptionIfIncapable);

      var processTransaction = new ProcessTransactionDelegate(ProcessTransactionInternal);

      return new TransactionAsyncResult(t, processTransaction.BeginInvoke(t, null, null));
    }

    private ImapCommandResult EndProcessTransaction(IAsyncResult asyncResult)
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

    private delegate IImapTransaction ProcessTransactionDelegate(IImapTransaction t);

    private IImapTransaction ProcessTransactionInternal(IImapTransaction t)
    {
      lock (transactionLockObject) {
        Trace.LogRequest(t);

        if (t is IdleTransaction)
          TraceInfo("idling");

        t.Process();

        if (t is IdleTransaction)
          TraceInfo("done");

        Trace.LogResponse(t);

        return t;
      }
    }

    private void PreProcessTransaction(IImapTransaction t, bool exceptionIfIncapable)
    {
      // TODO: '5.5. Multiple Commands in Progress'
      //   The following are examples of valid non-waiting command sequences:
      //      FETCH + STORE + SEARCH + CHECK
      //      STORE + COPY + EXPUNGE
      if (IsTransactionProceeding)
        throw new InvalidOperationException("another transaction proceesing");

      RejectIdling();

      // check and set literal options
      var isNonSyncLiteralCapable = serverCapabilities.Has(ImapCapability.LiteralNonSync);
      var isLiteral8Capable = serverCapabilities.Has(ImapCapability.Binary);
      var containsNonSyncLiteral = false;
      var containsLiteral8 = false;

      TraverseTransactionRequestArgumentLiterals(t.RequestArguments, delegate(IImapLiteralString literal) {
        var syncMode = literal.Options & ImapLiteralOptions.SynchronizationMode;
        var literalMode = literal.Options & ImapLiteralOptions.LiteralMode;

        switch (syncMode) {
          case ImapLiteralOptions.NonSynchronizingIfCapable:
            syncMode = isNonSyncLiteralCapable ? ImapLiteralOptions.NonSynchronizing : ImapLiteralOptions.Synchronizing;
            break;
          case ImapLiteralOptions.NonSynchronizing:
            containsNonSyncLiteral = true;
            break;
        }

        switch (literalMode) {
          case ImapLiteralOptions.Literal8IfCapable:
            literalMode = isLiteral8Capable ? ImapLiteralOptions.Literal8 : ImapLiteralOptions.Literal;
            break;
          case ImapLiteralOptions.Literal8:
            containsLiteral8 = true;
            break;
        }

        literal.Options = (syncMode | literalMode)
                          | (literal.Options & ~(ImapLiteralOptions.SynchronizationMode | ImapLiteralOptions.LiteralMode));
      });

      // check capability
      if (!exceptionIfIncapable)
        return;

      if (!serverCapabilities.Has(Imap4.ImapCapability.Imap4Rev1) && !(t is CapabilityTransaction))
        throw new ImapIncapableException(ImapCapability.Imap4Rev1);

      if (containsNonSyncLiteral && !isNonSyncLiteralCapable)
        throw new ImapIncapableException(ImapCapability.LiteralNonSync);
      if (containsLiteral8 && !isLiteral8Capable)
        throw new ImapIncapableException(ImapCapability.Binary);

      CheckServerCapability(t as IImapExtension);
      CheckServerCapability(t as IImapMultipleExtension);

      foreach (var arg in t.RequestArguments.Values) {
        CheckServerCapability(arg as IImapExtension);
        CheckServerCapability(arg as IImapMultipleExtension);
      }
    }

    private void TraverseTransactionRequestArgumentLiterals(IDictionary<string, ImapString> requestArguments, Action<IImapLiteralString> action)
    {
      foreach (var s in requestArguments.Values) {
        var literal = s as IImapLiteralString;

        if (literal == null) {
          var list = s as ImapStringList;

          if (list == null)
            continue;

          list.Traverse(delegate(ImapString ss) {
            var l = ss as IImapLiteralString;

            if (l != null)
              action(l);
          });
        }
        else {
          action(literal);
        }
      }
    }

    private ImapCommandResult PostProcessTransaction(IImapTransaction t)
    {
      lastTransactionResult = t.Result;

      switch (t.Result.Code) {
        /*
         * disconnect without processing responses
         */
        case ImapCommandResultCode.InternalError:
          CloseConnection();
          throw new ImapException(t.Result.Description, t.Result.Exception);

        case ImapCommandResultCode.SocketTimeout:
          CloseConnection();
          throw new TimeoutException(string.Format("socket timeout in {0}", t.GetType().FullName), t.Result.Exception);

        case ImapCommandResultCode.ConnectionError:
        case ImapCommandResultCode.UpgradeError:
          CloseConnection();
          throw t.Result.Exception;
       }

      ProcessUpdatedSizeAndStatusResponse(t.Result);

      if (t.Result.Code == ImapCommandResultCode.Bye) {
        TransitStateTo(ImapSessionState.NotAuthenticated);
        CloseConnection();
      }

      return t.Result;
    }
  }
}
