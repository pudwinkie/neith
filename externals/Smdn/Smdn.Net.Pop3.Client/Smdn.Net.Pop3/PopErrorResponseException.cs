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
using System.Runtime.Serialization;

using Smdn.Net.Pop3.Protocol.Client;

namespace Smdn.Net.Pop3 {
  // Exceptions:
  //   System.Excpetion
  //     System.SystemException
  //       Smdn.Net.Pop3.PopExcetion
  //         Smdn.Net.Pop3.PopInvalidOperationException
  //           Smdn.Net.Pop3.PopErrorResponseException

  [Serializable]
  public class PopErrorResponseException : PopInvalidOperationException {
    private PopCommandResult result;

    public PopCommandResult Result {
      get { return result; }
    }

    public PopErrorResponseException()
      : base()
    {
      this.result = null;
    }

    public PopErrorResponseException(PopCommandResult result)
      : this(string.Format("{0}: {1}", result.Code, result.ResultText), result)
    {
    }

    public PopErrorResponseException(string message, PopCommandResult result)
      : base(message)
    {
      this.result = result;
    }

    public PopErrorResponseException(string message, Exception innerException)
      : base(message, innerException)
    {
      this.result = null;
    }

    protected PopErrorResponseException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      this.result = (PopCommandResult)info.GetValue("result", typeof(PopCommandResult));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);

      info.AddValue("result", result);
    }
  }
}