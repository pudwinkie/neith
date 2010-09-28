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

namespace Smdn.Net.Pop3.Protocol.Client {
  // ResponseTypes:
  //   PopResponse(abstract base)
  //     => handles all response
  //     PopStatusResponse
  //       => handles '+OK' or '-ERR' status indicator responses
  //     PopFollowingResponse
  //       => handles a line of following multi-line response after PopStatusResponse
  //     PopTerminationResponse
  //       => handles a termination of multi-line response (CRLF.CRLF)
  //   * PopContinuationRequest
  //       => handles 'RFC 1734 POP3 AUTHentication command' continuation request

  public sealed class PopContinuationRequest : PopResponse {
    public ByteString Base64Text {
      get; private set;
    }

    internal PopContinuationRequest(ByteString base64Text)
    {
      this.Base64Text = base64Text;
    }

    public override string ToString()
    {
      return string.Format("{{Base64Text={0}}}", Base64Text);
    }
  }
}