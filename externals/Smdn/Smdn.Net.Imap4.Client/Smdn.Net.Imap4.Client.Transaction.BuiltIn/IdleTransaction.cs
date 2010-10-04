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

using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Transaction.BuiltIn {
  internal sealed class IdleTransaction : ImapTransactionBase, IImapExtension {
    ImapCapability IImapExtension.RequiredCapability {
      get { return ImapCapability.Idle; }
    }

    public IdleTransaction(ImapConnection connection, object keepIdleState, ImapKeepIdleCallback keepIdleCallback)
      : base(connection)
    {
      this.keepIdleState = keepIdleState;
      this.keepIdleCallback = keepIdleCallback;
    }

    public override void Dispose()
    {
      if (idleStateChangedEvent != null) {
        idleStateChangedEvent.Close();
        idleStateChangedEvent = null;
      }

      base.Dispose();
    }

    protected override ProcessTransactionDelegate Reset()
    {
      return ProcessIdle;
    }

    // 3. Specification
    //    IDLE Command
    //    Arguments:  none
    //    Responses:  continuation data will be requested; the client sends
    //                the continuation data "DONE" to end the command
    //    Result:     OK - IDLE completed after client sent "DONE"
    //                NO - failure: the server will not allow the IDLE
    //                     command at this time
    //               BAD - command unknown or arguments invalid
    private void ProcessIdle()
    {
      SendCommand("IDLE", ProcessReceiveResponse);
    }

    internal void Done()
    {
      if (Connection.IsIdling &&
          !IsFinished &&
          Interlocked.CompareExchange(ref doneSent, 1, 0) == 0) {
        Connection.ReceiveTimeout = prevReceiveTimeout;

        SendContinuation("DONE");
      }
    }

    internal void WaitForIdleStateChanged()
    {
      idleStateChangedEvent.WaitOne();
    }

    protected override void OnCommandContinuationRequestReceived(ImapCommandContinuationRequest continuationRequest)
    {
      prevReceiveTimeout = Connection.ReceiveTimeout;

      Connection.ReceiveTimeout = 30 * 1000; // 30secs
      Connection.SetIsIdling(true);

      idleStateChangedEvent.Set();
    }

    protected override void OnDataResponseReceived(ImapDataResponse data)
    {
      // 3. Specification
      //   IDLE Command
      //   The IDLE command remains active until the client
      //   responds to the continuation, and as long as an IDLE command is
      //   active, the server is now free to send untagged EXISTS, EXPUNGE, and
      //   other messages at any time
      if (keepIdleCallback != null) {
        var updatedStatus = ImapUpdatedStatus.CreateFrom(data);

        if (updatedStatus != null && !keepIdleCallback(keepIdleState, updatedStatus))
          Done();
      }

      base.OnDataResponseReceived(data);
    }

    protected override void OnTaggedStatusResponseReceived(ImapTaggedStatusResponse tagged)
    {
      Connection.SetIsIdling(false);

      idleStateChangedEvent.Set();

      base.OnTaggedStatusResponseReceived(tagged);
    }

    private ImapKeepIdleCallback keepIdleCallback;
    private object keepIdleState;
    private int doneSent = 0;
    private int prevReceiveTimeout;
    private AutoResetEvent idleStateChangedEvent = new AutoResetEvent(false);
  }
}