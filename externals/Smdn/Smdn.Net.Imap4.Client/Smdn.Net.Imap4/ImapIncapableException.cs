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
using System.Runtime.Serialization;

namespace Smdn.Net.Imap4 {
  // Exceptions:
  //   System.Excpetion
  //     System.SystemException
  //       Smdn.Net.Imap4.ImapExcetion
  //         Smdn.Net.Imap4.ImapInvalidOperationException
  //           Smdn.Net.Imap4.ImapIncapableException

  [Serializable]
  public class ImapIncapableException : ImapInvalidOperationException {
    private ImapCapability[] requiredCapabilities;

    public ImapCapability[] RequiredCapabilities {
      get { return requiredCapabilities; }
    }

    public ImapIncapableException()
      : base()
    {
      this.requiredCapabilities = null;
    }

    public ImapIncapableException(ImapCapability requiredCapability)
      : this(string.Concat(requiredCapability, " is incapable"), new[] {requiredCapability})
    {
    }

    public ImapIncapableException(ImapCapability[] requiredCapabilities)
#if NET_4_0
      : this(string.Format("incapable ({0})", string.Join<ImapCapability>(", ", requiredCapabilities)), requiredCapabilities)
#else
      : this(string.Format("incapable ({0})", string.Join(", ", Array.ConvertAll(requiredCapabilities, (c) => c.ToString()))), requiredCapabilities)
#endif
    {
    }

    public ImapIncapableException(string message, ImapCapability requiredCapability)
      : this(message, new[] {requiredCapability})
    {
    }

    public ImapIncapableException(string message, ImapCapability[] requiredCapabilities)
      : base(message)
    {
      this.requiredCapabilities = requiredCapabilities;
    }

    public ImapIncapableException(string message)
      : base(message)
    {
      this.requiredCapabilities = null;
    }

    protected ImapIncapableException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      this.requiredCapabilities = (ImapCapability[])info.GetValue("requiredCapabilities",
                                                                  typeof(ImapCapability[]));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);

      info.AddValue("requiredCapabilities", requiredCapabilities);
    }
  }
}
