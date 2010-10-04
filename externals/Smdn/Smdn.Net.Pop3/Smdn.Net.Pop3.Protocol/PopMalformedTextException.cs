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

namespace Smdn.Net.Pop3.Protocol {
  // Exceptions:
  //   System.Excpetion
  //     System.SystemException
  //       System.FormatException
  //         Smdn.Net.Pop3.Protocol.PopFormatExcetion
  //           Smdn.Net.Pop3.Protocol.PopMalformedTextException

  [Serializable]
  public class PopMalformedTextException : PopFormatException {
    public ByteString CausedText {
      get; private set;
    }

    public PopMalformedTextException()
      : base("invalid format")
    {
    }

    public PopMalformedTextException(string message)
      : base(message)
    {
    }

    public PopMalformedTextException(ByteString causedText)
      : this("invalid format", causedText)
    {
    }

    public PopMalformedTextException(string message, ByteString causedText)
      : base(string.Format("{0}{1}text: {2}", message, Environment.NewLine, causedText))
    {
      this.CausedText = causedText;
    }

    public PopMalformedTextException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected PopMalformedTextException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      CausedText = (ByteString)info.GetValue("CausedText", typeof(ByteString));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);

      info.AddValue("CausedText", CausedText);
    }
  }
}