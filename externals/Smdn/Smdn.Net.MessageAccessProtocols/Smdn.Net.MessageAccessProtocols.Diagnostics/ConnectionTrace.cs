// 
// Author:
//       smdn <smdn@mail.invisiblefulmoon.net>
// 
// Copyright (c) 2010 smdn
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
using System.Reflection;

namespace Smdn.Net.MessageAccessProtocols.Diagnostics {
  internal static class ConnectionTrace {
    [Conditional("TRACE")]
    public static void Log(ConnectionBase connection, Exception ex)
    {
      connection.traceSource.TraceData(TraceEventType.Error,
                                       connection.Id,
                                       new ExceptionTraceData(ex, connection.Id));
    }

    [Conditional("TRACE")]
    public static void Log(ConnectionBase connection, string format, params object[] args)
    {
      connection.traceSource.TraceData(TraceEventType.Information,
                                       connection.Id,
                                       new MessageTraceData(string.Format(format, args), connection.Id));
    }

    [Conditional("TRACE")]
    private static void LogReceived(ConnectionBase connection, byte[] received, int start, int count)
    {
      connection.traceSource.TraceData(TraceEventType.Verbose,
                                       connection.Id,
                                       new ReceiveTraceData(new ArraySegment<byte>(received, start, count),
                                                            connection.RemoteEndPoint,
                                                            connection.LocalEndPoint,
                                                            connection.Id));
    }

    [Conditional("TRACE")]
    private static void LogSent(ConnectionBase connection, byte[] sent, int start, int count)
    {
      connection.traceSource.TraceData(TraceEventType.Verbose,
                                       connection.Id,
                                       new SendTraceData(new ArraySegment<byte>(sent, start, count),
                                                         connection.RemoteEndPoint,
                                                         connection.LocalEndPoint,
                                                         connection.Id));
    }

    internal static Stream CreateTracingStream(ConnectionBase connection, Stream stream)
    {
#if TRACE
      return new TracingStream(connection, stream);
#else
      return stream;
#endif
    }

#if TRACE
    internal sealed class TracingStream : InterruptStream {
      public TracingStream(ConnectionBase connection, Stream innerStream)
        : base(innerStream)
      {
        this.connection = connection;
      }

      protected override void OnWritten(byte[] src, int offset, int count)
      {
        ConnectionTrace.LogSent(connection, src, offset, count);
      }

      protected override int OnRead(byte[] dest, int offset, int count)
      {
        ConnectionTrace.LogReceived(connection, dest, offset, count);

        return count;
      }

      private ConnectionBase connection;
    }
#endif
  }
}
