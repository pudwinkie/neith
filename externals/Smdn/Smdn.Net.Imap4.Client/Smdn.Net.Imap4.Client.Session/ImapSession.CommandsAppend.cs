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
using System.IO;
#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif

using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;
using Smdn.Net.Imap4.Client.Transaction.BuiltIn;

namespace Smdn.Net.Imap4.Client.Session {
  partial class ImapSession {
    /*
     * transaction methods : authenticated state / APPEND
     */

    /// <summary>sends APPEND command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Append(IImapAppendMessage message, ImapMailbox mailbox)
    {
      return AppendInternal(new[] {message}, false, mailbox);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY extension.
    /// </remarks>
    public ImapCommandResult AppendBinary(IImapAppendMessage message, ImapMailbox mailbox)
    {
      return AppendInternal(new[] {message}, true, mailbox);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support MULTIAPPEND extension.
    /// </remarks>
    public ImapCommandResult AppendMultiple(IEnumerable<IImapAppendMessage> messages, ImapMailbox mailbox)
    {
      return AppendInternal(messages, false, mailbox);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY or MULTIAPPEND extension.
    /// </remarks>
    public ImapCommandResult AppendBinaryMultiple(IEnumerable<IImapAppendMessage> messages, ImapMailbox mailbox)
    {
      return AppendInternal(messages, true, mailbox);
    }

    private ImapCommandResult AppendInternal(IEnumerable<IImapAppendMessage> messages, bool binary, ImapMailbox mailbox)
    {
      ValidateMailboxRelationship(mailbox);

      ImapAppendedUidSet discard;
      ImapMailbox discard2;

      return AppendInternal(messages, binary, mailbox.Name, false, out discard, out discard2);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult Append(IImapAppendMessage message, ImapMailbox mailbox, out ImapAppendedUidSet appendedUids)
    {
      return AppendInternal(new[] {message}, false, mailbox, out appendedUids);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY extension.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult AppendBinary(IImapAppendMessage message, ImapMailbox mailbox, out ImapAppendedUidSet appendedUids)
    {
      return AppendInternal(new[] {message}, true, mailbox, out appendedUids);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support MULTIAPPEND extension.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult AppendMultiple(IEnumerable<IImapAppendMessage> messages, ImapMailbox mailbox, out ImapAppendedUidSet appendedUids)
    {
      return AppendInternal(messages, false, mailbox, out appendedUids);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY or MULTIAPPEND extension.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult AppendBinaryMultiple(IEnumerable<IImapAppendMessage> messages, ImapMailbox mailbox, out ImapAppendedUidSet appendedUids)
    {
      return AppendInternal(messages, true, mailbox, out appendedUids);
    }

    private ImapCommandResult AppendInternal(IEnumerable<IImapAppendMessage> messages, bool binary, ImapMailbox mailbox, out ImapAppendedUidSet appendedUids)
    {
      ValidateMailboxRelationship(mailbox);

      ImapMailbox discard;

      return AppendInternal(messages, binary, mailbox.Name, false, out appendedUids, out discard);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Append(IImapAppendMessage message, string mailboxName)
    {
      return AppendInternal(new[] {message}, false, mailboxName);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY extension.
    /// </remarks>
    public ImapCommandResult AppendBinary(IImapAppendMessage message, string mailboxName)
    {
      return AppendInternal(new[] {message}, true, mailboxName);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support MULTIAPPEND extension.
    /// </remarks>
    public ImapCommandResult AppendMultiple(IEnumerable<IImapAppendMessage> messages, string mailboxName)
    {
      return AppendInternal(messages, false, mailboxName);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY or MULTIAPPEND extension.
    /// </remarks>
    public ImapCommandResult AppendBinaryMultiple(IEnumerable<IImapAppendMessage> messages, string mailboxName)
    {
      return AppendInternal(messages, true, mailboxName);
    }

    private ImapCommandResult AppendInternal(IEnumerable<IImapAppendMessage> messages, bool binary, string mailboxName)
    {
      ImapAppendedUidSet discard;
      ImapMailbox discard2;

      return AppendInternal(messages, binary, mailboxName, false, out discard, out discard2);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult Append(IImapAppendMessage message, string mailboxName, out ImapAppendedUidSet appendedUids)
    {
      return AppendInternal(new[] {message}, false, mailboxName, out appendedUids);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY extension.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult AppendBinary(IImapAppendMessage message, string mailboxName, out ImapAppendedUidSet appendedUids)
    {
      return AppendInternal(new[] {message}, true, mailboxName, out appendedUids);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support MULTIAPPEND extension.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult AppendMultiple(IEnumerable<IImapAppendMessage> messages, string mailboxName, out ImapAppendedUidSet appendedUids)
    {
      return AppendInternal(messages, false, mailboxName, out appendedUids);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY or MULTIAPPEND extension.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult AppendBinaryMultiple(IEnumerable<IImapAppendMessage> messages, string mailboxName, out ImapAppendedUidSet appendedUids)
    {
      return AppendInternal(messages, true, mailboxName, out appendedUids);
    }

    private ImapCommandResult AppendInternal(IEnumerable<IImapAppendMessage> messages, bool binary, string mailboxName, out ImapAppendedUidSet appendedUids)
    {
      ImapMailbox discard;

      return AppendInternal(messages, binary, mailboxName, false, out appendedUids, out discard);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method tries to automatically create the mailbox when server sent [TRYCREATE] response code.
    /// </remarks>
    public ImapCommandResult Append(IImapAppendMessage message, string mailboxName, out ImapMailbox createdMailbox)
    {
      return AppendInternal(new[] {message}, false, mailboxName, out createdMailbox);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY extension.
    /// this method tries to automatically create the mailbox when server sent [TRYCREATE] response code.
    /// </remarks>
    public ImapCommandResult AppendBinary(IImapAppendMessage message, string mailboxName, out ImapMailbox createdMailbox)
    {
      return AppendInternal(new[] {message}, true, mailboxName, out createdMailbox);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support MULTIAPPEND extension.
    /// this method tries to automatically create the mailbox when server sent [TRYCREATE] response code.
    /// </remarks>
    public ImapCommandResult AppendMultiple(IEnumerable<IImapAppendMessage> messages, string mailboxName, out ImapMailbox createdMailbox)
    {
      return AppendInternal(messages, false, mailboxName, out createdMailbox);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY or MULTIAPPEND extension.
    /// this method tries to automatically create the mailbox when server sent [TRYCREATE] response code.
    /// </remarks>
    public ImapCommandResult AppendBinaryMultiple(IEnumerable<IImapAppendMessage> messages, string mailboxName, out ImapMailbox createdMailbox)
    {
      return AppendInternal(messages, true, mailboxName, out createdMailbox);
    }

    private ImapCommandResult AppendInternal(IEnumerable<IImapAppendMessage> messages, bool binary, string mailboxName, out ImapMailbox createdMailbox)
    {
      ImapAppendedUidSet discard;

      return AppendInternal(messages, binary, mailboxName, true, out discard, out createdMailbox);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method tries to automatically create the mailbox when server sent [TRYCREATE] response code.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult Append(IImapAppendMessage message, string mailboxName, out ImapAppendedUidSet appendedUids, out ImapMailbox createdMailbox)
    {
      return AppendInternal(new[] {message}, false, mailboxName, out appendedUids, out createdMailbox);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY extension.
    /// this method tries to automatically create the mailbox when server sent [TRYCREATE] response code.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult AppendBinary(IImapAppendMessage message, string mailboxName, out ImapAppendedUidSet appendedUids, out ImapMailbox createdMailbox)
    {
      return AppendInternal(new[] {message}, true, mailboxName, out appendedUids, out createdMailbox);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support MULTIAPPEND extension.
    /// this method tries to automatically create the mailbox when server sent [TRYCREATE] response code.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult AppendMultiple(IEnumerable<IImapAppendMessage> messages, string mailboxName, out ImapAppendedUidSet appendedUids, out ImapMailbox createdMailbox)
    {
      return AppendInternal(messages, false, mailboxName, out appendedUids, out createdMailbox);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY or MULTIAPPEND extension.
    /// this method tries to automatically create the mailbox when server sent [TRYCREATE] response code.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult AppendBinaryMultiple(IEnumerable<IImapAppendMessage> messages, string mailboxName, out ImapAppendedUidSet appendedUids, out ImapMailbox createdMailbox)
    {
      return AppendInternal(messages, true, mailboxName, out appendedUids, out createdMailbox);
    }

    private ImapCommandResult AppendInternal(IEnumerable<IImapAppendMessage> messages, bool binary, string mailboxName, out ImapAppendedUidSet appendedUids, out ImapMailbox createdMailbox)
    {
      return AppendInternal(messages, binary, mailboxName, true, out appendedUids, out createdMailbox);
    }

    private ImapCommandResult AppendInternal(IEnumerable<IImapAppendMessage> messages, bool binary, string mailboxName, bool tryCreate, out ImapAppendedUidSet appendedUids, out ImapMailbox createdMailbox)
    {
      RejectNonAuthenticatedState();

      if (messages == null)
        throw new ArgumentNullException("messages");

      var argMailboxName = ImapMailboxNameString.CreateMailboxName(mailboxName);

      appendedUids = null;
      createdMailbox = null;

      // append message
      var messagesToUpload = new List<ImapString>();
      var messageCount = 0;
      var literalOptions = ImapLiteralOptions.NonSynchronizingIfCapable
                           | (binary ? ImapLiteralOptions.Literal8 : ImapLiteralOptions.Literal);

      foreach (var message in messages) {
        if (message == null)
          throw new ArgumentException("contains null", "messages");

        /*
         * RFC 4466 - Collected Extensions to IMAP4 ABNF
         * http://tools.ietf.org/html/rfc4466
         * 
         *    append          = "APPEND" SP mailbox 1*append-message
         *                      ;; only a single append-message may appear
         *                      ;; if MULTIAPPEND [MULTIAPPEND] capability
         *                      ;; is not present
         *    append-message  = append-opts SP append-data
         *    append-ext      = append-ext-name SP append-ext-value
         *                      ;; This non-terminal define extensions to
         *                      ;; to message metadata.
         *    append-ext-name = tagged-ext-label
         *    append-ext-value= tagged-ext-val
         *                      ;; This non-terminal shows recommended syntax
         *                      ;; for future extensions.
         *    append-data     = literal / literal8 / append-data-ext
         *    append-data-ext = tagged-ext
         *                      ;; This non-terminal shows recommended syntax
         *                      ;; for future extensions,
         *                      ;; i.e., a mandatory label followed
         *                      ;; by parameters.
         *    append-opts     = [SP flag-list] [SP date-time] *(SP append-ext)
         *                      ;; message metadata
         */

        // flag-list
        if (message.Flags != null && 0 < message.Flags.Count)
          messagesToUpload.Add(new ImapParenthesizedString(message.Flags.GetNonApplicableFlagsRemoved().ToArray()));

        // date-time
        if (message.InternalDate.HasValue)
          messagesToUpload.Add(ImapDateTimeFormat.ToDateTimeString(message.InternalDate.Value));

        // append-data
        messagesToUpload.Add(new ImapLiteralStream(message.GetMessageStream(),
                                                   literalOptions));

        messageCount++;
      }

      if (messageCount == 0)
        throw new ArgumentException("at least 1 message must be specified", "messages");

      ImapCommandResult failedResult = null;

      for (var i = 0; i < 2; i++) {
        var respTryCreate = false;

        using (var t = new AppendTransaction(connection, 1 < messageCount)) {
          // mailbox name
          t.RequestArguments["mailbox name"] = argMailboxName;

          // messages to upload
          t.RequestArguments["messages to upload"] = new ImapStringList(messagesToUpload.ToArray());

          if (ProcessTransaction(t).Succeeded) {
            appendedUids = t.Result.Value;
            return t.Result;
          }
          else {
            if (ProcessMailboxRefferalResponse(t.Result.TaggedStatusResponse) || !tryCreate)
              return t.Result;
          }

          failedResult = t.Result;

          // 6.3.11. APPEND Command
          //       If the destination mailbox does not exist, a server MUST return an
          //       error, and MUST NOT automatically create the mailbox.  Unless it
          //       is certain that the destination mailbox can not be created, the
          //       server MUST send the response code "[TRYCREATE]" as the prefix of
          //       the text of the tagged NO response.  This gives a hint to the
          //       client that it can attempt a CREATE command and retry the APPEND
          //       if the CREATE is successful.
          respTryCreate = (t.Result.GetResponseCode(ImapResponseCode.TryCreate) is ImapTaggedStatusResponse);
        }

        // try create
        if (i == 0 && respTryCreate)
          if (Create(mailboxName, out createdMailbox).Failed)
            return failedResult;
      }

      return failedResult;
    }

    public abstract class AppendContext {
      protected internal AppendContext() {}

      public abstract Stream WriteStream { get; }

      public abstract ImapCommandResult GetResult(out ImapAppendedUidSet appendedUidSet);
    }

    /// <summary>prepares to send APPEND command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public AppendContext PrepareAppend(long? length, DateTimeOffset? internalDate, IImapMessageFlagSet flags, ImapMailbox mailbox)
    {
      ValidateMailboxRelationship(mailbox);

      return PrepareAppend(length, internalDate, flags, mailbox.Name);
    }

    /// <summary>prepares to send APPEND command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public AppendContext PrepareAppend(long? length, DateTimeOffset? internalDate, IImapMessageFlagSet flags, string mailboxName)
    {
      RejectNonAuthenticatedState();
      RejectTransactionProceeding();

      var argMailboxName = ImapMailboxNameString.CreateMailboxName(mailboxName);

      // message buffer
      var appendBuffer = new ImapAppendMessageBodyBuffer(this,
                                                         length,
                                                         ReceiveTimeout,
                                                         SendTimeout);

      // append message
      var messagesToUpload = new List<ImapString>(1);

      // flag-list
      if (flags != null && 0 < flags.Count)
        messagesToUpload.Add(new ImapParenthesizedString(flags.GetNonApplicableFlagsRemoved().ToArray()));

      // date-time
      if (internalDate.HasValue)
        messagesToUpload.Add(ImapDateTimeFormat.ToDateTimeString(internalDate.Value));

      // append-data
      messagesToUpload.Add(new ImapLiteralStream(appendBuffer.ReadStream, ImapLiteralOptions.Synchronizing));

      AppendTransaction t = null;

      try {
        t = new AppendTransaction(connection, false);

        // mailbox name
        t.RequestArguments["mailbox name"] = argMailboxName;

        // messages to upload
        t.RequestArguments["messages to upload"] = new ImapStringList(messagesToUpload.ToArray());

        var asyncResult = BeginProcessTransaction(t, handlesIncapableAsException);

        // wait for started (or completed)
        for (;;) {
          if (asyncResult.IsCompleted)
            break;
          else if (IsTransactionProceeding)
            break;
          else
            System.Threading.Thread.Sleep(10);
        }

        appendBuffer.SetAppendAsyncResult(asyncResult);

        return appendBuffer;
      }
      catch {
        if (t != null) {
          t.Dispose();
          t = null;
        }

        throw;
      }
    }

    /// <summary>finishes to send APPEND command</summary>
    /// <remarks>valid in authenticated state</remarks>
    internal ImapCommandResult EndAppend(IAsyncResult asyncResult, out ImapAppendedUidSet appendedUid)
    {
      appendedUid = null;

      if (asyncResult == null)
        throw new ArgumentNullException("asyncResult");

      var appendAsyncResult = asyncResult as TransactionAsyncResult;

      if (appendAsyncResult == null)
        throw ExceptionUtils.CreateArgumentMustBeValidIAsyncResult("asyncResult");

      using (var t = appendAsyncResult.Transaction as AppendTransaction) {
        if (EndProcessTransaction(appendAsyncResult).Succeeded)
          appendedUid = t.Result.Value;
        else
          ProcessMailboxRefferalResponse(t.Result.TaggedStatusResponse);

        return t.Result;
      }
    }
  }
}
