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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

using Smdn.Net.Imap4.Client.Session;
using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.WebClients {
  // TODO: [Serializable]
  public abstract class ImapWebRequest : WebRequest, IImapSessionProfile {
    private ImapSessionManager sessionManager;
    private IAsyncResult asyncResult = null;
    internal protected IAsyncResult beginAppendAsyncResult = null;

    private ImapSession session = null;

    protected ImapSession Session {
      get { return session; }
    }

    private ICredentialsByHost credentials = null;

    /// <value>value must implement ICredentialsByHost</value>
    public override ICredentials Credentials {
      get { return credentials as ICredentials; }
      set
      {
        var creds = value as ICredentialsByHost;

        if (value != null && creds == null)
          throw new ArgumentException("value must be ICredentialsByHost", "Credentials");

        credentials = creds;
      }
    }

    ICredentialsByHost IImapSessionProfile.Credentials {
      get { return credentials; }
    }

    private readonly Uri requestUri;

    public override Uri RequestUri {
      get { return requestUri; }
    }

    protected string RequestMailbox {
      get { return ImapStyleUriParser.GetMailbox(requestUri); }
    }

    Uri IImapSessionProfile.Authority {
      get { return new Uri(requestUri.GetLeftPart(UriPartial.Authority)); }
    }

    private int timeout = ImapWebRequestDefaults.Timeout;

    public override int Timeout {
      get { return timeout; }
      set
      {
        CheckRequestStarted();
        if (value < -1)
          throw new ArgumentOutOfRangeException("Timeout", value, "must be greater than or equals to -1");
        timeout = value;
      }
    }

    private int readWriteTimeout = ImapWebRequestDefaults.ReadWriteTimeout;

    public virtual int ReadWriteTimeout {
      get { return readWriteTimeout; }
      set
      {
        CheckRequestStarted();
        if (value < -1)
          throw new ArgumentOutOfRangeException("ReadWriteTimeout", value, "must be greater than or equals to -1");
        readWriteTimeout = value;
      }
    }

    int IImapSessionProfile.SendTimeout {
      get { return readWriteTimeout; }
    }

    int IImapSessionProfile.ReceiveTimeout {
      get { return readWriteTimeout; }
    }

    private string method;

    public override string Method {
      get { return method; }
      set
      {
        CheckRequestStarted();
        if (value == null)
          throw new ArgumentNullException("Method");
        else if (!ImapWebRequestMethods.IsSupportedMethod(this, value))
          throw new ArgumentException("unsupported method", "Method");
        method = value;
      }
    }

    private Uri destinationUri = null;

    /// <value>be used when method is COPY or RENAME</value>
    public Uri DestinationUri {
      get { return destinationUri; }
      set
      {
        CheckRequestStarted();
        if (value == null)
          throw new ArgumentNullException("DestinationUri");
        else if (!string.Equals(value.Scheme, requestUri.Scheme, StringComparison.OrdinalIgnoreCase))
          throw new ArgumentException("scheme of DestinationUri must be same as RequestUri", "DestinationUri");
        else if (ImapStyleUriParser.GetStrongAuthority(value) != ImapStyleUriParser.GetStrongAuthority(requestUri))
          throw new ArgumentException("authority of DestinationUri must be same as RequestUri", "DestinationUri");
        destinationUri = value;
      }
    }

    private bool fetchPeek = ImapWebRequestDefaults.FetchPeek;

    /// <value>be used when method is FETCH</value>
    public bool FetchPeek {
      get { return fetchPeek; }
      set { CheckRequestStarted(); fetchPeek = value; }
    }

    private int fetchBlockSize = ImapWebRequestDefaults.FetchBlockSize;

    /// <value>be used when method is FETCH</value>
    public int FetchBlockSize {
      get { return fetchBlockSize; }
      set
      {
        CheckRequestStarted();
        if (value <= 0)
          throw new ArgumentOutOfRangeException("FetchBlockSize", value, "must be non-zero positive number");
        fetchBlockSize = value;
      }
    }

    /// <value>be used when method is SEARCH, SORT, THREAD or FETCH</value>
    private ImapFetchDataItemMacro fetchDataItem = ImapWebRequestDefaults.FetchDataItem;

    public ImapFetchDataItemMacro FetchDataItem {
      get { return fetchDataItem; }
      set { CheckRequestStarted(); fetchDataItem = value; }
    }

    private ImapStoreDataItem storeDataItem = null;

    /// <value>be used when method is STORE</value>
    public ImapStoreDataItem StoreDataItem {
      get { return storeDataItem; }
      set
      {
        CheckRequestStarted();
        if (value == null)
          throw new ArgumentNullException("StoreDataItem");
        storeDataItem = value;
      }
    }

    private ImapStatusDataItem statusDataItem = null;

    public ImapStatusDataItem StatusDataItem {
      get { return statusDataItem; }
      set
      {
        CheckRequestStarted();
        if (value == null)
          throw new ArgumentNullException("StatusDataItem");
        statusDataItem = value;
      }
    }

    private ImapSortCriteria sortCriteria = null;

    /// <value>be used when method is SORT</value>
    public ImapSortCriteria SortCriteria {
      get { return sortCriteria; }
      set
      {
        CheckRequestStarted();
        if (value == null)
          throw new ArgumentNullException("SortCriteria");
        sortCriteria = value;
      }
    }

    private ImapThreadingAlgorithm threadingAlgorithm = null;

    /// <value>be used when method is THREAD</value>
    public ImapThreadingAlgorithm ThreadingAlgorithm {
      get { return threadingAlgorithm; }
      set
      {
        CheckRequestStarted();
        if (value == null)
          throw new ArgumentNullException("ThreadingAlgorithm");
        threadingAlgorithm = value;
      }
    }

    private long contentLength = 0L;

    /// <value>be used when method is APPEND</value>
    public override long ContentLength {
      get { return contentLength; }
      set
      {
        CheckRequestStarted();
        if (value < 0)
          throw new ArgumentOutOfRangeException("ContentLength", value, "must be zero or positive number");
        contentLength = value;
      }
    }

    private bool subscription = ImapWebRequestDefaults.Subscription;

    public bool Subscription {
      get { return subscription; }
      set { CheckRequestStarted(); subscription = value; }
    }

    private bool allowCreateMailbox = ImapWebRequestDefaults.AllowCreateMailbox;

    public bool AllowCreateMailbox {
      get { return allowCreateMailbox; }
      set { CheckRequestStarted(); allowCreateMailbox = value; }
    }

    private bool useTlsIfAvailable = ImapWebRequestDefaults.UseTlsIfAvailable;

    public bool UseTlsIfAvailable {
      get { return useTlsIfAvailable; }
      set { CheckRequestStarted(); useTlsIfAvailable = value; }
    }

    public bool UseDeflateIfAvailable {
      get; set;
    }

    private string[] usingSaslMechanisms
      = (string[])ImapWebRequestDefaults.UsingSaslMechanisms.Clone();

    public string[] UsingSaslMechanisms {
      get { return usingSaslMechanisms; }
      set { CheckRequestStarted(); usingSaslMechanisms = value; }
    }

    private bool allowInsecureLogin = ImapWebRequestDefaults.AllowInsecureLogin;

    public bool AllowInsecureLogin {
      get { return allowInsecureLogin; }
      set { CheckRequestStarted(); allowInsecureLogin = value; }
    }

    private bool keepAlive = ImapWebRequestDefaults.KeepAlive;

    public bool KeepAlive {
      get { return keepAlive; }
      set { CheckRequestStarted(); keepAlive = value; }
    }

    private bool readOnly = ImapWebRequestDefaults.ReadOnly;

    public bool ReadOnly {
      get { return readOnly; }
      set { CheckRequestStarted(); readOnly = value; }
    }

    private ImapResponseCode[] expectedErrorResponseCodes
      = (ImapResponseCode[])ImapWebRequestDefaults.ExpectedErrorResponseCodes.Clone();

    public ImapResponseCode[] ExpectedErrorResponseCodes {
      get { return expectedErrorResponseCodes; }
      set { CheckRequestStarted(); expectedErrorResponseCodes = value; }
    }

    protected ImapWebRequest(Uri requestUri, string defaultMethod, ImapSessionManager sessionManager)
      : base()
    {
      // scheme is checked by IWebRequestCreate.Create
      this.requestUri = requestUri;

      this.method = defaultMethod;
      this.sessionManager = sessionManager;
      this.UseDeflateIfAvailable = false;
    }

    protected ImapWebRequest(SerializationInfo serializationInfo, StreamingContext streamingContext)
      : base(serializationInfo, streamingContext)
    {
    }

    protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
      base.GetObjectData(serializationInfo, streamingContext);
    }

    public override Stream GetRequestStream()
    {
      return EndGetRequestStream(BeginGetRequestStream(null, null));
    }

    public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
    {
      CheckRequestStarted();

      throw new NotSupportedException();
    }

    public override WebResponse GetResponse()
    {
      return EndGetResponse(BeginGetResponse(null, null));
    }

    private delegate ImapWebResponse GetResponseDelegate();

    public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
    {
      if (beginAppendAsyncResult == null)
        CheckRequestStarted();

      this.asyncResult = (new GetResponseDelegate(GetResponseProc)).BeginInvoke(callback, state);

      return this.asyncResult;
    }

    public override WebResponse EndGetResponse(IAsyncResult asyncResult)
    {
      var ar = asyncResult as System.Runtime.Remoting.Messaging.AsyncResult;

      if (ar != this.asyncResult)
        throw new ArgumentException("invalid IAsyncResult", "asyncResult");

      try {
        return (ar.AsyncDelegate as GetResponseDelegate).EndInvoke(ar);
      }
      finally {
        this.asyncResult = null;
      }
    }

    protected abstract ImapWebResponse InternalGetResponse();

    private ImapWebResponse GetResponseProc()
    {
      try {
        ImapCommandResult noopResult = null;

        if (beginAppendAsyncResult == null)
          GetSession(out noopResult);

        ImapWebResponse response = null;

        try {
          try {
            if (string.Equals(Method, ImapWebRequestMethods.NoOp, StringComparison.OrdinalIgnoreCase)) {
              if (noopResult == null)
                response = GetNoOpResponse();
              else
                response = new ImapWebResponse(noopResult);
            }
            else {
              response = InternalGetResponse();
            }
          }
          catch (TimeoutException ex) {
            throw new WebException("timed out", ex, WebExceptionStatus.Timeout, null);
          }
          catch (ImapException ex) {
            if (ex is ImapProtocolViolationException)
              throw new ProtocolViolationException(ex.Message);
            else if (ex is ImapIncapableException)
              throw new ProtocolViolationException(ex.Message);
            else
              throw new WebException("unexpected error", ex, WebExceptionStatus.UnknownError, null);
          }

          if (response != null)
            response.SetSessionInfo(session);
        }
        finally {
          if (!keepAlive && (response == null || !response.IsSessionClosedByResponseStream))
            CloseSession();
        }

        if (response == null)
          // success
          throw new WebException("No error was encountered.", WebExceptionStatus.Success);

        if (response.ResponseUri == null)
          response.SetResponseUri(requestUri);

        if (response.Result.Succeeded)
          // succeeded
          return response;

        if (expectedErrorResponseCodes != null && response.ResponseCode != null) {
          foreach (var code in expectedErrorResponseCodes) {
            if (response.ResponseCode == code)
              // expected error
              return response;
          }
        }

        // unexpected error
        throw new WebException(response.ResponseDescription, null, WebExceptionStatus.ProtocolError, response);
      }
      finally {
        session = null;
      }
    }

    protected ImapWebResponse GetNoOpResponse()
    {
      return new ImapWebResponse(session.NoOp());
    }

    protected Uri GetDestinationUri(ImapUriForm requiredForm)
    {
      if (destinationUri == null)
        throw new InvalidOperationException("DestinationUri must be set");

      var form = ImapStyleUriParser.GetUriForm(destinationUri);

      if (form != requiredForm)
        throw new InvalidOperationException(string.Format("invalid form of destination URI, required form is {0} but was {1}", requiredForm, form));

      return destinationUri;
    }

    protected string GetDestinationMailbox()
    {
      return ImapStyleUriParser.GetMailbox(GetDestinationUri(ImapUriForm.ListMessages));
    }

    protected ImapSearchCriteria GetSearchCriteria(ImapCapabilityList serverCapabilities)
    {
      string discard;

      return GetSearchCriteria(serverCapabilities, false, out discard);
    }

    protected ImapSearchCriteria GetSearchCriteria(ImapCapabilityList serverCapabilities, out string charset)
    {
      return GetSearchCriteria(serverCapabilities, true, out charset);
    }

    private ImapSearchCriteria GetSearchCriteria(ImapCapabilityList serverCapabilities, bool splitCharset, out string charset)
    {
      if (ImapStyleUriParser.GetUriForm(requestUri) != ImapUriForm.SearchMessages)
        throw new InvalidOperationException("request URI does not include query");

      /*
       * http://tools.ietf.org/html/rfc5092
       * RFC 5092 - IMAP URL Scheme
       * 
       *    Note that quoted strings and non-synchronizing literals [LITERAL+]
       *    are allowed in the <enc-search> content; however, synchronizing
       *    literals are not allowed, as their presence would effectively mean
       *    that the agent interpreting IMAP URLs needs to parse an <enc-search>
       *    content, find all synchronizing literals, and perform proper command
       *    continuation request handling (see Sections 4.3 and 7 of [IMAP4]).
       */
      charset = null;

      bool containsLiteral;

      var ret = ImapSearchCriteria.FromUri(requestUri, splitCharset, out containsLiteral, out charset);

      if (!serverCapabilities.Has(ImapCapability.LiteralNonSync) && containsLiteral)
        throw new ImapIncapableException("query contains literal but LITERAL+ is incapable.");

      return ret;
    }

    protected ImapFetchDataItem GetFetchDataItem()
    {
      return ImapFetchDataItem.FromMacro(fetchDataItem);
    }

    protected ImapSession GetSession()
    {
      ImapCommandResult discard;

      return GetSession(out discard);
    }

    private ImapSession GetSession(out ImapCommandResult noopResult)
    {
      noopResult = null;

      var s = sessionManager.GetExistSession(requestUri);

      if (s != null) {
        if (s.IsDisposed) {
          s = null;
        }
        else if (s.IsTransactionProceeding) {
          throw new WebException("another transaction proceeding", WebExceptionStatus.Pending);
        }
        else {
          try {
            // check connected
            if (s.State != ImapSessionState.NotConnected)
              noopResult = s.NoOp();
          }
          catch {
            // ignore exception
          }

          // disconnected
          if (s.IsDisposed || s.State == ImapSessionState.NotConnected) {
            sessionManager.UnregisterSession(requestUri);
            s = null;
            noopResult = null;
          }
        }
      }

      if (s == null) {
        var session = ImapSessionManager.CreateSession(this);

        if (keepAlive)
          sessionManager.RegisterSession(requestUri, session);

        s = session;
      }

      this.session = s;

      return s;
    }

    protected ImapWebResponse SelectRequestMailbox()
    {
      return SelectRequestMailbox(readOnly);
    }

    protected ImapWebResponse SelectRequestMailbox(bool selectAsReadOnly)
    {
      var mailbox = RequestMailbox;

      ImapWebResponse response = null;

      if (Session.State == ImapSessionState.Selected &&
          (!string.Equals(Session.SelectedMailbox.Name, mailbox) || selectAsReadOnly != session.SelectedMailbox.ReadOnly)) {
        response = CloseMailbox();

        if (response != null && response.Result.Failed)
          return response;
      }

      if (Session.State != ImapSessionState.Selected)
        response = new ImapWebResponse(selectAsReadOnly ? Session.Examine(mailbox) : Session.Select(mailbox));

      return response;
    }

    protected ImapWebResponse CloseMailbox()
    {
      if (Session.State != ImapSessionState.Selected)
        return null;

      return new ImapWebResponse(Session.Close());
    }

    private void CloseSession()
    {
      sessionManager.UnregisterSession(requestUri);

      CloseSession(session);

      session = null;
    }

    internal static void CloseSession(ImapSession session)
    {
      try {
        session.Disconnect(true);
      }
      catch {
        // ignore exceptions
      }
    }

    protected void CheckRequestStarted()
    {
      if ((asyncResult != null && !asyncResult.IsCompleted) || beginAppendAsyncResult != null)
        throw new InvalidOperationException("request is in progress");
    }
  }
}
