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

using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Transaction.BuiltIn {
  /*
   * RFC 5255 - Internet Message Access Protocol Internationalization
   * http://tools.ietf.org/html/rfc5255
   */
  internal sealed class LanguageTransaction : ImapTransactionBase<ImapCommandResult<Tuple<string[], ImapNamespace>>>, IImapExtension {
    ImapCapability IImapExtension.RequiredCapability {
      get { return ImapCapability.Language; }
    }

    public LanguageTransaction(ImapConnection connection)
      : base(connection)
    {
    }

    protected override ProcessTransactionDelegate Reset()
    {
      return ProcessLanguage;
    }

    /*
     * 3.2. LANGUAGE Command
     *    Arguments: Optional language range arguments.
     *    Response:  A possible LANGUAGE response (see Section 3.3).
     *               A possible NAMESPACE response (see Section 3.4).
     *    Result:    OK - Command completed
     *               NO - Could not complete command
     *               BAD - Arguments invalid
     */
    private void ProcessLanguage()
    {
      // LANGUAGE
      ImapString languageRange;
      
      if (RequestArguments.TryGetValue("language range arguments", out languageRange))
        SendCommand("LANGUAGE", ProcessReceiveResponse, languageRange);
      else
        SendCommand("LANGUAGE", ProcessReceiveResponse);
    }

    protected override void OnDataResponseReceived(ImapDataResponse data)
    {
      if (data.Type == ImapDataResponseType.Language)
        languageTags = ImapDataResponseConverter.FromLanguage(data);
      else if (data.Type == ImapDataResponseType.Namespace)
        namespaces = ImapDataResponseConverter.FromNamespace(data);

      base.OnDataResponseReceived(data);
    }

    protected override void OnTaggedStatusResponseReceived(ImapTaggedStatusResponse tagged)
    {
      if (tagged.Condition == ImapResponseCondition.Ok) {
        if (RequestArguments.ContainsKey("language range arguments")) {
          /*
           *    If the command succeeds, the server will return human-readable
           *    responses in the first supported language specified.  These responses
           *    will be in UTF-8 [RFC3629].  The server MUST send a LANGUAGE response
           *    specifying the language used, and the change takes effect immediately
           *    after the LANGUAGE response.
           */
          Connection.ReceiveResponseAsUTF8 = true;

          // convert received response's encoding
          tagged.ResponseText.ConvertTextToUTF8();
        }

        Finish(new ImapCommandResult<Tuple<string[], ImapNamespace>>(Tuple.Create(languageTags, namespaces),
                                                                     tagged.ResponseText));
      }
      else {
        base.OnTaggedStatusResponseReceived(tagged);
      }
    }

    private ImapNamespace namespaces = null;
    private string[] languageTags = null;
  }
}
