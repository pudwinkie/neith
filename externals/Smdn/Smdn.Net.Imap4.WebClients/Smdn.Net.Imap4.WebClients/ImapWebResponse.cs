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
using System.Net;
using System.Runtime.Serialization;
using System.Text;

using Smdn.Net.Imap4.Client.Session;
using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.WebClients {
  // TODO: [Serializable]
  public class ImapWebResponse : WebResponse {
    internal bool IsSessionClosedByResponseStream {
      get; set;
    }

    public ImapNamespace Namespaces {
      get; private set;
    }

    public IDictionary<string, string> ServerID {
      get; private set;
    }

    public ImapCapabilitySet ServerCapabilities {
      get; private set;
    }

    private Uri responseUri = null;

    public override Uri ResponseUri {
      get { return responseUri; }
    }

    private long contentLength = 0L;

    public override long ContentLength {
      get { return contentLength; }
    }

    private string contentType = string.Empty;

    public override string ContentType {
      get { return contentType; }
    }

    public override bool IsFromCache {
      get { return false; }
    }

    public ImapCommandResult Result {
      get; private set;
    }

    /// <summary>A response code of tagged status response.</summary>
    /// <value>This value might be null.</value>
    public ImapResponseCode ResponseCode {
      get
      {
        if (Result.TaggedStatusResponse == null)
          return null;
        else
          return Result.TaggedStatusResponse.ResponseText.Code;
      }
    }

    /// <summary>server response text or error description</summary>
    public string ResponseDescription {
      get { return Result.ResultText; }
    }

    private WebHeaderCollection headers = new WebHeaderCollection();

    public override WebHeaderCollection Headers {
      get { return headers; }
    }

    [CLSCompliant(false)]
    public IImapMessageAttribute[] MessageAttributes {
      get; internal set;
    }

    public ImapMailbox[] Mailboxes {
      get; internal set;
    }

    public ImapThreadTree ThreadTree {
      get; internal set;
    }

    public Encoding[] SupportedCharsets {
      get; internal set;
    }

    private bool hasResponseStream = false;
    private Stream responseStream = null;

    internal ImapWebResponse(ImapCommandResult r)
      : base()
    {
      this.IsSessionClosedByResponseStream = false;

      this.MessageAttributes = new IImapMessageAttribute[] {};
      this.Mailboxes = new ImapMailbox[] {};
      this.ThreadTree = new ImapThreadTree(true, null, new ImapThreadTree[] {});
      this.SupportedCharsets = new Encoding[] {};

      this.Result = r;
    }

    protected ImapWebResponse(SerializationInfo serializationInfo, StreamingContext streamingContext)
      : base(serializationInfo, streamingContext)
    {
    }

    protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
      base.GetObjectData(serializationInfo, streamingContext);
    }

    public override Stream GetResponseStream()
    {
      CheckDisposed();

      if (!hasResponseStream)
        throw new InvalidOperationException("response has no data stream");
      else if (responseStream == null)
        throw new InvalidOperationException("response stream has been closed");
      else
        return responseStream;
    }

    public override void Close()
    {
      if (responseStream != null) {
        responseStream.Close();
        responseStream = null;
      }

      disposed = true;
    }

    internal void SetResponseUri(Uri uri)
    {
      responseUri = uri;
    }

    internal void SetSessionInfo(ImapSession session)
    {
      Namespaces = session.Namespaces.Clone();
      ServerCapabilities = new ImapCapabilitySet(true, session.ServerCapabilities);
      ServerID = session.ServerID;
    }

    internal void SetResponseStream(Stream stream, long length, bool isSessionClosedByResponseStream)
    {
      hasResponseStream = true;

      responseStream = stream;
      contentLength = length;

      IsSessionClosedByResponseStream = isSessionClosedByResponseStream;
    }

    internal void SetContentType(string contentType)
    {
      this.contentType = contentType;
    }

    private bool disposed = false;

    private void CheckDisposed()
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);
    }
  }
}
