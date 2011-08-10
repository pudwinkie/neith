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
using System.IO;
using System.Net;

using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Session {
  public class ImapFetchMessageBodyStream : Stream {
    public static readonly int DefaultFetchBlockSize = 10 * 1024;

    public override bool CanSeek {
      get { return /*!IsClosed &&*/ false; }
    }

    public override bool CanRead {
      get { return !IsClosed /*&& true*/; }
    }

    public override bool CanWrite {
      get { return /*!IsClosed &&*/ false; }
    }

    public override bool CanTimeout {
      get { return true; }
    }

    private bool IsClosed {
      get { return session == null; }
    }

    public override long Position {
      get { RejectDisposed(); return position; }
      set { throw ExceptionUtils.CreateNotSupportedSeekingStream(); }
    }

    public override long Length {
      get { RejectDisposed(); return length; }
    }

    public override int ReadTimeout {
      get { RejectDisposed(); return session.ReceiveTimeout; }
      set { RejectDisposed(); session.ReceiveTimeout = value; }
    }

    public override int WriteTimeout {
      get { RejectDisposed(); return session.SendTimeout; }
      set { RejectDisposed(); session.SendTimeout = value; }
    }

    protected ImapSession Session {
      get { return session; }
    }

    internal protected long FetchBlockSize {
      get { return fetchBlockSize; }
    }

    internal protected ImapFetchMessageBodyStream(ImapSession session,
                                                  bool peek,
                                                  ImapSequenceSet fetchUidSet,
                                                  int fetchBlockSize)
      : this(session, peek, fetchUidSet, null, null, fetchBlockSize)
    {
    }

    internal protected ImapFetchMessageBodyStream(ImapSession session,
                                                  bool peek,
                                                  ImapSequenceSet fetchUidSet,
                                                  string fetchSection,
                                                  ImapPartialRange? fetchRange,
                                                  int fetchBlockSize)
    {
      this.session = session;
      this.peek = peek;
      this.fetchUidSet = fetchUidSet;
      this.fetchSection = fetchSection;
      this.fetchRange = fetchRange;
      this.fetchBlockSize = (long)fetchBlockSize;

      this.partialFetch = (!string.IsNullOrEmpty(fetchSection) || fetchRange != null);
    }

    public override void Close()
    {
      Dispose(true);
    }

    protected void DetachFromSession()
    {
      session = null;
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);

      buffer = null;
      session = null;
    }

    public override void SetLength(long value)
    {
      throw ExceptionUtils.CreateNotSupportedSettingStreamLength();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      throw ExceptionUtils.CreateNotSupportedSeekingStream();
    }

    public override void Flush()
    {
      throw ExceptionUtils.CreateNotSupportedWritingStream();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      throw ExceptionUtils.CreateNotSupportedWritingStream();
    }

    public override int Read(byte[] dest, int offset, int count)
    {
      RejectDisposed();

      if (dest == null)
        throw new ArgumentNullException("dest");
      if (offset < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("offset", offset);
      if (count < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("count", count);
      if (dest.Length - count < offset)
        throw ExceptionUtils.CreateArgumentAttemptToAccessBeyondEndOfArray("offset", dest, offset, count);

      var read = 0;

      for (;;) {
        if (bufRemain == 0) {
          if (eof)
            return read;
          else
            FillBuffer();
        }

        if (count <= bufRemain) {
          Buffer.BlockCopy(buffer, bufOffset, dest, offset, count);

          bufOffset += count;
          bufRemain -= count;
          position  += count;

          read += count;

          return read;
        }
        else {
          Buffer.BlockCopy(buffer, bufOffset, dest, offset, bufRemain);

          offset    += bufRemain;
          count     -= bufRemain;
          position  += bufRemain;

          read += bufRemain;

          bufRemain = 0;
        }
      }
    }

    [CLSCompliant(false)]
    internal protected virtual ImapCommandResult Prepare(ImapFetchDataItem fetchDataItemMacro,
                                                         out IImapMessageAttribute messageAttr)
    {
      ImapMessage fetched;

      if (!partialFetch && fetchDataItemMacro == null)
        fetchDataItemMacro = ImapFetchDataItem.Rfc822Size;

      var result = FetchAndFillBuffer(fetchDataItemMacro, out fetched);

      if (result.Succeeded) {
        messageAttr = fetched;

        if (!partialFetch)
          length = fetched.Rfc822Size;
      }
      else {
        messageAttr = null;
      }

      return result;
    }

    private void FillBuffer()
    {
      ImapMessageBody discard; // used as type specifier

      var result = FetchAndFillBuffer(null, out discard);

      if (result.Failed)
        throw GetFetchFailureException(result);
    }

    private ImapCommandResult FetchAndFillBuffer<TMessageBody>(ImapFetchDataItem extraFetchDataItem,
                                                               out TMessageBody fetched)
      where TMessageBody : ImapMessageBody
    {
      ImapCommandResult result;
      TMessageBody[] fetchedMessages;

      try {
        result = session.Fetch(fetchUidSet,
                               GetNextRangeFetchDataItem(extraFetchDataItem),
                               out fetchedMessages);
      }
      catch (TimeoutException ex) {
        throw GetTimeoutException(ex);
      }
      catch (ImapException ex) {
        throw GetUnexpectedException(ex);
      }

      if (result.Succeeded) {
        if (fetchedMessages.Length <= 0) {
          throw GetNoSuchMessageException(result);
        }
        else {
          fetched = fetchedMessages[0];

          var len = fetched.GetFirstBody(ref buffer);

          bufOffset = 0;
          bufRemain = len;

          eof = (len < fetchBlockSize);
        }
      }
      else {
        fetched = null;
      }

      return result;
    }

    private ImapFetchDataItem GetNextRangeFetchDataItem(ImapFetchDataItem extraFetchDataItem)
    {
      if (partialFetch) {
        var fetchStart = fetchRange.HasValue ? fetchRange.Value.Start : 0L;
        var fetchLength = (fetchRange.HasValue && fetchRange.Value.IsLengthSpecified)
          ? Math.Min(fetchRange.Value.Length.Value - position, fetchBlockSize)
          : fetchBlockSize;

        return ImapFetchDataItem.BodyText(peek, fetchSection, position + fetchStart, fetchLength);
      }
      else {
        var fetchDataItem = ImapFetchDataItem.BodyText(peek, position, fetchBlockSize);

        if (extraFetchDataItem == null)
          return fetchDataItem;
        else
          return extraFetchDataItem + fetchDataItem;
      }
    }

    protected virtual Exception GetNoSuchMessageException(ImapCommandResult result)
    {
      return new ImapProtocolViolationException("no such message or expunged");
    }

    protected virtual Exception GetFetchFailureException(ImapCommandResult result)
    {
      return new ImapProtocolViolationException(result.ResultText);
    }

    protected virtual Exception GetTimeoutException(TimeoutException ex)
    {
      return ex;
    }

    protected virtual Exception GetUnexpectedException(ImapException ex)
    {
      return ex;
    }

    private void RejectDisposed()
    {
      if (IsClosed)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private /*readonly*/ ImapSession session;
    private readonly bool partialFetch;
    private readonly bool peek;
    private ImapSequenceSet fetchUidSet;
    private string fetchSection;
    private ImapPartialRange? fetchRange;
    private readonly long fetchBlockSize;
    private long length;
    private /*readonly*/ byte[] buffer;
    private long position = 0;
    private int bufOffset = 0;
    private int bufRemain  = 0;
    private bool eof = false;
  }
}
