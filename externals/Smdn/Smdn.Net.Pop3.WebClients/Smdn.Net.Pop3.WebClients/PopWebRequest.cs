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
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

using Smdn.Net.Pop3.Client.Session;
using Smdn.Net.Pop3.Protocol;
using Smdn.Net.Pop3.Protocol.Client;

namespace Smdn.Net.Pop3.WebClients {
  // TODO: [Serializable]
  public abstract class PopWebRequest : WebRequest, IPopSessionProfile {
    private PopSessionManager sessionManager;
    private IAsyncResult asyncResult = null;

    private PopSession session = null;

    protected PopSession Session {
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

    ICredentialsByHost IPopSessionProfile.Credentials {
      get { return credentials; }
    }

    private Uri requestUri;

    public override Uri RequestUri {
      get { return requestUri; }
    }

    Uri IPopSessionProfile.Authority {
      get { return new Uri(requestUri.GetLeftPart(UriPartial.Authority)); }
    }

    private int timeout = PopWebRequestDefaults.Timeout;

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

    private int readWriteTimeout = PopWebRequestDefaults.ReadWriteTimeout;

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

    int IPopSessionProfile.SendTimeout {
      get { return readWriteTimeout; }
    }

    int IPopSessionProfile.ReceiveTimeout {
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
        else if (!PopWebRequestMethods.IsSupportedMethod(this, value))
          throw new ArgumentException("unsupported method", "Method");
        method = value;
      }
    }

    private bool deleteAfterRetrieve = PopWebRequestDefaults.DeleteAfterRetrieve;

    public bool DeleteAfterRetrieve {
      get { return deleteAfterRetrieve; }
      set { CheckRequestStarted(); deleteAfterRetrieve = value; }
    }

    private bool useTlsIfAvailable = PopWebRequestDefaults.UseTlsIfAvailable;

    public bool UseTlsIfAvailable {
      get { return useTlsIfAvailable; }
      set { CheckRequestStarted(); useTlsIfAvailable = value; }
    }

    private string[] usingSaslMechanisms
      = (string[])PopWebRequestDefaults.UsingSaslMechanisms.Clone();

    public string[] UsingSaslMechanisms {
      get { return usingSaslMechanisms; }
      set { CheckRequestStarted(); usingSaslMechanisms = value; }
    }

    private bool allowInsecureLogin = PopWebRequestDefaults.AllowInsecureLogin;

    public bool AllowInsecureLogin {
      get { return allowInsecureLogin; }
      set { CheckRequestStarted(); allowInsecureLogin = value; }
    }

    private bool keepAlive = PopWebRequestDefaults.KeepAlive;

    public bool KeepAlive {
      get { return keepAlive; }
      set { CheckRequestStarted(); keepAlive = value; }
    }

    private PopResponseCode[] expectedErrorResponseCodes
      = (PopResponseCode[])PopWebRequestDefaults.ExpectedErrorResponseCodes.Clone();

    public PopResponseCode[] ExpectedErrorResponseCodes {
      get { return expectedErrorResponseCodes; }
      set { CheckRequestStarted(); expectedErrorResponseCodes = value; }
    }

    protected PopWebRequest(Uri requestUri, string defaultMethod, PopSessionManager sessionManager)
      : base()
    {
      // scheme is checked by IWebRequestCreate.Create
      this.requestUri = requestUri;

      this.method = defaultMethod;
      this.sessionManager = sessionManager;
      //this.ExpectedErrorResponseCodes = new PopResponseCode[] {};
    }

    protected PopWebRequest(SerializationInfo serializationInfo, StreamingContext streamingContext)
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

    private delegate PopWebResponse GetResponseDelegate();

    public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
    {
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

    protected abstract PopWebResponse InternalGetResponse();

    private PopWebResponse GetResponseProc()
    {
      try {
        PopCommandResult noopResult;

        GetSession(out noopResult);

        PopWebResponse response;

        try {
          try {
            if (string.Equals(Method, PopWebRequestMethods.NoOp, StringComparison.OrdinalIgnoreCase)) {
              if (noopResult == null)
                response = GetNoOpResponse();
              else
                response = new PopWebResponse(noopResult);
            }
            else {
              response = InternalGetResponse();
            }
          }
          catch (TimeoutException ex) {
            throw new WebException("timed out", ex, WebExceptionStatus.Timeout, null);
          }
          catch (PopException ex) {
            if (ex is PopProtocolViolationException)
              throw new ProtocolViolationException(ex.Message);
            else if (ex is PopIncapableException)
              throw new ProtocolViolationException(ex.Message);
            else
              throw new WebException("unexpected error", ex, WebExceptionStatus.UnknownError, null);
          }

          response.SetSessionInfo(session);
        }
        finally {
          if (!KeepAlive)
            CloseSession();
        }

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

    protected PopWebResponse GetNoOpResponse()
    {
      return new PopWebResponse(session.NoOp());
    }

    protected PopSession GetSession()
    {
      PopCommandResult discard;

      return GetSession(out discard);
    }

    private PopSession GetSession(out PopCommandResult noopResult)
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
            if (s.State != PopSessionState.NotConnected)
              noopResult = s.NoOp();
          }
          catch {
            // ignore exception
          }

          // disconnected
          if (s.IsDisposed || s.State == PopSessionState.NotConnected) {
            sessionManager.UnregisterSession(requestUri);
            s = null;
            noopResult = null;
          }
        }
      }

      if (s == null) {
        var session = PopSessionManager.CreateSession(this);

        if (keepAlive)
          sessionManager.RegisterSession(requestUri, session);

        s = session;
      }

      this.session = s;

      return s;
    }

    private void CloseSession()
    {
      sessionManager.UnregisterSession(requestUri);

      try {
        session.Disconnect(true);
      }
      catch {
        // ignore exceptions
      }

      session = null;
    }

    protected void CheckRequestStarted()
    {
      if (asyncResult != null && !asyncResult.IsCompleted)
        throw new InvalidOperationException("request is in progress");
    }
  }
}