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
using System.Net;

using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Transaction.BuiltIn {
  internal sealed class LoginTransaction : ImapTransactionBase {
    public LoginTransaction(ImapConnection connection, NetworkCredential credential)
      : base(connection)
    {
      if (credential == null)
        throw new ArgumentNullException("credential");

      this.credential = credential;
    }

    // 6.2.3. LOGIN Command
    //    Arguments:  user name
    //                password
    //    Responses:  no specific responses for this command
    //    Result:     OK - login completed, now in authenticated state
    //                NO - login failure: user name or password rejected
    //                BAD - command unknown or arguments invalid
    protected override ImapCommand PrepareCommand()
    {
      return Connection.CreateCommand("LOGIN",
                                      string.IsNullOrEmpty(credential.UserName)
                                        ? ImapQuotedString.Empty
                                        : new ImapString(credential.UserName),
                                      string.IsNullOrEmpty(credential.Password)
                                        ? ImapQuotedString.Empty
                                        : new ImapString(credential.Password));
    }

    private NetworkCredential credential;
  }
}
