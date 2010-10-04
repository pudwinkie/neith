// 
// Author:
//       smdn <smdn@mail.invisiblefulmoon.net>
// 
// Copyright (c) 2010 smdn
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
using System.Runtime.Serialization;

namespace Smdn.Net.Pop3.WebClients {
  // TODO: [Serializable]
  internal sealed class PopMailboxWebRequest : PopWebRequest {
    public PopMailboxWebRequest(Uri requestUri, PopSessionManager sessionManager)
      : base(requestUri, PopWebRequestMethods.List, sessionManager)
    {
    }

    protected PopMailboxWebRequest(SerializationInfo serializationInfo, StreamingContext streamingContext)
      : base(serializationInfo, streamingContext)
    {
    }

    protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
      base.GetObjectData(serializationInfo, streamingContext);
    }

    protected override PopWebResponse InternalGetResponse()
    {
      switch (Method) {
        case PopWebRequestMethods.List: return GetListResponse();
        case PopWebRequestMethods.Uidl: return GetUidlResponse();
        case PopWebRequestMethods.Rset: return GetRsetResponse();
        case PopWebRequestMethods.Stat: return GetStatResponse();
        default: throw new NotSupportedException(string.Format("unsupported request method: {0}", Method));
      }
    }

    private PopWebResponse GetListResponse()
    {
      PopScanListing[] scanLists;

      var response = new PopWebResponse(Session.List(out scanLists));

      if (response.Result.Failed)
        return response;

      response.ScanLists = scanLists;

      return response;
    }

    private PopWebResponse GetUidlResponse()
    {
      PopUniqueIdListing[] uniqueIdLists;

      var response = new PopWebResponse(Session.Uidl(out uniqueIdLists));

      if (response.Result.Failed)
        return response;

      response.UniqueIdLists = uniqueIdLists;

      return response;
    }

    private PopWebResponse GetRsetResponse()
    {
      return new PopWebResponse(Session.Rset());
    }

    private PopWebResponse GetStatResponse()
    {
      PopDropListing dropList;

      var response = new PopWebResponse(Session.Stat(out dropList));

      if (response.Result.Failed)
        return response;

      response.DropList = dropList;

      return response;
    }
  }
}
