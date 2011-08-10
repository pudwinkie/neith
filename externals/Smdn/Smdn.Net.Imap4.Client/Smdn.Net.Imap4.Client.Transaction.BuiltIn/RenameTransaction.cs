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
  internal sealed class RenameTransaction : ImapTransactionBase {
    public RenameTransaction(ImapConnection connection)
      : base(connection)
    {
    }

    // RFC 4466 - Collected Extensions to IMAP4 ABNF
    // http://tools.ietf.org/html/rfc4466
    // 2.3. Extended RENAME Command
    //    Arguments:  existing mailbox name
    //                new mailbox name
    //                OPTIONAL list of RENAME parameters
    //    Responses:  no specific responses for this command
    //    Result:     OK - rename completed
    //                NO - rename failure: cannot rename mailbox with
    //                     that name, cannot rename to mailbox with
    //                     that name, etc.
    //                BAD - argument(s) invalid
    protected override ImapCommand PrepareCommand()
    {
#if DEBUG
      if (!RequestArguments.ContainsKey("existing mailbox name") ||
          !RequestArguments.ContainsKey("new mailbox name")) {
        FinishError(ImapCommandResultCode.RequestError, "arguments 'existing mailbox name' and 'new mailbox name' must be setted");
        return null;
      }
#endif

      ImapString renameParameters;

      if (RequestArguments.TryGetValue("rename parameters", out renameParameters))
        return Connection.CreateCommand("RENAME",
                                        RequestArguments["existing mailbox name"],
                                        RequestArguments["new mailbox name"],
                                        renameParameters);
      else
        return Connection.CreateCommand("RENAME",
                                        RequestArguments["existing mailbox name"],
                                        RequestArguments["new mailbox name"]);
    }
  }
}