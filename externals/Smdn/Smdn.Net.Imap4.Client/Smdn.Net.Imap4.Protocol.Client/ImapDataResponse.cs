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

namespace Smdn.Net.Imap4.Protocol.Client {
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

  [Serializable]
  public sealed class ImapDataResponse : ImapResponse {
    public ImapDataResponseType Type {
      get; private set;
    }

    public ImapData[] Data {
      get; private set;
    }

    internal static ImapDataResponse Create(ImapData[] data)
    {
      ImapDataResponseType responseType;

      if (2 <= data.Length &&
          data[1].Format == ImapDataFormat.Text &&
          ImapDataResponseType.SizeStatusTypes.TryGet(data[1].GetTextAsString(), out responseType)) {
        // 'Mailbox Size' or 'Message Status'
        var dataWithoutType = new ImapData[data.Length - 1];

        dataWithoutType[0] = data[0];

        Array.Copy(data, 2, dataWithoutType, 1, dataWithoutType.Length - 1);

        return new ImapDataResponse(responseType, dataWithoutType);
      }
      else if (1 <= data.Length &&
               data[0].Format == ImapDataFormat.Text &&
               ImapDataResponseType.AllTypes.TryGet(data[0].GetTextAsString(), out responseType)) {
        return new ImapDataResponse(responseType,
                                    data.Slice(1));
      }
      else {
        Smdn.Net.Imap4.Client.Trace.Verbose(string.Concat("unknown data response type: ", data[0]));

        return new ImapDataResponse(ImapDataResponseType.InvalidOrUnknown, data);
      }
    }

    private ImapDataResponse(ImapDataResponseType type, ImapData[] data)
    {
      this.Type = type;
      this.Data = data;
    }

    public override string ToString()
    {
      return string.Format("{{Type={0}, Data={1}}}", Type, string.Join(", ", Array.ConvertAll(Data, delegate(ImapData d) {
        return d.ToString();
      })));
    }
  }
}