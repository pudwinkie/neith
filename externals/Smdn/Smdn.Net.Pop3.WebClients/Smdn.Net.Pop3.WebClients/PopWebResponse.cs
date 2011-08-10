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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;

using Smdn.Net.Pop3.Client.Session;
using Smdn.Net.Pop3.Protocol;
using Smdn.Net.Pop3.Protocol.Client;

namespace Smdn.Net.Pop3.WebClients {
  // TODO: [Serializable]
  public class PopWebResponse : WebResponse {
    public PopCapabilitySet ServerCapabilities {
      get; private set;
    }

    private Uri responseUri = null;

    public override Uri ResponseUri {
      get { return responseUri; }
    }

    public override long ContentLength {
      get { CheckDisposed(); return (RetrievedMessageStream == null) ? 0L : RetrievedMessageStream.Length; }
    }

    public override bool IsFromCache {
      get { return false; }
    }

    public PopCommandResult Result {
      get; private set;
    }

    /// <summary>A response code of status response.</summary>
    /// <value>This value might be null.</value>
    public PopResponseCode ResponseCode {
      get
      {
        if (Result.StatusResponse == null)
          return null;
        else
          return Result.StatusResponse.ResponseText.Code;
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

    public PopDropListing DropList {
      get; internal set;
    }

    public PopScanListing[] ScanLists {
      get; internal set;
    }

    public PopUniqueIdListing[] UniqueIdLists {
      get; internal set;
    }

    internal Stream RetrievedMessageStream {
      get; set;
    }

    internal PopWebResponse(PopCommandResult r)
      : base()
    {
      this.DropList = PopDropListing.Empty;
      this.ScanLists = new PopScanListing[] {};
      this.UniqueIdLists = new PopUniqueIdListing[] {};
      this.RetrievedMessageStream = null;

      this.Result = r;
    }

    protected PopWebResponse(SerializationInfo serializationInfo, StreamingContext streamingContext)
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

      if (RetrievedMessageStream == null)
        throw new InvalidOperationException();
      else
        return RetrievedMessageStream;
    }

    public override void Close()
    {
      if (RetrievedMessageStream != null) {
        RetrievedMessageStream.Close();
        RetrievedMessageStream = null;
      }

      disposed = true;
    }

    internal void SetResponseUri(Uri uri)
    {
      responseUri = uri;
    }

    internal void SetSessionInfo(PopSession session)
    {
      ServerCapabilities = new PopCapabilitySet(true, session.ServerCapabilities);
    }

    private bool disposed = false;

    private void CheckDisposed()
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);
    }
  }
}
