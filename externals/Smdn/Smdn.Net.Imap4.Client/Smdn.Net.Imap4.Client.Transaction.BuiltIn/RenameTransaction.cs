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

using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Transaction.BuiltIn {
  internal sealed class RenameTransaction : ImapTransactionBase {
    public RenameTransaction(ImapConnection connection)
      : base(connection)
    {
    }

    protected override ProcessTransactionDelegate Reset()
    {
#if DEBUG
      if (!RequestArguments.ContainsKey("existing mailbox name"))
        return ProcessArgumentNotSetted;
      else if (!RequestArguments.ContainsKey("new mailbox name"))
        return ProcessArgumentNotSetted;
      else
#endif
        return ProcessRename;
    }

#if DEBUG
    private void ProcessArgumentNotSetted()
    {
      FinishError(ImapCommandResultCode.RequestError, "arguments 'existing mailbox name' and 'new mailbox name' must be setted");
    }
#endif

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
    private void ProcessRename()
    {
      ImapString renameParameters;

      if (RequestArguments.TryGetValue("rename parameters", out renameParameters))
        SendCommand("RENAME",
                    ProcessReceiveResponse,
                    RequestArguments["existing mailbox name"],
                    RequestArguments["new mailbox name"],
                    renameParameters);
      else
        SendCommand("RENAME",
                    ProcessReceiveResponse,
                    RequestArguments["existing mailbox name"],
                    RequestArguments["new mailbox name"]);
    }
  }
}