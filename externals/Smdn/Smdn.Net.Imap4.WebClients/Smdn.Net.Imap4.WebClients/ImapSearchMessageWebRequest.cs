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
using System.Text;
using System.Runtime.Serialization;

using Smdn.Net.Imap4.Client.Session;
using Smdn.Net.Imap4.Client.Transaction;
using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.WebClients {
  // TODO: [Serializable]
  internal sealed class ImapSearchMessageWebRequest : ImapMessageWebRequestBase {
    public ImapSearchMessageWebRequest(Uri requestUri, ImapSessionManager sessionManager)
      : base(requestUri, ImapWebRequestMethods.Search, sessionManager)
    {
    }

    protected ImapSearchMessageWebRequest(SerializationInfo serializationInfo, StreamingContext streamingContext)
      : base(serializationInfo, streamingContext)
    {
    }

    protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
      base.GetObjectData(serializationInfo, streamingContext);
    }

    private delegate ImapWebResponse GetResponseDelegate(ImapMatchedSequenceSet matched);

    protected override ImapWebResponse InternalGetResponse()
    {
      var response = SelectRequestMailbox();

      if (response != null && response.Result.Failed)
        return response;

      GetResponseDelegate getResponse = null;

      switch (Method) {
        case ImapWebRequestMethods.Search:  getResponse = GetSearchResponse; break;
        case ImapWebRequestMethods.Sort:    return GetSortResponse();
        case ImapWebRequestMethods.Thread:  return GetThreadResponse();
        case ImapWebRequestMethods.Copy:    getResponse = GetCopyResponse; break;
        case ImapWebRequestMethods.Expunge: getResponse = GetExpungeResponse; break;
        case ImapWebRequestMethods.Store:   getResponse = GetStoreResponse; break;
        default: throw new NotSupportedException(string.Format("unsupported request method: {0}", Method));
      }

      ImapCommandResult result;
      ImapMatchedSequenceSet matched;

      if (Session.ServerCapabilities.Contains(ImapCapability.Searchres))
        // use SEARCHRES extension
        result = Session.UidESearch(GetSearchCriteria(Session.ServerCapabilities),
                                    null,
                                    ImapSearchResultOptions.Save,
                                    out matched);
      else
        result = Session.UidSearch(GetSearchCriteria(Session.ServerCapabilities),
                                   out matched);

      if (result.Failed)
        return CreateSearchErrorResponse(result);
      else if (matched.IsEmpty)
        return CreateNotMatchedResponse(result);
      else
        return getResponse(matched);
    }

    private static ImapWebResponse CreateNotMatchedResponse(ImapCommandResult result)
    {
      var response = new ImapWebResponse(result);

      response.MessageAttributes = new IImapMessageAttribute[] {};

      return response;
    }

    private static ImapWebResponse CreateSearchErrorResponse(ImapCommandResult result)
    {
      var response = new ImapWebResponse(result);

      if (result.TaggedStatusResponse.ResponseText.Code == ImapResponseCode.BadCharset) {
        var supportedCharsets = new List<Encoding>();

        foreach (var charset in ImapResponseTextConverter.FromBadCharset(result.TaggedStatusResponse.ResponseText)) {
          try {
            supportedCharsets.Add(Encoding.GetEncoding(charset));
          }
          catch (ArgumentException) {
            // not supported by framework
          }
        }

        response.SupportedCharsets = supportedCharsets.ToArray();
      }

      return response;
    }

    private ImapWebResponse GetSearchResponse(ImapMatchedSequenceSet matched)
    {
      ImapMessageAttribute[] messageAttributes;

      var response = new ImapWebResponse(Session.Fetch(matched,
                                                       GetFetchDataItem(),
                                                       out messageAttributes));

      if (response.Result.Succeeded)
        response.MessageAttributes = messageAttributes;

      return response;
    }

    private ImapWebResponse GetSortResponse()
    {
      if (SortCriteria == null)
        throw new InvalidOperationException("SortCriteria must be set");

      string charset;
      ImapMatchedSequenceSet matched;

      var result = Session.UidSortPreformatted(SortCriteria,
                                               GetSearchCriteria(Session.ServerCapabilities, out charset),
                                               charset,
                                               out matched);

      if (result.Failed)
        return CreateSearchErrorResponse(result);
      else if (matched.IsEmpty)
        return CreateNotMatchedResponse(result);

      ImapMessageAttribute[] messageAttributes;

      var response = new ImapWebResponse(Session.Fetch(matched,
                                                       GetFetchDataItem(),
                                                       out messageAttributes));

      if (response.Result.Failed)
        return response;

      var sortedUids = matched.ToArray();
      var sortedMessageAttributes = new IImapMessageAttribute[sortedUids.Length];

      for (var i = 0; i < sortedUids.Length; i++) {
        sortedMessageAttributes[i] = null;

        for (var j = 0; j < messageAttributes.Length; j++) {
          if (sortedUids[i] == messageAttributes[j].Uid) {
            sortedMessageAttributes[i] = messageAttributes[j];
            break;
          }
        }
      }

      response.MessageAttributes = sortedMessageAttributes;

      return response;
    }

    private ImapWebResponse GetThreadResponse()
    {
      if (ThreadingAlgorithm == null)
        throw new InvalidOperationException("ThreadingAlgorithm must be set");

      string charset;
      ImapThreadList threadList;

      var result = Session.UidThreadPreformatted(ThreadingAlgorithm,
                                                 GetSearchCriteria(Session.ServerCapabilities, out charset),
                                                 charset,
                                                 out threadList);

      if (result.Failed)
        return CreateSearchErrorResponse(result);
      else if (threadList.Children.Length == 0)
        return CreateNotMatchedResponse(result);

      ImapMessageAttribute[] messageAttributes;

      var response = new ImapWebResponse(Session.Fetch(threadList.ToSequenceSet(),
                                                       GetFetchDataItem(),
                                                       out messageAttributes));

      if (response.Result.Failed)
        return response;

      response.MessageAttributes = messageAttributes;

      var messageDictionary = new Dictionary<long, IImapMessageAttribute>();

      foreach (var messageAttr in messageAttributes) {
        messageDictionary.Add(messageAttr.Uid, messageAttr);
      }

      response.ThreadTree = new ImapThreadTree(true, null, CreateMessageTree(threadList.Children, messageDictionary));

      return response;
    }

    private ImapThreadTree[] CreateMessageTree(ImapThreadList[] children, Dictionary<long, IImapMessageAttribute> messageAttributes)
    {
      return Array.ConvertAll(children, delegate(ImapThreadList list) {
        return new ImapThreadTree(false, messageAttributes[list.Number], CreateMessageTree(list.Children, messageAttributes));
      });
    }
  }
}
