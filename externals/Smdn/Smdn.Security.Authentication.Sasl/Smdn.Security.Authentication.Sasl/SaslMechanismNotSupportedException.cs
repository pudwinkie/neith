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
using System.Runtime.Serialization;

namespace Smdn.Security.Authentication.Sasl {
  [Serializable]
  public class SaslMechanismNotSupportedException : NotSupportedException {
    public SaslMechanismNotSupportedException()
      : base()
    {
    }

    public SaslMechanismNotSupportedException(string mechanismName)
      : this(mechanismName, "unsupported mechanism")
    {
    }

    public SaslMechanismNotSupportedException(string mechanismName, Exception innerException)
      : this(mechanismName, "unsupported mechanism", innerException)
    {
    }

    public SaslMechanismNotSupportedException(string mechanismName, string message)
      : base(string.Format("{0}\nmechanism name: {1}", message, mechanismName))
    {
    }

    public SaslMechanismNotSupportedException(string mechanismName, string message, Exception innerException)
      : base(string.Format("{0}\nmechanism name: {1}", message, mechanismName), innerException)
    {
    }

    protected SaslMechanismNotSupportedException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
