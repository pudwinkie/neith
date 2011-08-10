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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using Smdn.Net.Pop3.Client;

namespace Smdn.Net.Pop3.Protocol.Client {
  public sealed class PopConnection : PopConnectionBase {
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
      get { return popConnectionTraceSource; }
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
    private static readonly TraceSource popConnectionTraceSource = new TraceSource("POP");

    /*
     * properties
     */

    // for autologout timer
    public DateTime LastSentTime {
      get { return lastSentTime; }
    }

    internal bool HandleResponseAsMultiline {
      get { return (Receiver as PopResponseReceiver).HandleAsMultiline; }
      set { (Receiver as PopResponseReceiver).HandleAsMultiline = value; }
    }

    public PopConnection(string host,
                         int port,
                         int millisecondsTimeout,
                         UpgradeConnectionStreamCallback createAuthenticatedStreamCallback)
      : base(popConnectionTraceSource)
    {
      if (port == -1) {
        if (createAuthenticatedStreamCallback == null)
          port = PopDefaultPorts.Pop;
        else
          port = PopDefaultPorts.Pops;
      }

      Connect(host,
              port,
              millisecondsTimeout,
              createAuthenticatedStreamCallback);
    }

    internal void SetIsSecureConnection(bool isSecureConnection)
    {
      base.IsSecureConnection = isSecureConnection;
    }

    protected override PopSender CreateSender(LineOrientedBufferedStream stream)
    {
      return new PopCommandSender(stream);
    }

    protected override PopReceiver CreateReceiver(LineOrientedBufferedStream stream)
    {
      return new PopResponseReceiver(stream);
    }

#region "seinding / receiving"
    public void SendCommand(PopCommand command)
    {
      CheckConnected();

      try {
        (Sender as PopCommandSender).Send(command);

        lastSentTime = DateTime.Now;
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

    public PopResponse TryReceiveResponse()
    {
      CheckConnected();

      try {
        // receive and parse response
        return (Receiver as PopResponseReceiver).ReceiveResponse();
      }
      catch (SocketException ex) {
        throw CreateSocketErrorException(ex, true);
      }
      catch (IOException ex) {
        var socketException = ex.InnerException as SocketException;

        if (socketException == null)
          // unexpected IO error
          throw;
        else
          throw CreateSocketErrorException(socketException, true);
      }
    }

    public bool TryReceiveLine(Stream stream)
    {
      CheckConnected();

      try {
        return (Receiver as PopResponseReceiver).ReceiveLine(stream);
      }
      catch (IOException ex) {
        var socketException = ex.InnerException as SocketException;

        if (socketException == null)
          // unexpected IO error
          throw;
        else
          throw CreateSocketErrorException(socketException, true);
      }
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
          return new PopConnectionException(string.Format("unexpected socket error ({0})", ex.SocketErrorCode),
                                            ex);
      }
    }
#endregion

    private DateTime lastSentTime = DateTime.Now;
  }
}
