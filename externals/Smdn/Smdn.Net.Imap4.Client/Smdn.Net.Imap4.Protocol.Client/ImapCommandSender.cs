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
using System.IO;

using Smdn.Net.Imap4.Protocol;

namespace Smdn.Net.Imap4.Protocol.Client {
  public sealed class ImapCommandSender : ImapSender {
    public bool CommandContinuing {
      get { return 0 < base.UnsentFragments; }
    }

    public ImapCommandSender(LineOrientedBufferedStream stream)
      : base(stream)
    {
    }

    public void Send(ImapCommand command)
    {
      ClearUnsent();

      var argCount = command.Arguments.Length + (command.CommandString == null ? 0 : 2);
      var argIndex = 0;
      var args = new ImapString[argCount];

      if (command.CommandString != null) {
        args[argIndex++] = command.Tag;
        args[argIndex++] = command.CommandString;
      }

      for (var i = 0; i < command.Arguments.Length; i++, argIndex++) {
        args[argIndex] = command.Arguments[i];
      }

      Enqueue(args);

      Enqueue(Smdn.Formats.Octets.CRLF);

      Send();
    }
  }
}