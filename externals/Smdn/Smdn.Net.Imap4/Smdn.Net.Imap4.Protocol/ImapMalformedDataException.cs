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

namespace Smdn.Net.Imap4.Protocol {
  // Exceptions:
  //   System.Excpetion
  //     System.SystemException
  //       Smdn.Net.Imap4.ImapExcetion
  //         Smdn.Net.Imap4.Protocol.ImapFormatException
  //           Smdn.Net.Imap4.Protocol.ImapMalformedDataException

  [Serializable]
  public class ImapMalformedDataException : ImapFormatException {
    private ImapData causedData;

    public ImapData CausedData {
      get { return causedData; }
    }

    public ImapMalformedDataException()
      : this("invalid format", null)
    {
    }

    public ImapMalformedDataException(string message)
      : this(message, null)
    {
    }

    public ImapMalformedDataException(ImapData causedData)
      : this("invalid format", causedData)
    {
    }

    public ImapMalformedDataException(string message, ImapData causedData)
      : base(string.Format("{0}{1}data: {2}", message, Environment.NewLine, causedData))
    {
      this.causedData = causedData;
    }

    protected ImapMalformedDataException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      causedData = (ImapData)info.GetValue("causedData", typeof(ImapData));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);

      info.AddValue("causedData", causedData);
    }
  }
}