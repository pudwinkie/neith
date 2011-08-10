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

using Smdn.Net.Imap4.Protocol;

namespace Smdn.Net.Imap4.Protocol.Client {
  [Serializable]
  public sealed class ImapResponseText {
    public ImapResponseCode Code {
      get; private set;
    }

    public ImapData[] Arguments {
      get; private set;
    }

    public string Text {
      get; private set;
    }

    internal ImapResponseText(string text)
    {
      if (text == null)
        throw new ArgumentNullException("text");

      this.Code = null;
      this.Arguments = null;
      this.Text = text;
    }

    internal ImapResponseText(ImapResponseCode code, ImapData[] arguments, string text)
    {
      if (code == null)
        throw new ArgumentNullException("code");
      if (arguments == null)
        throw new ArgumentNullException("arguments");
      if (text == null)
        throw new ArgumentNullException("text");

      this.Code = code;
      this.Arguments = arguments;
      this.Text = text;
    }

    /*
     * RFC 5255 - Internet Message Access Protocol Internationalization
     * http://tools.ietf.org/html/rfc5255
     */
    internal void ConvertTextToUTF8()
    {
      var bytes = NetworkTransferEncoding.Transfer8Bit.GetBytes(Text);

      Text = System.Text.Encoding.UTF8.GetString(bytes);
    }

    public override string ToString()
    {
      var args = (Arguments == null) ?
        string.Empty :
        string.Join(", ", Array.ConvertAll(Arguments, delegate(ImapData arg) {
          return arg.ToString();
        }));

      return string.Format("{{Code={0}, Arguments=[{1}], Text={2}}}", Code, args, Text);
    }
  }
}
