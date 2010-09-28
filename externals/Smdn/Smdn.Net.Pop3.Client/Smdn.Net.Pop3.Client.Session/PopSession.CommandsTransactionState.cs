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
using System.IO;

using Smdn.Net.Pop3.Protocol.Client;
using Smdn.Net.Pop3.Client.Transaction.BuiltIn;

namespace Smdn.Net.Pop3.Client.Session {
  partial class PopSession {
    /*
     * transaction methods : transaction state
     */

    /// <summary>sends STAT command</summary>
    /// <remarks>valid in transaction state</remarks>
    public PopCommandResult Stat(out PopDropListing dropListing)
    {
      RejectNonTransactionState();

      dropListing = PopDropListing.Empty;

      using (var t = new StatTransaction(connection)) {
        if (ProcessTransaction(t).Succeeded)
          dropListing = t.Result.Value;

        return t.Result;
      }
    }

    /// <summary>sends LIST command</summary>
    /// <remarks>valid in transaction state</remarks>
    public PopCommandResult List(out PopScanListing[] scanListings)
    {
      return ListCore(null, out scanListings);
    }

    /// <summary>sends LIST command</summary>
    /// <remarks>valid in transaction state</remarks>
    public PopCommandResult List(long messageNumber, out PopScanListing scanListing)
    {
      PopScanListing[] scanListings;

      var ret = ListCore(messageNumber, out scanListings);

      if (ret.Succeeded)
        scanListing = scanListings[0];
      else
        scanListing = PopScanListing.Invalid;

      return ret;
    }

    private PopCommandResult ListCore(long? messageNumber, out PopScanListing[] scanListings)
    {
      RejectNonTransactionState();

      if (messageNumber.HasValue && messageNumber.Value <= 0L)
        throw new ArgumentOutOfRangeException("messageNumber", messageNumber.Value, "must be non-zero positive number");

      scanListings = null;

      using (var t = new ListTransaction(connection)) {
        if (messageNumber.HasValue)
          t.RequestArguments["msg"] = messageNumber.Value.ToString();

        if (ProcessTransaction(t).Succeeded)
          scanListings = t.Result.Value;

        return t.Result;
      }
    }

    /// <summary>sends RETR command</summary>
    /// <remarks>valid in transaction state</remarks>
    public PopCommandResult Retr(long messageNumber, out Stream messageStream)
    {
      if (messageNumber <= 0L)
        throw new ArgumentOutOfRangeException("messageNumber", messageNumber, "must be non-zero positive number");

      RejectNonTransactionState();

      messageStream = null;

      using (var t = new RetrTransaction(connection)) {
        t.RequestArguments["msg"] = messageNumber.ToString();

        if (ProcessTransaction(t).Succeeded)
          messageStream = t.Result.Value;

        return t.Result;
      }
    }

    /// <summary>sends DELE command</summary>
    /// <remarks>valid in transaction state</remarks>
    public PopCommandResult Dele(long messageNumber)
    {
      if (messageNumber <= 0L)
        throw new ArgumentOutOfRangeException("messageNumber", messageNumber, "must be non-zero positive number");

      RejectNonTransactionState();

      using (var t = new DeleTransaction(connection)) {
        t.RequestArguments["msg"] = messageNumber.ToString();

        return ProcessTransaction(t);
      }
    }

    /// <summary>sends NOOP command</summary>
    /// <remarks>valid in transaction state</remarks>
    public PopCommandResult NoOp()
    {
      RejectNonTransactionState();

      using (var t = new NoOpTransaction(connection)) {
        return ProcessTransaction(t);
      }
    }

    /// <summary>sends RSET command</summary>
    /// <remarks>valid in transaction state</remarks>
    public PopCommandResult Rset()
    {
      RejectNonTransactionState();

      using (var t = new RsetTransaction(connection)) {
        return ProcessTransaction(t);
      }
    }

    /// <summary>sends TOP command</summary>
    /// <remarks>valid in transaction state</remarks>
    public PopCommandResult Top(long messageNumber, int lines, out Stream messageStream)
    {
      if (messageNumber <= 0L)
        throw new ArgumentOutOfRangeException("messageNumber", messageNumber, "must be non-zero positive number");
      if (lines < 0)
        throw new ArgumentOutOfRangeException("lines", lines, "must be zero or positive number");

      RejectNonTransactionState();

      messageStream = null;

      using (var t = new TopTransaction(connection)) {
        t.RequestArguments["msg"] = messageNumber.ToString();
        t.RequestArguments["n"] = lines.ToString();

        if (ProcessTransaction(t).Succeeded)
          messageStream = t.Result.Value;

        return t.Result;
      }
    }

    /// <summary>sends UIDL command</summary>
    /// <remarks>valid in transaction state</remarks>
    public PopCommandResult Uidl(out PopUniqueIdListing[] uniqueIdListings)
    {
      return UidlCore(null, out uniqueIdListings);
    }

    /// <summary>sends UIDL command</summary>
    /// <remarks>valid in transaction state</remarks>
    public PopCommandResult Uidl(long messageNumber, out PopUniqueIdListing uniqueIdListing)
    {
      PopUniqueIdListing[] uniqueIdListings;

      var ret = UidlCore(messageNumber, out uniqueIdListings);

      if (ret.Succeeded)
        uniqueIdListing = uniqueIdListings[0];
      else
        uniqueIdListing = PopUniqueIdListing.Invalid;

      return ret;
    }

    private PopCommandResult UidlCore(long? messageNumber, out PopUniqueIdListing[] uniqueIdListings)
    {
      RejectNonTransactionState();

      if (messageNumber.HasValue && messageNumber.Value <= 0L)
        throw new ArgumentOutOfRangeException("messageNumber", messageNumber.Value, "must be non-zero positive number");

      uniqueIdListings = null;

      using (var t = new UidlTransaction(connection)) {
        if (messageNumber.HasValue)
          t.RequestArguments["msg"] = messageNumber.Value.ToString();

        if (ProcessTransaction(t).Succeeded)
          uniqueIdListings = t.Result.Value;

        return t.Result;
      }
    }
  }
}
