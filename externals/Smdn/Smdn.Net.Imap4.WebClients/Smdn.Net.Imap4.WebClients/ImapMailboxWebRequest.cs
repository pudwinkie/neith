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
using System.IO;
using System.Net;
using System.Runtime.Serialization;

using Smdn.Net.Imap4.Client.Session;

namespace Smdn.Net.Imap4.WebClients {
  // TODO: [Serializable]
  internal sealed class ImapMailboxWebRequest : ImapWebRequest {
    public ImapMailboxWebRequest(Uri requestUri, ImapSessionManager sessionManager)
      : base(requestUri, ImapWebRequestMethods.Fetch, sessionManager)
    {
    }

    protected ImapMailboxWebRequest(SerializationInfo serializationInfo, StreamingContext streamingContext)
      : base(serializationInfo, streamingContext)
    {
    }

    protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
      base.GetObjectData(serializationInfo, streamingContext);
    }

    private bool IsRequestAppend {
      get { return string.Equals(ImapWebRequestMethods.Append, Method, StringComparison.OrdinalIgnoreCase); }
    }

    protected override ImapWebResponse InternalGetResponse()
    {
      switch (Method) {
        case ImapWebRequestMethods.Fetch:       return GetFetchResponse();
        case ImapWebRequestMethods.Append:      return GetAppendResponse();
        case ImapWebRequestMethods.Create:      return GetCreateResponse();
        case ImapWebRequestMethods.Rename:      return GetRenameResponse();
        case ImapWebRequestMethods.Delete:      return GetDeleteResponse();
        case ImapWebRequestMethods.Expunge:     return GetExpungeResponse();
        case ImapWebRequestMethods.Select:      return GetSelectExamineResponse(false);
        case ImapWebRequestMethods.Examine:     return GetSelectExamineResponse(true);
        case ImapWebRequestMethods.Status:      return GetStatusResponse();
        case ImapWebRequestMethods.Subscribe:   return GetSubscribeUnsubscribeResponse(true);
        case ImapWebRequestMethods.Unsubscribe: return GetSubscribeUnsubscribeResponse(false);
        case ImapWebRequestMethods.Check:       return GetCheckResponse();
        default: throw new NotSupportedException(string.Format("unsupported request method: {0}", Method));
      }
    }

    private ImapWebResponse GetFetchResponse()
    {
      var response = SelectRequestMailbox();

      if (response != null && response.Result.Failed)
        return response;

      ImapMessageAttribute[] messageAttributes;

      response = new ImapWebResponse(Session.Fetch(ImapSequenceSet.CreateUidFromSet(1),
                                                   GetFetchDataItem(),
                                                   out messageAttributes));

      if (response.Result.Succeeded)
        response.MessageAttributes = messageAttributes;

      return response;
    }

#region "APPEND"
    private delegate Stream GetRequestStreamDelegate();

    public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
    {
      CheckRequestStarted();

      if (!IsRequestAppend)
        throw new ProtocolViolationException("request method is not APPEND");

      beginGetRequestStreamAsyncResult = (new GetRequestStreamDelegate(GetRequestStreamProc)).BeginInvoke(callback, state);

      return beginGetRequestStreamAsyncResult;
    }

    public override Stream EndGetRequestStream(IAsyncResult asyncResult)
    {
      var ar = asyncResult as System.Runtime.Remoting.Messaging.AsyncResult;

      if (ar != beginGetRequestStreamAsyncResult)
        throw new ArgumentException("invalid IAsyncResult", "asyncResult");

      try {
        return (ar.AsyncDelegate as GetRequestStreamDelegate).EndInvoke(ar);
      }
      finally {
        beginGetRequestStreamAsyncResult = null;
      }
    }

    private Stream GetRequestStreamProc()
    {
      var session = GetSession();

      CloseMailbox(); // ignore error

      appendMessageBodyStream = new ImapWebClientAppendMessageBodyStream(ReadWriteTimeout);

      // Content-Length
      if (0 < ContentLength)
        appendMessageBodyStream.SetLength(ContentLength);

      // TODO: TRYCREATE when AllowCreateMailbox == true
      beginAppendAsyncResult = session.BeginAppend(appendMessageBodyStream, DateTimeOffset.Now, null, RequestMailbox);

      return appendMessageBodyStream;
    }

