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

using Smdn.Collections;
using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Transaction.BuiltIn {
  /*
   * RFC 2087 - IMAP4 QUOTA extension
   * http://tools.ietf.org/html/rfc2087
   */
  internal sealed class GetQuotaRootTransaction : ImapTransactionBase<ImapCommandResult<IDictionary<string, ImapQuota[]>>>, IImapExtension {
    ImapCapability IImapExtension.RequiredCapability {
      get { return ImapCapability.Quota; }
    }

    public GetQuotaRootTransaction(ImapConnection connection)
      : base(connection)
    {
    }

    protected override ProcessTransactionDelegate Reset()
    {
#if DEBUG
      if (!RequestArguments.ContainsKey("mailbox name"))
        return ProcessArgumentNotSetted;
      else
#endif
        return ProcessGetQuotaRoot;
    }

#if DEBUG
    private void ProcessArgumentNotSetted()
    {
      FinishError(ImapCommandResultCode.RequestError, "arguments 'mailbox name' must be setted");
    }
#endif

    /*
     * 4.3. GETQUOTAROOT Command
     *    Arguments:  mailbox name
     * 
     *    Data:       untagged responses: QUOTAROOT, QUOTA
     * 
     *    Result:     OK - getquota completed
     *                NO - getquota error: no such mailbox, permission denied
     *                BAD - command unknown or arguments invalid
     */
    private void ProcessGetQuotaRoot()
    {
      // GETQUOTAROOT
      SendCommand("GETQUOTAROOT", ProcessReceiveResponse, RequestArguments["mailbox name"]);
    }

    protected override void OnDataResponseReceived(ImapDataResponse data)
    {
      if (data.Type == ImapDataResponseType.QuotaRoot) {
        string[] quotaRootNames;

        quotaRoots.Add(ImapDataResponseConverter.FromQuotaRoot(data, out quotaRootNames), quotaRootNames);
      }
      else if (data.Type == ImapDataResponseType.Quota) {
        quotas.Add(ImapDataResponseConverter.FromQuota(data));
      }

      base.OnDataResponseReceived(data);
    }

    protected override void OnTaggedStatusResponseReceived(ImapTaggedStatusResponse tagged)
    {
      if (tagged.Condition == ImapResponseCondition.Ok) {
        var result = new Dictionary<string, ImapQuota[]>(StringComparer.OrdinalIgnoreCase);

        foreach (var quotaRoot in quotaRoots) {
          var rootedQuotas = new List<ImapQuota>();

          foreach (var quota in quotas.ToArray()) {
            foreach (var root in quotaRoot.Value) {
              if (string.Equals(quota.Root, root, StringComparison.OrdinalIgnoreCase)) {
                rootedQuotas.Add(quota);
                quotas.Remove(quota);
              }
            }
          }

          result.Add(quotaRoot.Key, rootedQuotas.ToArray());
        }

        Finish(new ImapCommandResult<IDictionary<string, ImapQuota[]>>(result.AsReadOnly(),
                                                                       tagged.ResponseText));
      }
      else {
        base.OnTaggedStatusResponseReceived(tagged);
      }
    }

    private Dictionary<string, string[]> quotaRoots = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
    private List<ImapQuota> quotas = new List<ImapQuota>();
  }
}

