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
using System.Runtime.Serialization;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif

namespace Smdn.Net.Pop3.Protocol.Client {
  [Serializable]
  internal sealed class PopCommandResult<TResultValue> : PopCommandResult {
    public TResultValue Value {
      get { return val; }
    }

    // default constructor used by Activator.CreateInstance
    public PopCommandResult()
      : base()
    {
      this.val = default(TResultValue);
    }

    internal PopCommandResult(TResultValue val, PopResponseText responseText)
      : base(PopCommandResultCode.Ok, responseText)
    {
      /*
      if (val == null)
        throw new ArgumentNullException("val");
      */

      this.val = val;
    }

    protected PopCommandResult(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    [NonSerialized]
    private /*readonly*/ TResultValue val;
  }

  [Serializable]
  public class PopCommandResult : ISerializable {
    public PopCommandResultCode Code {
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

    public PopStatusResponse StatusResponse {
      get { return statusResponse; }
      internal set { statusResponse = value; }
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

    public virtual IEnumerable<PopResponse> ReceivedResponses {
      get { return receivedResponses; }
      protected internal set
      {
        if (value == null)
          throw new ArgumentNullException("ReceivedResponses");
        receivedResponses = value;
      }
    }

    // default constructor used by Activator.CreateInstance
    public PopCommandResult()
      : this(PopCommandResultCode.Default, null, null)
    {
    }

    internal PopCommandResult(PopCommandResultCode code, string description)
      : this(code, description, null)
    {
    }

    internal PopCommandResult(PopCommandResultCode code, PopResponseText responseText)
      : this(code, null, responseText.GetTextAsString())
    {
    }

    private PopCommandResult(PopCommandResultCode code, string description, string responseText)
    {
      this.code = code;
      this.description = description;
      this.responseText = responseText;
    }

    internal protected PopCommandResult(SerializationInfo info, StreamingContext context)
    {
      this.code = (PopCommandResultCode)info.GetValue("code", typeof(PopCommandResultCode));
      this.description = info.GetString("description");
      this.responseText = info.GetString("responseText");
      this.statusResponse = (PopStatusResponse)info.GetValue("statusResponse", typeof(PopStatusResponse));
      this.receivedResponses = (PopResponse[])info.GetValue("receivedResponses", typeof(PopResponse[]));
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("code", code);
      info.AddValue("description", description);
      info.AddValue("responseText", responseText);
      info.AddValue("statusResponse", statusResponse);
      info.AddValue("receivedResponses", receivedResponses.ToArray());
    }

    public PopStatusResponse GetResponseCode(PopResponseCode code)
    {
      foreach (var resp in receivedResponses) {
        var status = resp as PopStatusResponse;

        if (status != null && status.ResponseText.Code == code)
          return status;
      }

      return null;
    }

    public static explicit operator bool(PopCommandResult r)
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

    private PopCommandResultCode code;
    private Exception exception;
    private string description;
    private string responseText;
    private PopStatusResponse statusResponse = null;
    private IEnumerable<PopResponse> receivedResponses = new PopResponse[] {};
  }
}
