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
  internal sealed class CreateTransaction : ImapTransactionBase, IImapExtension {
    ImapCapability IImapExtension.RequiredCapability {
      get { return createParametersCapabilityRequirement; }
    }

    public CreateTransaction(ImapConnection connection, ImapCapability createParametersCapabilityRequirement)
      : base(connection)
    {
      this.createParametersCapabilityRequirement = createParametersCapabilityRequirement;
    }

    protected override ProcessTransactionDelegate Reset()
    {
#if DEBUG
      if (!RequestArguments.ContainsKey("mailbox name"))
        return ProcessArgumentNotSetted;
      else
#endif
        return ProcessCreate;
    }

#if DEBUG
    private void ProcessArgumentNotSetted()
    {
      FinishError(ImapCommandResultCode.RequestError, "arguments 'mailbox name' must be setted");
    }
#endif

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
    private void ProcessCreate()
    {
      ImapString createParameters;

      if (RequestArguments.TryGetValue("create parameters", out createParameters))
        SendCommand("CREATE",
                    ProcessReceiveResponse,
                    RequestArguments["mailbox name"],
                    createParameters);
      else
        SendCommand("CREATE",
                    ProcessReceiveResponse,
                    RequestArguments["mailbox name"]);
    }

    private ImapCapability createParametersCapabilityRequirement;
  }
}
