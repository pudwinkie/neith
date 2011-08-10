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
using System.Net;
using System.Runtime.Serialization;

using Smdn.Net.Imap4.Client.Session;

namespace Smdn.Net.Imap4.WebClients {
  // TODO: [Serializable]
  internal sealed class ImapFetchMessageWebRequest : ImapMessageWebRequestBase {
    public ImapFetchMessageWebRequest(Uri requestUri, ImapSessionManager sessionManager)
      : base(requestUri, ImapWebRequestMethods.Fetch, sessionManager)
    {
    }

    protected ImapFetchMessageWebRequest(SerializationInfo serializationInfo, StreamingContext streamingContext)
      : base(serializationInfo, streamingContext)
    {
    }

    protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
      base.GetObjectData(serializationInfo, streamingContext);
    }

    protected override ImapWebResponse InternalGetResponse()
    {
      var response = SelectRequestMailbox();

      if (response != null && response.Result.Failed)
        return response;

      var requestedUidSet = ImapSequenceSet.FromUri(RequestUri);

      switch (Method) {
        case ImapWebRequestMethods.Fetch:   return GetFetchResponse(requestedUidSet);
        case ImapWebRequestMethods.Expunge: return GetExpungeResponse(requestedUidSet);
        case ImapWebRequestMethods.Copy:    return GetCopyResponse(requestedUidSet);
        case ImapWebRequestMethods.Store:   return GetStoreResponse(requestedUidSet);
        default: throw new NotSupportedException(string.Format("unsupported request method: {0}", Method));
      }
    }

    private ImapWebResponse GetFetchResponse(ImapSequenceSet requestedUidSet)
    {
      var section = ImapStyleUriParser.GetSection(RequestUri);
      var partial = ImapStyleUriParser.GetPartial(RequestUri);

      if (string.IsNullOrEmpty(section) && partial == null)
        return GetFullFetchResponse(requestedUidSet);
      else
        return GetPartialFetchResponse(requestedUidSet, section, partial);
    }

    private ImapWebResponse GetFullFetchResponse(ImapSequenceSet requestedUidSet)
    {
      var responseStream = new ImapWebClientFetchMessageBodyStream(Session,
                                                                   KeepAlive,
                                                                   FetchPeek,
                                                                   requestedUidSet,
                                                                   FetchBlockSize,
                                                                   ReadWriteTimeout);

      IImapMessageAttribute messageAttr;
      var response = new ImapWebResponse(responseStream.Prepare(GetFetchDataItem(), out messageAttr));

      if (response.Result.Failed)
        return response;

      response.MessageAttributes = new[] {messageAttr};
      response.SetResponseStream(responseStream,
                                 responseStream.Length,
                                 true);

      if (messageAttr.BodyStructure != null)
        response.SetContentType((string)messageAttr.BodyStructure.MediaType);

      return response;
    }

    private ImapWebResponse GetPartialFetchResponse(ImapSequenceSet requestedUidSet, string section, ImapPartialRange? partial)
    {
      var responseStream = new ImapWebClientFetchMessageBodyStream(Session,
                                                                   KeepAlive,
                                                                   FetchPeek,
                                                                   requestedUidSet,
                                                                   section,
                                                                   partial,
                                                                   FetchBlockSize,
                                                                   ReadWriteTimeout);

      IImapMessageAttribute discard;
      var response = new ImapWebResponse(responseStream.Prepare(GetFetchDataItem(), out discard));

      if (response.Result.Failed)
        return response;

      response.SetResponseStream(responseStream,
                                 responseStream.Length,
                                 true);

      return response;
    }
  }
}
