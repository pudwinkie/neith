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
  internal sealed class GetMetadataTransaction : ImapTransactionBase<ImapCommandResult<ImapMetadata[]>> {
    /*
     * must be checked by caller
    ImapCapability IImapExtension.RequiredCapability {
      get { throw new NotImplementedException(); }
    }
    */

    public GetMetadataTransaction(ImapConnection connection)
      : base(connection)
    {
    }

    /*
     * 4.2. GETMETADATA Command
     *        Arguments:  mailbox-name
     *                    options
     *                    entry-specifier
     *        Responses:  required METADATA response
     *        Result:     OK - command completed
     *                    NO - command failure: can't access annotations on
     *                         the server
     *                    BAD - command unknown or arguments invalid
     */
    protected override ImapCommand PrepareCommand()
    {
#if DEBUG
      if (!RequestArguments.ContainsKey("mailbox-name") ||
          !RequestArguments.ContainsKey("entry-specifier")) {
        FinishError(ImapCommandResultCode.RequestError, "arguments 'mailbox-name' and 'entry-specifier' must be setted");
        return null;
      }
#endif

      // GETMETADATA
      ImapString options;

      if (RequestArguments.TryGetValue("options", out options))
        return Connection.CreateCommand("GETMETADATA",
                                        RequestArguments["mailbox-name"],
                                        options,
                                        RequestArguments["entry-specifier"]);
      else
        return Connection.CreateCommand("GETMETADATA",
                                        RequestArguments["mailbox-name"],
                                        RequestArguments["entry-specifier"]);
    }

    protected override void OnDataResponseReceived(ImapDataResponse data)
    {
      if (data.Type == ImapDataResponseType.Metadata) {
        string discard;

        metadata.AddRange(ImapDataResponseConverter.FromMetadataEntryValues(data, out discard));
      }

      base.OnDataResponseReceived(data);
    }

    protected override void OnTaggedStatusResponseReceived(ImapTaggedStatusResponse tagged)
    {
      if (tagged.Condition == ImapResponseCondition.Ok) {
        Finish(new ImapCommandResult<ImapMetadata[]>(metadata.ToArray(),
                                                     tagged.ResponseText));

        metadata.Clear();
      }
      else {
        base.OnTaggedStatusResponseReceived(tagged);
      }
    }

    private List<ImapMetadata> metadata = new List<ImapMetadata>();
  }
}
