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
using System.Collections.Generic;

using Smdn.Net.Pop3.Protocol.Client;

namespace Smdn.Net.Pop3.Client {
  partial class PopClient {
    private const bool defaultGetUniqueId = false;

    public bool DeleteAfterRetrieve {
      get; set;
    }

    public long MessageCount {
      get { ThrowIfNotConnected(); return EnsureDropListUpdated().MessageCount; }
    }

    public long TotalSize {
      get { ThrowIfNotConnected(); return EnsureDropListUpdated().SizeInOctets; }
    }

    private PopDropListing EnsureDropListUpdated()
    {
      if (dropListing.HasValue)
        return dropListing.Value;

      PopDropListing dropList;

      ThrowIfError(Session.Stat(out dropList));

      dropListing = dropList;

      return dropListing.Value;
    }

    public IEnumerable<PopMessageInfo> GetMessages()
    {
      return GetMessages(defaultGetUniqueId);
    }

    public IEnumerable<PopMessageInfo> GetMessages(bool getUniqueId)
    {
      PopScanListing[] scanLists;
      Dictionary<long, PopUniqueIdListing> uidLists = null;

      ThrowIfError(Session.List(out scanLists));

      if (getUniqueId && 0 < scanLists.Length) {
        PopUniqueIdListing[] uids = null;

        ThrowIfError(Session.Uidl(out uids));

        uidLists = new Dictionary<long, PopUniqueIdListing>();

        foreach (var uid in uids) {
          uidLists.Add(uid.MessageNumber, uid);
        }
      }

      foreach (var scanList in scanLists) {
        PopUniqueIdListing uniqueIdList;

        if (getUniqueId && uidLists.TryGetValue(scanList.MessageNumber, out uniqueIdList))
          yield return ToMessage(scanList, uniqueIdList);
        else
          yield return ToMessage(scanList, null);
      }
    }

    public PopMessageInfo GetMessage(long messageNumber)
    {
      return GetMessage(messageNumber, defaultGetUniqueId);
    }

    public PopMessageInfo GetMessage(long messageNumber, bool getUniqueId)
    {
      if (messageNumber <= 0L)
        throw new ArgumentOutOfRangeException("messageNumber", messageNumber, "must be non-zero positive number");

      /*
       * find exist
       */
      var message = FindMessageByMessageNumber(messageNumber);

      if (message != null) {
        message.ThrowIfMakredAsDeleted();

        if (!getUniqueId || message.UniqueIdList != null)
          return message;
      }

      /*
       * list newly
       */
      if (MessageCount < messageNumber)
        throw new ArgumentOutOfRangeException("messageNumber",
                                              messageNumber,
                                              string.Format("invalid message number, only {0} messages in maildrop", MessageCount));

      PopScanListing scanList;

      var result = Session.List(messageNumber, out scanList);

      if (result.Code == PopCommandResultCode.Error)
        throw new PopMessageNotFoundException(messageNumber);
      else
        ThrowIfError(result);

      if (getUniqueId) {
        PopUniqueIdListing uniqueIdList;

        ThrowIfError(Session.Uidl(messageNumber, out uniqueIdList));

        return ToMessage(scanList, uniqueIdList);
      }
      else {
        return ToMessage(scanList, null);
      }
    }

    public PopMessageInfo GetFirstMessage()
    {
      return GetFirstMessage(defaultGetUniqueId);
    }

    public PopMessageInfo GetFirstMessage(bool getUniqueId)
    {
      return GetMessage(1L, getUniqueId);
    }

    public PopMessageInfo GetLastMessage()
    {
      return GetLastMessage(defaultGetUniqueId);
    }

    public PopMessageInfo GetLastMessage(bool getUniqueId)
    {
      return GetMessage(MessageCount, getUniqueId);
    }

    public PopMessageInfo GetMessage(string uniqueId)
    {
      /*
       * find exist
       */
      var message = FindMessageByUniqueId(GetValidatedUniqueId(uniqueId));

      if (message != null) {
        message.ThrowIfMakredAsDeleted();

        return message;
      }

      /*
       * list newly
       */
      PopUniqueIdListing[] uniqueIdLists;

      ThrowIfError(Session.Uidl(out uniqueIdLists));

      foreach (var uniqueIdList in uniqueIdLists) {
        if (!uniqueIdList.Equals(uniqueId))
          continue;

        PopScanListing scanList;

        ThrowIfError(Session.List(uniqueIdList.MessageNumber, out scanList));

        return ToMessage(scanList, uniqueIdList);
      }

      throw new PopMessageNotFoundException(uniqueId);
    }

    private PopMessageInfo FindMessageByMessageNumber(long messageNumber)
    {
      lock (((System.Collections.ICollection)messages).SyncRoot) {
        PopMessageInfo message;

        if (messages.TryGetValue(messageNumber, out message))
          return message;
        else
          return null;
      }
    }

    private PopMessageInfo FindMessageByUniqueId(string uniqueId)
    {
      lock (((System.Collections.ICollection)messages).SyncRoot) {
        foreach (var message in messages.Values) {
          if (message.UniqueIdList.HasValue && message.UniqueIdList.Value.Equals(uniqueId))
            return message;
        }

        return null;
      }
    }

    private PopMessageInfo ToMessage(PopScanListing scanList, PopUniqueIdListing? uniqueIdList)
    {
      lock (((System.Collections.ICollection)messages).SyncRoot) {
        PopMessageInfo message;

        if (messages.TryGetValue(scanList.MessageNumber, out message)) {
          // update values
          message.ScanList = scanList;
          message.UniqueIdList = uniqueIdList ?? message.UniqueIdList;
        }
        else {
          message = new PopMessageInfo(this, scanList, uniqueIdList);

          messages[message.MessageNumber] = message;
        }

        return message;
      }
    }

    public void CancelDelete()
    {
      ThrowIfError(Session.Rset());

      dropListing = null; // must be updated

      lock (((System.Collections.ICollection)messages).SyncRoot) {
        foreach (var message in messages.Values) {
          message.IsMarkedAsDeleted = false;
        }
      }
    }

    public void KeepAlive()
    {
      ThrowIfError(Session.NoOp());
    }

    internal void Delete(PopMessageInfo message)
    {
      ThrowIfError(Session.Dele(message.MessageNumber));

      dropListing = null; // must be updated
    }

    /*
     * utilities
     */
    internal static string GetValidatedUniqueId(string uniqueId)
    {
      if (uniqueId == null)
        throw new ArgumentNullException("uniqueId");
      else if (uniqueId.Length == 0)
        throw new ArgumentException("must be non-empty string", "uniqueId");

      return uniqueId;
    }

    internal static void ThrowIfError(PopCommandResult result)
    {
      if (result.Failed)
        throw new PopErrorResponseException(result);
    }

    private PopDropListing? dropListing = null;
    private Dictionary<long, PopMessageInfo> messages = new Dictionary<long, PopMessageInfo>();
  }
}
