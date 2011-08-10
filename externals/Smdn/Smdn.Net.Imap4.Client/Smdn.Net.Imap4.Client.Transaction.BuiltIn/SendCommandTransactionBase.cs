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

using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Transaction.BuiltIn {
  internal abstract class SendCommandTransactionBase : ImapTransactionBase {
    public SendCommandTransactionBase(ImapConnection connection)
      : base(connection)
    {
    }

    // 6.1.3. LOGOUT Command
    //    Arguments:  none
    //    Responses:  REQUIRED untagged response: BYE
    //    Result:     OK - logout completed
    //                BAD - command unknown or arguments invalid

    // 6.4.2. CLOSE Command
    //    Arguments:  none
    //    Responses:  no specific responses for this command
    //    Result:     OK - close completed, now in authenticated state
    //                BAD - command unknown or arguments invalid
    //      The CLOSE command permanently removes all messages that have the
    //      \Deleted flag set from the currently selected mailbox, and returns
    //      to the authenticated state from the selected state.  No untagged
    //      EXPUNGE responses are sent.

    // 2. UNSELECT Command
    //    Arguments:  none
    //    Responses:  no specific responses for this command
    //    Result:     OK - unselect completed, now in authenticated state
    //                BAD - no mailbox selected, or argument supplied but
    //                      none permitted

    protected override ImapCommand PrepareCommand()
    {
      if (this is LogoutTransaction)
        return Connection.CreateCommand("LOGOUT");
      else if (this is CloseTransaction)
        return Connection.CreateCommand("CLOSE");
      else /*if (this is UnselectTransaction)*/
        return Connection.CreateCommand("UNSELECT");
    }
  }
}