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
using System.IO;

using Smdn.IO;
using Smdn.Formats;
using Smdn.Net.Pop3.Protocol;
using Smdn.Net.Pop3.Protocol.Client;

namespace Smdn.Net.Pop3.Client.Transaction.BuiltIn {
  internal abstract class RetrTopTransactionBase : PopTransactionBase<PopCommandResult<Stream>> {
    protected RetrTopTransactionBase(PopConnection connection)
      : base(connection)
    {
      IsResponseMultiline = true;
    }

    protected override ProcessTransactionDelegate Reset()
    {
#if DEBUG
      if (!RequestArguments.ContainsKey("msg"))
        return ProcessArgumentNotSetted;
      else if (this is TopTransaction && !RequestArguments.ContainsKey("n"))
        return ProcessTopArgumentNotSetted;
#endif

      return ProcessRetrTop;
    }

#if DEBUG
    private void ProcessArgumentNotSetted()
    {
      FinishError(PopCommandResultCode.RequestError, "arguments 'msg' must be setted");
    }

    private void ProcessTopArgumentNotSetted()
    {
      FinishError(PopCommandResultCode.RequestError, "arguments 'n' must be setted");
    }
#endif

    /*
     * 5. The TRANSACTION State
     * RETR msg
     *    Arguments:
     *        a message-number (required) which may NOT refer to a
     *        message marked as deleted
     *    Restrictions:
     *        may only be given in the TRANSACTION state
     *    Possible Responses:
     *        +OK message follows
     *        -ERR no such message
     */

    /*
     * 7. Optional POP3 Commands
     * TOP msg n
     *    Arguments:
     *        a message-number (required) which may NOT refer to to a
     *        message marked as deleted, and a non-negative number
     *        of lines (required)
     *    Restrictions:
     *        may only be given in the TRANSACTION state
     *    Possible Responses:
     *        +OK top of message follows
     *        -ERR no such message
     */
    private void ProcessRetrTop()
    {
      if (this is RetrTransaction)
        SendCommand("RETR", ProcessReceiveResponse, RequestArguments["msg"]);
      else if (this is TopTransaction)
        SendCommand("TOP",  ProcessReceiveResponse, RequestArguments["msg"], RequestArguments["n"]);
    }

    protected override void OnStatusResponseReceived(PopStatusResponse status)
    {
      if (status.Status == PopStatusIndicator.Positive) {
        body = new ChunkedMemoryStream(512);

        base.OnStatusResponseReceived(status);

        Transit(ProcessRetrieveMessage);
      }
      else {
        base.OnStatusResponseReceived(status);
      }
    }

    private void ProcessRetrieveMessage()
    {
      if (!Connection.TryReceiveLine(body)) {
        body.Position = 0L;
        Finish(new PopCommandResult<Stream>(body, StatusResponse.ResponseText));
      }
    }

    private ChunkedMemoryStream body = null;
  }
}
