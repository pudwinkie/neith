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

using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Transaction.BuiltIn {
  internal sealed class GreetingTransaction : ImapTransactionBase {
    public GreetingTransaction(ImapConnection connection)
      : base(connection)
    {
    }

    protected override ImapCommand PrepareCommand()
    {
      Trace.Log(this, "waiting for server greeting response");

      // send nothing
      return null;
    }

    protected override void OnCommandContinuationRequestReceived(ImapCommandContinuationRequest continuationRequest)
    {
      FinishError(ImapCommandResultCode.ResponseError, "unexpected command continuation request");
    }

    protected override void OnDataResponseReceived(ImapDataResponse data)
    {
      FinishError(ImapCommandResultCode.ResponseError, "unexpected data response");
    }

    protected override void OnTaggedStatusResponseReceived(ImapTaggedStatusResponse tagged)
    {
      FinishError(ImapCommandResultCode.ResponseError, "unexpected tagged status response");
    }

    protected override void OnUntaggedStatusResponseReceived(ImapUntaggedStatusResponse untagged)
    {
      if (untagged.Condition == ImapResponseCondition.Ok)
        Finish(new ImapCommandResult(ImapCommandResultCode.Ok, untagged.ResponseText));
      else if (untagged.Condition == ImapResponseCondition.PreAuth)
        Finish(new ImapCommandResult(ImapCommandResultCode.PreAuth, untagged.ResponseText));
      else if (untagged.Condition == ImapResponseCondition.Bye)
        Finish(new ImapCommandResult(ImapCommandResultCode.Bye, untagged.ResponseText));
      else
        // BAD or NO; inpossible?
        Finish(new ImapCommandResult(ImapCommandResultCode.ResponseError, untagged.ResponseText));
    }
  }
}
