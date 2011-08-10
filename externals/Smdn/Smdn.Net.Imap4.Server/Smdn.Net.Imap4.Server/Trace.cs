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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Reflection;
using System.Threading;

using Smdn.Net.Imap4.Protocol.Server;
//using Smdn.Net.Imap4.Server.Transaction;
using Smdn.Net.Imap4.Server.Session;

namespace Smdn.Net.Imap4.Server {
  internal static class Trace {
    private static readonly TraceSource           traceSource = new TraceSource(Assembly.GetExecutingAssembly().GetName().Name);
    private static readonly TraceSource connectionTraceSource = new TraceSource(Assembly.GetExecutingAssembly().GetName().Name + "#Connection");

    internal static TraceSource ConnectionTraceSource {
      get { return connectionTraceSource; }
    }

    private static string Now()
    {
      return string.Format("{0:s}.{1:d4} ", DateTime.Now, DateTime.Now.Millisecond);
    }

    [Conditional("TRACE")]
    public static void Info(string format, params object[] args)
    {
      traceSource.TraceEvent(TraceEventType.Information, Thread.CurrentThread.ManagedThreadId, Now() + format, args);
    }

    [Conditional("TRACE")]
    public static void Verbose(string format, params object[] args)
    {
      traceSource.TraceEvent(TraceEventType.Verbose, Thread.CurrentThread.ManagedThreadId, Now() + format, args);
    }

    [Conditional("TRACE")]
    public static void Error(string format, params object[] args)
    {
      traceSource.TraceEvent(TraceEventType.Error, Thread.CurrentThread.ManagedThreadId, Now() + format, args);
    }

    [Conditional("TRACE")]
    public static void Log(Exception ex)
    {
      traceSource.TraceEvent(TraceEventType.Error, Thread.CurrentThread.ManagedThreadId, "EXCEPTION {0}{1}", Environment.NewLine, ex);
    }

    [Conditional("TRACE")]
    public static void Log(ImapConnection connection, string format, params object[] args)
    {
      Verbose("CID:{0} [IMAP4rev1] {1}",
              connection.Id,
              string.Format(format, args));
    }

    [Conditional("TRACE")]
    public static void LogReceived(ImapConnection connection, byte[] received, int start, int count)
    {
      var recv = ImapEncoding.Transfer8Bit.GetString(received, start, count);

      connectionTraceSource.TraceEvent(TraceEventType.Verbose, connection.Id, "[{0, 17}] {1}C: {2}",
                                       connection.RemoteEndPoint,
                                       Now(),
                                       recv.TrimEnd());

      var reader = new StringReader(recv);

      for (;;) {
        var line = reader.ReadLine();

        if (line == null)
          break;

        Log(connection, "    < {0}", line);
      }
    }

    [Conditional("TRACE")]
    public static void LogSent(ImapConnection connection, byte[] sent, int start, int count)
    {
      var snt = ImapEncoding.Transfer8Bit.GetString(sent, start, count);

      connectionTraceSource.TraceEvent(TraceEventType.Verbose, connection.Id, "[{0, 17}] {1}S: {2}",
                                       connection.LocalEndPoint,
                                       Now(),
                                       snt.TrimEnd());

      var reader = new StringReader(snt);

      for (;;) {
        var line = reader.ReadLine();

        if (line == null)
          break;

        Log(connection, "    > {0}", line);
      }
    }

#if false
    [Conditional("TRACE")]
    public static void Log(IImapTransaction transaction, string format, params object[] args)
    {
      Verbose("CID:{0} [Transaction] {1} {2}",
              transaction.Connection.Id,
              transaction.GetType().Name.Replace("Transaction", string.Empty),
              args == null ? format : string.Format(format, args));
    }

    [Conditional("TRACE")]
    public static void LogRequest(IImapTransaction transaction)
    {
      var args = ":";

      foreach (var p in transaction.Request.Arguments) {
        var val = p.Value.ToString();

        if (0x100 < val.Length)
          val = val.Substring(0, 0x100) + " ...";

        args += string.Format(" '{0}'=>'{1}';", p.Key, val);
      }

      Log(transaction, args, null);
    }

    [Conditional("TRACE")]
    public static void LogResponse(IImapTransaction transaction)
    {
      Log(transaction,
          "{0} '{1}'",
          transaction.Result.Code,
          transaction.Result.Description);
    }
#endif

    [Conditional("TRACE")]
    public static void Log(ImapSession session, string format, params object[] args)
    {
      Verbose("CID:{0} [Session] {1}",
              session.Connection == null ? "-" : session.Connection.Id.ToString(),
              args == null ? format : string.Format(format, args));
    }
  }
}
