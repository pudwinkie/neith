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

using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Transaction.BuiltIn {
  /*
   * RFC 5464 - The IMAP METADATA Extension
   * http://tools.ietf.org/html/rfc5464
   */
  internal sealed class SetMetadataTransaction : ImapTransactionBase {
    /*
     * must be checked by caller
    ImapCapability IImapExtension.RequiredCapability {
      get { throw new NotImplementedException(); }
    }
    */

    public SetMetadataTransaction(ImapConnection connection)
      : base(connection)
    {
    }

    /*
     * 4.3. SETMETADATA Command
     *        Arguments:  mailbox-name
     *                    entry
     *                    value
     *                    list of entry, values
     *        Responses:  no specific responses for this command
     *        Result:     OK - command completed
     *                    NO - command failure: can't set annotations,
     *                         or annotation too big or too many
     *                    BAD - command unknown or arguments invalid
     */
    protected override ImapCommand PrepareCommand()
    {
#if DEBUG
      if (!RequestArguments.ContainsKey("mailbox-name") ||
          !RequestArguments.ContainsKey("list of entry, values")) {
        FinishError(ImapCommandResultCode.RequestError, "arguments 'mailbox-name' and 'list of entry, values' must be setted");
        return null;
      }
#endif

      // SETMETADATA
      return Connection.CreateCommand("SETMETADATA",
                                      RequestArguments["mailbox-name"],
                                      RequestArguments["list of entry, values"]);
    }
  }
}
