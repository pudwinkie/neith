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

namespace Smdn.Net.Imap4.Protocol.Client {
  public enum ImapCommandResultCode : int {
    Default = 999,

    // 1xx Informational(not used)

    // 2xx Success
    Ok            = 200,  // tagged OK response
    PreAuth       = 201,  // tagged PREAUTH response
    RequestDone   = 250,  // request was succeeded with no operation

    // 3xx Redirection

    // 4xx Client error
    No            = 400,  // tagged NO response
    InternalError = 401,  // exception
    RequestError  = 402,  // missing or invalid arguments, unsupported feature etc.
    SocketTimeout = 408,  // socket timeout
    UpgradeError  = 426,  // upgrade connection failed
    ConnectionError = 450,  // connection error

    // 5xx Server error
    Bad           = 500,  // tagged BAD response
    Bye           = 501,  // untagged BYE response
    ResponseError = 502,  // missing or unexpected responce
  }
}
