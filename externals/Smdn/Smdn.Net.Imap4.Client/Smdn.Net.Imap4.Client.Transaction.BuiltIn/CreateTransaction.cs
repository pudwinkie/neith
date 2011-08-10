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
  internal sealed class CreateTransaction : ImapTransactionBase, IImapExtension {
    IEnumerable<ImapCapability> IImapExtension.RequiredCapabilities {
      get
      {
        if (createParametersCapabilityRequirement != null)
          yield return createParametersCapabilityRequirement;
      }
    }

    public CreateTransaction(ImapConnection connection, ImapCapability createParametersCapabilityRequirement)
      : base(connection)
    {
      this.createParametersCapabilityRequirement = createParametersCapabilityRequirement;
    }

    // RFC 4466 - Collected Extensions to IMAP4 ABNF
    // http://tools.ietf.org/html/rfc4466
    // 2.2. Extended CREATE Command
    //    Arguments:  mailbox name
    //                OPTIONAL list of CREATE parameters
    //    Responses:  no specific responses for this command
    //    Result:     OK - create completed
    //                NO - create failure: cannot create mailbox with
    //                     that name
    //                BAD - argument(s) invalid
    protected override ImapCommand PrepareCommand()
    {
#if DEBUG
      if (!RequestArguments.ContainsKey("mailbox name")) {
        FinishError(ImapCommandResultCode.RequestError, "arguments 'mailbox name' must be setted");
        return null;
      }
#endif

      ImapString createParameters;

      if (RequestArguments.TryGetValue("create parameters", out createParameters))
        return Connection.CreateCommand("CREATE",
                                        RequestArguments["mailbox name"],
                                        createParameters);
      else
        return Connection.CreateCommand("CREATE",
                                        RequestArguments["mailbox name"]);
    }

    private ImapCapability createParametersCapabilityRequirement;
  }
}
