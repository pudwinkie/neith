// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2010-2011 smdn
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
using System.IO;
using System.Runtime.Serialization;

namespace Smdn.Net.Pop3.WebClients {
  // TODO: [Serializable]
  internal sealed class PopMessageWebRequest : PopWebRequest {
    public PopMessageWebRequest(Uri requestUri, PopSessionManager sessionManager)
      : base(requestUri, PopWebRequestMethods.Retr, sessionManager)
    {
    }

    protected PopMessageWebRequest(SerializationInfo serializationInfo, StreamingContext streamingContext)
      : base(serializationInfo, streamingContext)
    {
    }

    protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
      base.GetObjectData(serializationInfo, streamingContext);
    }

    internal protected long MessageNumber {
      get { return ExtendedPopStyleUriParser.GetMsg(RequestUri); }
    }

    protected override PopWebResponse InternalGetResponse()
    {
      switch (Method) {
        case PopWebRequestMethods.Retr: return GetRetrResponse();
        case PopWebRequestMethods.Top:  return GetTopResponse();
        case PopWebRequestMethods.Dele: return GetDeleResponse();
        case PopWebRequestMethods.List: return GetListResponse();
        case PopWebRequestMethods.Uidl: return GetUidlResponse();
        default: throw new NotSupportedException(string.Format("unsupported request method: {0}", Method));
      }
    }

    private PopWebResponse GetRetrResponse()
    {
      Stream messageStream;

      var response = new PopWebResponse(Session.Retr(MessageNumber, out messageStream));

      if (response.Result.Failed)
        return response;

      if (DeleteAfterRetrieve)
        Session.Dele(MessageNumber);

      response.RetrievedMessageStream = messageStream;

      return response;
    }

    private PopWebResponse GetTopResponse()
    {
      Stream messageStream;

      var response = new PopWebResponse(Session.Top(MessageNumber, 0, out messageStream));

      if (response.Result.Failed)
        return response;

      response.RetrievedMessageStream = messageStream;

      return response;
    }

    private PopWebResponse GetDeleResponse()
    {
      return new PopWebResponse(Session.Dele(MessageNumber));
    }

    private PopWebResponse GetListResponse()
    {
      PopScanListing scanList;

      var response = new PopWebResponse(Session.List(MessageNumber, out scanList));

      if (response.Result.Failed)
        return response;

      response.ScanLists = new[] {scanList};

      return response;
    }

    private PopWebResponse GetUidlResponse()
    {
      PopUniqueIdListing uniqueIdList;

      var response = new PopWebResponse(Session.Uidl(MessageNumber, out uniqueIdList));

      if (response.Result.Failed)
        return response;

      response.UniqueIdLists = new[] {uniqueIdList};

      return response;
    }
  }
}