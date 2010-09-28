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

using Smdn.Net.Pop3.Protocol;
using Smdn.Net.Pop3.Protocol.Client;

namespace Smdn.Net.Pop3.Client.Transaction.BuiltIn {
  internal sealed class UidlTransaction : PopTransactionBase<PopCommandResult<PopUniqueIdListing[]>>, IPopExtension {
    PopCapability IPopExtension.RequiredCapability {
      get { return PopCapability.Uidl; }
    }

    public UidlTransaction(PopConnection connection)
      : base(connection)
    {
    }

    protected override ProcessTransactionDelegate Reset()
    {
      messageNumberSpecified = RequestArguments.ContainsKey("msg");

      IsResponseMultiline = !messageNumberSpecified;

      return ProcessUidl;
    }

    /*
     * 7. Optional POP3 Commands
     * UIDL [msg]
     *    Arguments:
     *        a message-number (optional), which, if present, may NOT
     *        refer to a message marked as deleted
     *    Restrictions:
     *        may only be given in the TRANSACTION state.
     *    Possible Responses:
     *        +OK unique-id listing follows
     *        -ERR no such message
     */
    private void ProcessUidl()
    {
      if (messageNumberSpecified)
        SendCommand("UIDL", ProcessReceiveResponse, RequestArguments["msg"]);
      else
        SendCommand("UIDL", ProcessReceiveResponse);
    }

    protected override void OnStatusResponseReceived(PopStatusResponse status)
    {
      if (messageNumberSpecified && status.Status == PopStatusIndicator.Positive) {
        Finish(new PopCommandResult<PopUniqueIdListing[]>(new[] {PopResponseConverter.FromUidl(status)}, status.ResponseText));
        return;
      }

      base.OnStatusResponseReceived(status);
    }

    protected override void OnFollowingResponseReceived(PopFollowingResponse following)
    {
      uniqueIdListings.Add(PopResponseConverter.FromUidl(following));
    }

    protected override void OnTerminationResponse(PopTerminationResponse termination)
    {
      Finish(new PopCommandResult<PopUniqueIdListing[]>(uniqueIdListings.ToArray(), StatusResponse.ResponseText));
    }

    private bool messageNumberSpecified;
    private List<PopUniqueIdListing> uniqueIdListings = new List<PopUniqueIdListing>();
  }
}
