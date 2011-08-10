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

using Smdn.Net.Pop3.Protocol.Client;

namespace Smdn.Net.Pop3.Client.Transaction.BuiltIn {
  internal sealed class PassTransaction : PopTransactionBase, IPopExtension {
    PopCapability IPopExtension.RequiredCapability {
      get { return PopCapability.User; }
    }

    public PassTransaction(PopConnection connection)
      : base(connection)
    {
    }

    /*
     * 7. Optional POP3 Commands
     * PASS string
     *    Arguments:
     *        a server/mailbox-specific password (required)
     *    Restrictions:
     *        may only be given in the AUTHORIZATION state immediately
     *        after a successful USER command
     *    Possible Responses:
     *        +OK maildrop locked and ready
     *        -ERR invalid password
     *        -ERR unable to lock maildrop
     */
    protected override PopCommand PrepareCommand()
    {
#if DEBUG
      if (!RequestArguments.ContainsKey("string")) {
        FinishError(PopCommandResultCode.RequestError, "arguments 'string' must be setted");
        return null;
      }
      else
#endif
        return new PopCommand("PASS", RequestArguments["string"]);
    }
  }
}
