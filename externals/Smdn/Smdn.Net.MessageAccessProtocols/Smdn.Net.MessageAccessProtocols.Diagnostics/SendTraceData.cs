// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2010-2011 smdn
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

namespace Smdn.Net.MessageAccessProtocols.Diagnostics {
#if TRACE
  public sealed class SendTraceData : TraceDataBase {
    public ArraySegment<byte> Data {
      get; private set;
    }

    public IPEndPoint RemoteEndPoint {
      get; private set;
    }

    public IPEndPoint LocalEndPoint {
      get; private set;
    }

    internal SendTraceData(ArraySegment<byte> data,
                           IPEndPoint remoteEndPoint,
                           IPEndPoint localEndPoint,
                           int connectionId)
      : base(connectionId)
    {
      Data = data;
      RemoteEndPoint = remoteEndPoint;
      LocalEndPoint = localEndPoint;
    }

    public override string ToString()
    {
      var sent = NetworkTransferEncoding.Transfer8Bit.GetString(Data.Array,
                                                                Data.Offset,
                                                                Data.Count);

      return string.Format("{0:o} CID:{1} [{2, 17}] C: {3}",
                           DateTime,
                           ConnectionId,
                           LocalEndPoint,
                           sent.TrimEnd());
    }
  }
#endif
}
