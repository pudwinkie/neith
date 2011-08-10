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
  //             Smdn.Net.Pop3.PopAuthenticationException

  [Serializable]
  public class PopAuthenticationException : PopErrorResponseException {
    public PopAuthenticationException()
      : base("authentication failed", (PopCommandResult)null)
    {
    }

    public PopAuthenticationException(PopCommandResult result)
      : base(string.Format("authentication failed {0} (result code:{1})", result.ResultText, result.Code), result)
    {
    }

    public PopAuthenticationException(string message)
      : base(message, (PopCommandResult)null)
    {
    }

    public PopAuthenticationException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected PopAuthenticationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
