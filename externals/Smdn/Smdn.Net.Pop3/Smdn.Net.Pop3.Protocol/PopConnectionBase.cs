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
using System.Diagnostics;
using System.IO;

namespace Smdn.Net.Pop3.Protocol {
  public abstract class PopConnectionBase : ConnectionBase {
    /*
     * properties
     */
    protected PopSender Sender {
      get { return sender; }
    }

    protected PopReceiver Receiver {
      get { return receiver; }
    }

    protected PopConnectionBase(TraceSource traceSource)
      : base(traceSource)
    {
    }

    protected override void Connect(string host, int port, UpgradeConnectionStreamCallback createAuthenticatedStreamCallback)
    {
      try {
        base.Connect(host, port, createAuthenticatedStreamCallback);
      }
      catch (ConnectionException ex) {
        throw new PopConnectionException(ex.Message, ex.InnerException);
      }
    }

    public override void UpgradeStream(UpgradeConnectionStreamCallback upgradeStreamCallback)
    {
      try {
        base.UpgradeStream(upgradeStreamCallback);
      }
      catch (ConnectionException ex) {
        throw new PopUpgradeConnectionException(ex.Message, ex.InnerException);
      }
    }

    protected override LineOrientedBufferedStream CreateBufferedStream(Stream stream)
    {
      var bufferedStream = base.CreateBufferedStream(stream);

      receiver  = CreateReceiver(bufferedStream);
      sender    = CreateSender(bufferedStream);

      return bufferedStream;
    }

    protected abstract PopSender CreateSender(LineOrientedBufferedStream stream);

    protected abstract PopReceiver CreateReceiver(LineOrientedBufferedStream stream);

    private PopReceiver receiver = null;
    private PopSender sender = null;
  }
}