    private ImapWebResponse GetAppendResponse()
    {
      if (beginAppendAsyncResult == null)
        throw new InvalidOperationException("GetRequestStream not called");

      try {
        appendMessageBodyStream.UpdateLength();

        ImapAppendedUidSet appendedUid;

        var response = new ImapWebResponse(Session.EndAppend(beginAppendAsyncResult, out appendedUid));

        if (response.Result.Succeeded) {
          // set empty stream; WebClient.Upload*() methods call WebResponse.GetResponseStream
          response.SetResponseStream(Stream.Null, 0L, false);

          if (appendedUid != null) {
            var builder = new ImapUriBuilder(RequestUri);

            builder.UidValidity = appendedUid.UidValidity;
            builder.Uid = appendedUid.ToNumber(); // if appendedUid.IsSingle

            response.SetResponseUri(builder.Uri);
          }
        }

        return response;
      }
      finally {
        appendMessageBodyStream.InternalDispose();
        appendMessageBodyStream = null;

        beginAppendAsyncResult = null;
      }
    }

    private IAsyncResult beginGetRequestStreamAsyncResult = null;
    private ImapWebClientAppendMessageBodyStream appendMessageBodyStream = null;
#endregion

    private ImapWebResponse GetCreateResponse()
    {
      CloseMailbox(); // ignore error

      ImapMailbox createdMailbox;

      var response = new ImapWebResponse(Session.Create(RequestMailbox, out createdMailbox));

      if (response.Result.Failed)
        return response;

      if (Subscription) {
        Session.Subscribe(createdMailbox);
        Session.Lsub(createdMailbox);
      }
      else {
        Session.List(createdMailbox);
      }

      response.Mailboxes = new[] {createdMailbox};
      response.SetResponseUri(createdMailbox.Url);

      return response;
    }

    private ImapWebResponse GetRenameResponse()
    {
      CloseMailbox(); // ignore error

      ImapMailbox renamedMailbox;

      var mailbox = RequestMailbox;
      var response = new ImapWebResponse(Session.Rename(mailbox, GetDestinationMailbox(), out renamedMailbox));

      if (response.Result.Failed)
        return response;

      if (Subscription) {
        Session.Unsubscribe(mailbox);
        Session.Subscribe(renamedMailbox);
        Session.Lsub(renamedMailbox);
      }
      else {
        Session.List(renamedMailbox);
      }

      // unsubscribe/subscribe children
      if (Subscription && !string.IsNullOrEmpty(renamedMailbox.HierarchyDelimiter)) {
        var wildcard = renamedMailbox.HierarchyDelimiter + "*";
        ImapMailbox[] children;

        if (Session.Lsub(mailbox + wildcard, out children).Succeeded) {
          foreach (var child in children) {
            Session.Unsubscribe(child);
          }
        }

        if (Session.List(renamedMailbox.Name + wildcard, out children).Succeeded) {
          foreach (var child in children) {
            Session.Subscribe(child);
          }
        }
      }

      response.Mailboxes = new[] {renamedMailbox};
      response.SetResponseUri(renamedMailbox.Url);

      return response;
    }

    private ImapWebResponse GetDeleteResponse()
    {
      CloseMailbox(); // ignore error

      var mailbox = RequestMailbox;
      var response = new ImapWebResponse(Session.Delete(mailbox));

      if (response.Result.Failed)
        return response;

      if (Subscription)
        Session.Unsubscribe(mailbox);

      return response;
    }

    private ImapWebResponse GetExpungeResponse()
    {
      var response = SelectRequestMailbox();

      if (response != null && response.Result.Failed)
        return response;

      return new ImapWebResponse(Session.Expunge());
    }

    private ImapWebResponse GetSelectExamineResponse(bool readOnly)
    {
      var response = SelectRequestMailbox(readOnly);

      if (response != null && response.Result.Succeeded) {
        response.Mailboxes = new[] {Session.SelectedMailbox};
        response.SetResponseUri(Session.SelectedMailbox.Url);
      }

      return response;
    }

    private ImapWebResponse GetStatusResponse()
    {
      if (StatusDataItem == null)
        throw new InvalidOperationException("StatusDataItem must be set");

      ImapMailbox statusMailbox;

      var response = new ImapWebResponse(Session.Status(RequestMailbox, StatusDataItem, out statusMailbox));

      if (response.Result.Succeeded)
        response.Mailboxes = new ImapMailbox[] {statusMailbox};

      return response;
    }

    private ImapWebResponse GetSubscribeUnsubscribeResponse(bool subscribe)
    {
      CloseMailbox(); // ignore error

      return new ImapWebResponse(subscribe ? Session.Subscribe(RequestMailbox) : Session.Unsubscribe(RequestMailbox));
    }

    private ImapWebResponse GetCheckResponse()
    {
      var response = SelectRequestMailbox();

      if (response != null && response.Result.Failed)
        return response;

      return new ImapWebResponse(Session.Check());
    }
  }
}
