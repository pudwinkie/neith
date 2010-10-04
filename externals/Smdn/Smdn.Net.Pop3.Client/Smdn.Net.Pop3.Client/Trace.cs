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
using System.Text;
using System.Reflection;
using System.Threading;

using Smdn.Net.Pop3.Protocol.Client;
using Smdn.Net.Pop3.Client.Session;
using Smdn.Net.Pop3.Client.Transaction;

namespace Smdn.Net.Pop3.Client {
  internal static class Trace {
    private static readonly TraceSource traceSource = new TraceSource(Assembly.GetExecutingAssembly().GetName().Name);

    private static string Format(string format, params object[] args)
    {
      return string.Format("{0:o} ", DateTime.Now) + string.Format(format, args);
    }

    [Conditional("TRACE")]
    public static void Info(string format, params object[] args)
    {
      traceSource.TraceEvent(TraceEventType.Information, Thread.CurrentThread.ManagedThreadId, Format(format, args));
    }

    [Conditional("TRACE")]
    public static void Verbose(string format, params object[] args)
    {
      traceSource.TraceEvent(TraceEventType.Verbose, Thread.CurrentThread.ManagedThreadId, Format(format, args));
    }

    [Conditional("TRACE")]
    public static void Log(Exception ex)
    {
      traceSource.TraceEvent(TraceEventType.Error, Thread.CurrentThread.ManagedThreadId, Format("EXCEPTION{0}{1}", Environment.NewLine, ex));
    }

    [Conditional("TRACE")]
    public static void Log(IPopTransaction transaction, string format, params object[] args)
    {
      Verbose("CID:{0} {1}: {2}",
              transaction.Connection.Id,
              transaction.GetType().Name.Replace("Transaction", string.Empty),
              args == null ? format : string.Format(format, args));
    }

    [Conditional("TRACE")]
    public static void LogRequest(IPopTransaction transaction)
    {
      var args = new StringBuilder();

      foreach (var p in transaction.RequestArguments) {
        args.AppendFormat("'{0}'=>'{1}'; ", p.Key, p.Value);
      }

      Log(transaction,
          args.ToString(),
          null);
    }

    [Conditional("TRACE")]
    public static void LogResponse(IPopTransaction transaction)
    {
      Log(transaction,
          "{0} {1}",
          transaction.Result.Code,
          transaction.Result.ResultText);
    }

    [Conditional("TRACE")]
    public static void Log(PopSession session, string format, params object[] args)
    {
      Info("CID:{0} {1}",
           session.Id.HasValue ? session.Id.Value.ToString() :  "-",
           args == null ? format : string.Format(format, args));
    }
  }
}
