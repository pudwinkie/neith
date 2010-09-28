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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Threading;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using Smdn.Net.Imap4.Client;

namespace Smdn.Net.Imap4.Protocol.Client {
  // Layers:
  //   Session.ImapSession
  //   Transaction.IImapTransaction
  // * Protocol.ImapConnection/Protocol.ImapCommand/Protocol.ImapResponse
  //   (IMAP4)

  public sealed class ImapConnection : ImapConnectionBase {
    /*
     * class members
     */
    public static X509Certificate2Collection ClientCertificates {
      get { return clientCertificates; }
    }

    public static RemoteCertificateValidationCallback ServerCertificateValidationCallback {
      get; set;
    }

    public static LocalCertificateSelectionCallback ClientCertificateSelectionCallback {
      get; set;
    }

    public static TraceSource TraceSource {
      get { return imapConnectionTraceSource; }
    }

    public static Stream CreateSslStream(ConnectionBase connection, Stream baseStream)
    {
      return ConnectionBase.CreateClientSslStream(connection,
                                                  baseStream,
                                                  clientCertificates,
                                                  ServerCertificateValidationCallback,
                                                  ClientCertificateSelectionCallback);
    }

    private static readonly X509Certificate2Collection clientCertificates = new X509Certificate2Collection();
    private static readonly TraceSource imapConnectionTraceSource = new TraceSource("IMAP");

    /*
     * properties
     */

    // for '5.4. Autologout Timer'
    public DateTime LastSentTime {
      get { return lastSentTime; }
    }

    /*
     * RFC 5255 - Internet Message Access Protocol Internationalization
     * http://tools.ietf.org/html/rfc5255
     */
    public bool ReceiveResponseAsUTF8 {
      get { return (Receiver as ImapResponseReceiver).ReceiveResponseAsUTF8; }
      set { (Receiver as ImapResponseReceiver).ReceiveResponseAsUTF8 = value; }
    }

    public ImapConnection(string host)
      : this(host, -1, null)
    {
    }

    public ImapConnection(string host, int port)
      : this(host, port, null)
    {
    }

    public ImapConnection(string host, UpgradeConnectionStreamCallback createAuthenticatedStreamCallback)
      : this(host, -1, createAuthenticatedStreamCallback)
    {
    }

    public ImapConnection(string host, int port, UpgradeConnectionStreamCallback createAuthenticatedStreamCallback)
      : base(imapConnectionTraceSource)
    {
      if (port == -1) {
        if (createAuthenticatedStreamCallback == null)
          port = ImapDefaultPorts.Imap;
        else
          port = ImapDefaultPorts.Imaps;
      }

      Connect(host, port, createAuthenticatedStreamCallback);
    }

    internal void SetIsIdling(bool isIdling)
    {
      base.IsIdling = isIdling;
    }

    internal void SetIsSecureConnection(bool isSecureConnection)
    {
      base.IsSecureConnection = isSecureConnection;
    }

    protected override ImapSender CreateSender(LineOrientedBufferedStream stream)
    {
      return new ImapCommandSender(stream);
    }

    protected override ImapReceiver CreateReceiver(LineOrientedBufferedStream stream)
    {
      return new ImapResponseReceiver(stream);
    }

    public ImapCommand CreateCommand(string command, params ImapString[] arguments)
    {
      var comm = new ImapCommand(command, Interlocked.Increment(ref commandTag).ToString("x4"), arguments);

      Interlocked.CompareExchange(ref commandTag, 0, 0x10000);

      return comm;
    }

    public ImapCommand CreateContinuingCommand(params ImapString[] arguments)
    {
      return new ImapCommand(null, null, arguments);
    }

#region "seinding / receiving"
    public interface ICommandContinuationContext {
    }

    private class SendCommandContext : ICommandContinuationContext {
      public ImapCommand Command;
      public bool Sent;

      public SendCommandContext(ImapCommand command)
      {
        this.Command = command;
        this.Sent = false;
      }
    }

    public ICommandContinuationContext SendCommand(ImapCommand command)
    {
      return SendCommand(new SendCommandContext(command));
    }

    public ICommandContinuationContext SendCommand(ICommandContinuationContext context)
    {
      CheckConnected();

      try {
        var c = context as SendCommandContext;
        var s = Sender as ImapCommandSender;

        if (c.Sent) {
          s.Send();
        }
        else {
          s.Send(c.Command);
          c.Sent = true;
        }

        lastSentTime = DateTime.Now;

        if (s.CommandContinuing)
          return c;
        else
          return null;
      }
      catch (SocketException ex) {
        throw CreateSocketErrorException(ex, false);
      }
      catch (IOException ex) {
        var socketException = ex.InnerException as SocketException;

        if (socketException == null)
          // unexpected IO error
          throw;
        else
          throw CreateSocketErrorException(socketException, false);
      }
    }

    public ImapResponse TryReceiveResponse()
    {
      CheckConnected();

      // return queued response
      if (0 < responseQueue.Count)
        return responseQueue.Dequeue();

      // receive and parse response
      var r = Receiver as ImapResponseReceiver;

      for (;;) {
        try {
          var response = r.ReceiveResponse();

          if (response == null)
            break;

          responseQueue.Enqueue(response);

          if (!r.ResponseContinuing)
            break;
        }
        catch (SocketException ex) {
          throw CreateSocketErrorException(ex, true);
        }
        catch (IOException ex) {
          var socketException = ex.InnerException as SocketException;

          if (socketException == null)
            // unexpected IO error
            throw;

          var exp = CreateSocketErrorException(socketException, true);

          if (IsIdling && exp is TimeoutException) {
            // expected timeout
            Smdn.Net.Imap4.Client.Trace.Verbose("idling");
            continue;
          }
          else {
            throw exp;
          }
        }
      }

      // return received response
      if (0 < responseQueue.Count)
        return responseQueue.Dequeue();

      return null;
    }

    private Exception CreateSocketErrorException(SocketException ex, bool receiving)
    {
      switch (ex.SocketErrorCode) {
        case SocketError.WouldBlock:
        case SocketError.TimedOut:
          // timeout error
          return new TimeoutException(receiving ? "receive timeout" : "send timeout", ex);

        default:
          // unexpected socket exception
          return new ImapConnectionException("unexpected socket error", ex);
      }
    }
#endregion

    private DateTime lastSentTime = DateTime.MinValue;
    private int commandTag = -1;
    private /*readonly*/ Queue<ImapResponse> responseQueue = new Queue<ImapResponse>();
  }
}
