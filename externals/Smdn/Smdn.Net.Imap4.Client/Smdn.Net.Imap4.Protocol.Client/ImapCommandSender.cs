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
using System.IO;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif

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

      var argCount = (command.CommandString == null ? 0 : 2) + command.Arguments.Length;

      if (argFragments.Length < argCount)
        argFragments = new ImapString[argCount];

      if (command.CommandString == null) {
        Array.Copy(command.Arguments, 0, argFragments, 0, command.Arguments.Length);
      }
      else {
        argFragments[0] = command.Tag;
        argFragments[1] = command.CommandString;

        Array.Copy(command.Arguments, 0, argFragments, 2, command.Arguments.Length);
      }

      Enqueue(true, argFragments.Take(argCount));

      Send();
    }

    private ImapString[] argFragments = new ImapString[8];
  }
}