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

namespace Smdn.Net.Imap4.Protocol.Server {
  // ResponseTypes:
  //   ImapResponse(abstract base)
  //     => handles '7. Server Responses'
  //     ImapStatusResponse(abstract base)
  //       => handles '7.1. Server Responses - Status Responses'
  //       ImapTaggedStatusResponse
  //       => handles tagged status response
  //       ImapUntaggedStatusResponse
  //       => handles tagged status response
  //     ImapDataResponse
  //       => handles '7.2. Server Responses - Server and Mailbox Status'
  //                  '7.3. Server Responses - Mailbox Size'
  //                  '7.4. Server Responses - Message Status'
  //     ImapCommandContinuationRequest
  //       => handles '7.5. Server Responses - Command Continuation Request'
  public abstract class ImapStatusResponse : ImapResponse {
    public abstract string Tag { get; }

    public ImapResponseCondition Condition {
      get; private set;
    }

    public ImapString ResponseTextCode {
      get; private set;
    }

    public string Text {
      get; private set;
    }

    protected ImapStatusResponse(ImapResponseCondition condition, ImapString responseTextCode, string text)
    {
      this.Condition = condition;
      this.ResponseTextCode = responseTextCode;
      this.Text = text;
    }

    internal override IEnumerable<ImapString> GetResponseLine()
    {
      var resp = new List<ImapString>();

      resp.Add(Tag);

      switch (Condition) {
        case ImapResponseCondition.Ok:      resp.Add("OK"); break;
        case ImapResponseCondition.No:      resp.Add("NO"); break;
        case ImapResponseCondition.Bad:     resp.Add("BAD"); break;
        case ImapResponseCondition.Bye:     resp.Add("BYE"); break;
        case ImapResponseCondition.PreAuth: resp.Add("PREAUTH"); break;
        default: throw new InvalidOperationException("invalid condition");
      }

      if (ResponseTextCode != null) {
        resp.Add("[");
        resp.Add(ResponseTextCode);
        resp.Add("]");
      }

      if (Text != null)
        resp.Add(Text);

      return resp;
    }
  }
}
