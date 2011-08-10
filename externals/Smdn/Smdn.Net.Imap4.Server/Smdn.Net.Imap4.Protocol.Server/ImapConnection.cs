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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Smdn.Net.Imap4.Server;

namespace Smdn.Net.Imap4.Protocol.Server {
  // Layers:
  //   Session.ImapSession
  //   Transaction.IImapTransaction/Transaction.ImapTransactionRequest/Transaction.ImapTransactionResult
  // * ImapConnection/ImapCommand/ImapResponse
  //   (IMAP4)

  public class ImapConnection : ImapConnectionBase {
    /*
     * class members
     */
    public const int DefaultTimeoutMilliseconds = 16000;

    /*
     * properties
     */
    internal new Socket Socket {
      get { return base.Socket; }
    }

    public override string Host {
      get { return RemoteEndPoint.ToString(); }
    }

    // for '5.4. Autologout Timer'
    public DateTime LastSentTime {
      get
      {
        CheckDisposed();
        return lastSentTime;
      }
    }

    public ImapConnection(TcpClient client)
      : this(client, null)
    {
    }

    public ImapConnection(TcpClient client, ImapUpgradeConnectionStreamCallback createAuthenticatedStreamCallback)
    {
      if (client == null)
        throw new ArgumentNullException("client");

      try {
        client.ReceiveTimeout = DefaultTimeoutMilliseconds;
        client.SendTimeout    = DefaultTimeoutMilliseconds;

        base.SetClient(client, createAuthenticatedStreamCallback);
      }
      catch {
        client.Close();

        throw;
      }

      Trace.Info("CID:{0} connected from {1}", Id, Host);
    }

#if TRACE
    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);

      if (disposing)
        Trace.Info("CID:{0} disconnected", Id);
    }

    protected override ImapBufferedStream CreateBufferedStream(Stream stream)
    {
      return new ImapBufferedStream(new TracingStream(this, stream));
    }

    private class TracingStream : Smdn.Net.MessageAccessProtocols.InterruptStream {
      public TracingStream(ImapConnection connection, Stream innerStream)
        : base(innerStream)
      {
        this.connection = connection;
      }

      protected override void OnWritten(byte[] src, int offset, int count)
      {
        Trace.LogSent(connection, src, offset, count);
      }

      protected override int OnRead(byte[] dest, int offset, int count)
      {
        Trace.LogReceived(connection, dest, offset, count);

        return count;
      }

      private ImapConnection connection;
    }
#endif

    internal new void UpgradeStream(ImapUpgradeConnectionStreamCallback upgradeStreamCallback)
    {
      base.UpgradeStream(upgradeStreamCallback);
    }

    internal void SetIsIdling(bool isIdling)
    {
      base.IsIdling = isIdling;
    }

    protected override ImapSender CreateSender(ImapBufferedStream stream)
    {
      return new ImapResponseSender(stream);
    }

    protected override ImapReceiver CreateReceiver(ImapBufferedStream stream)
    {
      return new ImapCommandReceiver(stream);
    }

#region "seinding / receiving"
    public void SendResponse(ImapResponse response)
    {
      try {
        var s = Sender as ImapResponseSender;

        s.Send(response);
      }
      catch (IOException ex) {
        if (ex.InnerException is SocketException)
          throw new ImapConnectionException((ex.InnerException as SocketException).Message, ex.InnerException);
        else
          throw ex;
      }
    }

    public ImapCommand TryReceiveCommand()
    {
      CheckDisposed();
      CheckConnected();

      // return queued command
      if (0 < commandQueue.Count)
        return commandQueue.Dequeue();

      // receive and parse command
      var r = Receiver as ImapCommandReceiver;

      for (;;) {
        try {
          var command = r.ReceiveCommand();

          if (command == null)
            break;

          commandQueue.Enqueue(command);

          if (!r.CommandContinuing)
            break;
        }
        catch (IOException ex) {
          var socketException = ex.InnerException as SocketException;

          if (socketException == null)
            // unexpected IO error
            throw ex;

          if (IsIdling && socketException.SocketErrorCode == SocketError.WouldBlock) {
            // expected socket exception
            Trace.Log(this, "idling");
            continue;
          }
          else {
            // unexpected socket exception
            throw new ImapConnectionException(socketException.Message, socketException.InnerException);
          }
        }
      }

      // return received command
      if (0 < commandQueue.Count)
        return commandQueue.Dequeue();

      return null;
    }
#endregion

    private DateTime lastSentTime = DateTime.MinValue;
    private /*readonly*/ Queue<ImapCommand> commandQueue = new Queue<ImapCommand>();
  }
}
