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
   * RFC 2087 - IMAP4 QUOTA extension
   * http://tools.ietf.org/html/rfc2087
   */
  internal abstract class GetSetQuotaTransactionBase : ImapTransactionBase<ImapCommandResult<ImapQuota>>, IImapExtension {
    IEnumerable<ImapCapability> IImapExtension.RequiredCapabilities {
      get { yield return ImapCapability.Quota; }
    }

    protected GetSetQuotaTransactionBase(ImapConnection connection)
      : base(connection)
    {
    }

    /*
     * 4.1. SETQUOTA Command
     *    Arguments:  quota root
     *                list of resource limits
     * 
     *    Data:       untagged responses: QUOTA
     * 
     *    Result:     OK - setquota completed
     *                NO - setquota error: can't set that data
     *                BAD - command unknown or arguments invalid
     *
     * 4.2. GETQUOTA Command
     *    Arguments:  quota root
     * 
     *    Data:       untagged responses: QUOTA
     * 
     *    Result:     OK - getquota completed
     *                NO - getquota  error:  no  such  quota  root,  permission
     *                denied
     *                BAD - command unknown or arguments invalid
     */
    protected override ImapCommand PrepareCommand()
    {
#if DEBUG
      if (!RequestArguments.ContainsKey("quota root")) {
        FinishError(ImapCommandResultCode.RequestError, "arguments 'quota root' must be setted");
        return null;
      }
      else if (this is SetQuotaTransaction && !RequestArguments.ContainsKey("list of resource limits")) {
        FinishError(ImapCommandResultCode.RequestError, "arguments 'list of resource limits' must be setted");
        return null;
      }
#endif

      if (this is SetQuotaTransaction)
        // SETQUOTA
        return Connection.CreateCommand("SETQUOTA",
                                        RequestArguments["quota root"],
                                        RequestArguments["list of resource limits"]);
      else
        // GETQUOTA
        return Connection.CreateCommand("GETQUOTA",
                                        RequestArguments["quota root"]);
    }

    protected override void OnDataResponseReceived(ImapDataResponse data)
    {
      if (data.Type == ImapDataResponseType.Quota)
        quota = ImapDataResponseConverter.FromQuota(data);

      base.OnDataResponseReceived(data);
    }

    protected override void OnTaggedStatusResponseReceived(ImapTaggedStatusResponse tagged)
    {
      if (tagged.Condition == ImapResponseCondition.Ok)
        Finish(new ImapCommandResult<ImapQuota>(quota,
                                                tagged.ResponseText));
      else
        base.OnTaggedStatusResponseReceived(tagged);
    }

    private ImapQuota quota = null;
  }
}
