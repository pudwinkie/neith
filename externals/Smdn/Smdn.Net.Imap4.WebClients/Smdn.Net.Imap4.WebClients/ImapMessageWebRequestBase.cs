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

namespace Smdn.Net.Imap4.WebClients {
  // TODO: [Serializable]
  internal abstract class ImapMessageWebRequestBase : ImapWebRequest {
    protected ImapMessageWebRequestBase(Uri requestUri, string defaultMethod, ImapSessionManager sessionManager)
      : base(requestUri, defaultMethod, sessionManager)
    {
    }

    protected ImapMessageWebRequestBase(SerializationInfo serializationInfo, StreamingContext streamingContext)
      : base(serializationInfo, streamingContext)
    {
    }

    protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
      base.GetObjectData(serializationInfo, streamingContext);
    }

    protected ImapWebResponse GetExpungeResponse(ImapSequenceSet requestUidSet)
    {
      var result = Session.Store(requestUidSet, ImapStoreDataItem.AddFlagsSilent(ImapMessageFlag.Deleted));

      if (result.Failed)
        return new ImapWebResponse(result);

      if (Session.ServerCapabilities.Contains(ImapCapability.UidPlus))
        result = Session.UidExpunge(requestUidSet);
      else
        result = Session.Expunge();

      return new ImapWebResponse(result);
    }

    protected ImapWebResponse GetCopyResponse(ImapSequenceSet requestUidSet)
    {
      ImapCopiedUidSet copiedUids;
      ImapMailbox createdMailbox = null;

      var mailbox = GetDestinationMailbox();
      var response = new ImapWebResponse(AllowCreateMailbox
                                         ? Session.Copy(requestUidSet, mailbox, out copiedUids, out createdMailbox)
                                         : Session.Copy(requestUidSet, mailbox, out copiedUids));

      if (response.Result.Succeeded) {
        if (createdMailbox != null) {
          if (Subscription) {
            Session.Subscribe(createdMailbox);
            Session.Lsub(createdMailbox);
          }
          else {
            Session.List(createdMailbox);
          }

          response.Mailboxes = new[] {createdMailbox};
        }

        if (copiedUids != null) {
          var builder = new ImapUriBuilder(DestinationUri);

          builder.UidValidity = copiedUids.UidValidity;

          if (copiedUids.AssignedUidSet.IsSingle)
            builder.Uid = copiedUids.AssignedUidSet.ToNumber();
          else
            builder.SearchCriteria = ImapSearchCriteria.Uid(copiedUids.AssignedUidSet);

          response.SetResponseUri(builder.Uri);
        }
      }

      return response;
    }

    protected ImapWebResponse GetStoreResponse(ImapSequenceSet requestUidSet)
    {
      if (StoreDataItem == null)
        throw new InvalidOperationException("StoreDataItem must be set");

      return new ImapWebResponse(Session.Store(requestUidSet, StoreDataItem));
    }
  }
}
