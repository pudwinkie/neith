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
using System.Runtime.Serialization;
#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif

namespace Smdn.Net.Imap4.Protocol.Client {
  [Serializable]
  internal sealed class ImapCommandResult<TResultValue> : ImapCommandResult where TResultValue : class {
    public TResultValue Value {
      get { return val; }
    }

    // default constructor called by Activator.CreateInstance
    public ImapCommandResult()
      : base()
    {
      this.val = null;
    }

    internal ImapCommandResult(ImapCommandResultCode code, TResultValue val, ImapResponseText responseText)
      : base(code, responseText)
    {
      // allow null
      this.val = val;
    }

    public ImapCommandResult(TResultValue val, ImapResponseText responseText)
      : base(ImapCommandResultCode.Ok, responseText)
    {
      if (val == null)
        throw new ArgumentNullException("val");

      this.val = val;
    }

    protected ImapCommandResult(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    [NonSerialized]
    private /*readonly*/ TResultValue val;
  }

  [Serializable]
  public class ImapCommandResult : ISerializable {
    public ImapCommandResultCode Code {
      get { return code; }
      internal set { code = value; }
    }

    public bool Succeeded {
      // true for 2xx, false for 3xx/4xx/5xx
      get { return ((int)code < 300); }
    }

    public bool Failed {
      get { return !Succeeded; }
    }

    internal Exception Exception {
      get { return exception; }
      set { exception = value; }
    }

    public string Description {
      get { return description; }
      internal set { description = value; }
    }

    public string ResponseText {
      get { return responseText; }
      internal set { responseText = value; }
    }

    public string ResultText {
      get
      {
        if (responseText == null)
          return string.Concat("<", description, ">");
        else
          return string.Concat("\"", responseText, "\"");
      }
    }

    public ImapTaggedStatusResponse TaggedStatusResponse {
      get { return taggedStatusResponse; }
      internal set { taggedStatusResponse = value; }
    }

    public virtual IEnumerable<ImapResponse> ReceivedResponses {
      get { return receivedResponses; }
      protected internal set
      {
        if (value == null)
          throw new ArgumentNullException("ReceivedResponses");
        receivedResponses = value;
      }
    }

    // default constructor called by Activator.CreateInstance
    public ImapCommandResult()
      : this(ImapCommandResultCode.Default, null, null)
    {
    }

    internal ImapCommandResult(ImapCommandResultCode code, string description)
      : this(code, description, null)
    {
    }

    internal ImapCommandResult(ImapCommandResultCode code, ImapResponseText responseText)
      : this(code, null, responseText.Text)
    {
    }

    private ImapCommandResult(ImapCommandResultCode code, string description, string responseText)
    {
      this.code = code;
      this.description = description;
      this.responseText = responseText;
    }

    internal protected ImapCommandResult(SerializationInfo info, StreamingContext context)
    {
      this.code = (ImapCommandResultCode)info.GetValue("code", typeof(ImapCommandResultCode));
      this.description = info.GetString("description");
      this.responseText = info.GetString("responseText");
      this.taggedStatusResponse = (ImapTaggedStatusResponse)info.GetValue("taggedStatusResponse", typeof(ImapTaggedStatusResponse));
      this.receivedResponses = (ImapResponse[])info.GetValue("receivedResponses", typeof(ImapResponse[]));
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("code", code);
      //info.AddValue("exception", exception); non serialized
      info.AddValue("description", description);
      info.AddValue("responseText", responseText);
      info.AddValue("taggedStatusResponse", taggedStatusResponse);
      info.AddValue("receivedResponses", receivedResponses.ToArray());
    }

    public ImapStatusResponse GetResponseCode(ImapResponseCode code)
    {
      foreach (var resp in receivedResponses) {
        var status = resp as ImapStatusResponse;

        if (status != null && status.ResponseText.Code == code)
          return status;
      }

      return null;
    }

    public ImapDataResponse GetResponse(ImapDataResponseType type)
    {
      foreach (var resp in receivedResponses) {
        var data = resp as ImapDataResponse;

        if (data != null && data.Type == type)
          return data;
      }

      return null;
    }

    public static explicit operator bool(ImapCommandResult r)
    {
      return r.Succeeded;
    }

    public override string ToString()
    {
      return string.Format("{{Succeeded={0} Code={1} Description='{2}' ResponseText='{3}'}}",
                           Succeeded,
                           code,
                           description,
                           responseText);
    }

    private ImapCommandResultCode code;
    private Exception exception;
    private string description;
    private string responseText;
    private ImapTaggedStatusResponse taggedStatusResponse = null;
    private IEnumerable<ImapResponse> receivedResponses = new ImapResponse[] {};
  }
}
