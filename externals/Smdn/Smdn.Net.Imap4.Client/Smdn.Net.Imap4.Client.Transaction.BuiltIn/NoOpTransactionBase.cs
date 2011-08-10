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

using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Transaction.BuiltIn {
  internal abstract class NoOpTransactionBase : ImapTransactionBase {
    protected NoOpTransactionBase(ImapConnection connection)
      : base(connection)
    {
    }

    // 6.1.2. NOOP Command
    //    Arguments:  none
    //    Responses:  no specific responses for this command (but see below)
    //    Result:     OK - noop completed
    //                BAD - command unknown or arguments invalid
    //       Since any command can return a status update as untagged data, the
    //       NOOP command can be used as a periodic poll for new messages or
    //       message status updates during a period of inactivity (this is the
    //       preferred method to do this).
    //
    //    Example:    C: a002 NOOP
    //                S: a002 OK NOOP completed
    //                   . . .
    //                C: a047 NOOP
    //                S: * 22 EXPUNGE
    //                S: * 23 EXISTS
    //                S: * 3 RECENT
    //                S: * 14 FETCH (FLAGS (\Seen \Deleted))
    //                S: a047 OK NOOP completed

    // 6.4.1. CHECK Command
    //    Arguments:  none
    //    Responses:  no specific responses for this command
    //    Result:     OK - check completed
    //                BAD - command unknown or arguments invalid
    //
    //       There is no guarantee that an EXISTS untagged response will happen
    //       as a result of CHECK.  NOOP, not CHECK, SHOULD be used for new
    //       message polling.
    protected override ImapCommand PrepareCommand()
    {
      if (this is NoOpTransaction)
        return Connection.CreateCommand("NOOP");
      else /*if (this is CheckTransaction)*/
        return Connection.CreateCommand("CHECK");
    }
  }
}
