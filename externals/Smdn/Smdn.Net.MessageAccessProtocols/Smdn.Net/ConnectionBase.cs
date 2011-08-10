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
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

using Smdn.Net.MessageAccessProtocols.Diagnostics;

namespace Smdn.Net {
  public abstract class ConnectionBase : IDisposable, IConnectionInfo {
    private static int connectionIdCount = 0;

    internal readonly TraceSource traceSource;

    public bool IsClosed {
      get { return client == null; }
    }

    public int Id {
      get { return id; }
    }

    public string Host {
      get { CheckConnected(); return host; }
    }

    public int Port {
      get { return RemoteEndPoint.Port; }
    }

    public IPEndPoint RemoteEndPoint {
      get { CheckConnected(); return remoteEndPoint; }
    }

    public IPEndPoint LocalEndPoint {
      get { CheckConnected(); return localEndPoint; }
    }

    public int SendTimeout {
      get { CheckConnected(); return sendTimeout; }
      set
      {
        Socket.SendTimeout = (value == 0) ? 1 : value;
        sendTimeout = value;
      }
    }

    public int ReceiveTimeout {
      get { CheckConnected(); return receiveTimeout; }
      set
      {
        Socket.ReceiveTimeout = (value == 0) ? 1 : value;
        receiveTimeout = value;
      }
    }

    public bool IsSecurePortConnection {
      get { CheckConnected(); return isSecurePortConnection; }
      protected set { isSecurePortConnection = value; }
    }

    public bool IsSecureConnection {
      get { CheckConnected(); return isSecureConnection; }
      protected set { isSecureConnection = value; }
    }

    protected virtual Stream RawStream {
      get
      {
        CheckConnected();

#if TRACE
        if (stream.InnerStream is ConnectionTrace.TracingStream)
          return (stream.InnerStream as ConnectionTrace.TracingStream).InnerStream;
        else
#endif
          return stream.InnerStream;
      }
    }

    protected LineOrientedBufferedStream Stream {
      get { CheckConnected(); return stream; }
    }

    protected Socket Socket {
      get { CheckConnected(); return client; }
    }

    protected ConnectionBase(TraceSource traceSource)
    {
      this.id = Interlocked.Increment(ref connectionIdCount);

      Interlocked.CompareExchange(ref connectionIdCount, 0, int.MaxValue);

      this.traceSource = traceSource;
    }

    ~ConnectionBase()
    {
      Dispose(false);
    }

    void IDisposable.Dispose()
    {
      Close();
    }

    public void Close()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing) {
        if (stream != null) {
          stream.Close();
          stream = null;
        }

        if (client != null) {
          client.Close();
          client = null;

          ConnectionTrace.Log(this, "disconnected: {0} ({1})", remoteEndPoint, host);
        }
      }
    }

    protected virtual void Connect(string host,
                                   int port,
                                   int millisecondsTimeout,
                                   UpgradeConnectionStreamCallback createAuthenticatedStreamCallback)
    {
      if (host == null)
        throw new ArgumentNullException("host");
      if (port < IPEndPoint.MinPort || IPEndPoint.MaxPort < port)
        throw ExceptionUtils.CreateArgumentMustBeInRange(IPEndPoint.MinPort, IPEndPoint.MaxPort, "port", port);
      if (millisecondsTimeout < -1)
        throw ExceptionUtils.CreateArgumentMustBeGreaterThanOrEqualTo(-1, "millisecondsTimeout", millisecondsTimeout);

      if (client != null)
        throw new InvalidOperationException("already connected");

      Socket c = null;

      try {
        ConnectionTrace.Log(this, "connecting to {0}:{1}", host, port);

        c = (new Connector()).Connect(host, port, millisecondsTimeout);

        if (c == null)
          throw new TimeoutException("connect timeout");

        this.host = host;
        this.client = c;
        this.remoteEndPoint = c.RemoteEndPoint as IPEndPoint;
        this.localEndPoint  = c.LocalEndPoint as IPEndPoint;
        this.SendTimeout = Timeout.Infinite;
        this.ReceiveTimeout = Timeout.Infinite;
        this.stream = CreateBufferedStream(ConnectionTrace.CreateTracingStream(this, new NetworkStream(c, false)));

        ConnectionTrace.Log(this, "connected: {0} ({1})", remoteEndPoint, host);
      }
      catch (Exception ex) {
        ConnectionTrace.Log(this, ex);

        if (c != null)
          c.Close();

        this.stream = null;
        this.client = null;

        throw CreateConnectException("connect failed", ex);
      }

      try {
        if (createAuthenticatedStreamCallback != null) {
          UpgradeStream(createAuthenticatedStreamCallback);

          isSecurePortConnection = true;
        }
      }
      catch {
        this.stream.Close();
        this.stream = null;

        this.client.Close();
        this.client = null;

        throw;
      }
    }

    private class Connector {
      public Socket Connect(string host, int port, int millisecondsTimeout)
      {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        if (millisecondsTimeout == Timeout.Infinite) {
          socket.Connect(host, port);

          return socket;
        }

        // use Connect+BeginInvoke instead of BeginConnect
        Action<string, int> asyncConnect = new Action<string, int>(socket.Connect);

        asyncResult = asyncConnect.BeginInvoke(host, port, null, null);

        if (asyncResult.IsCompleted ||
            asyncResult.AsyncWaitHandle.WaitOne(millisecondsTimeout, false)) {
          asyncConnect.EndInvoke(asyncResult);

          return socket;
        }
        else {
          cleanupWaitHandle = ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle,
                                                                     CleanUp,
                                                                     this,
                                                                     Timeout.Infinite,
                                                                     true);

          return null;
        }
      }

      private static void CleanUp(object state, bool timedOut)
      {
        var c = state as Connector;

        try {
          var asyncConnect = (Action<string, int>)(c.asyncResult as AsyncResult).AsyncDelegate;

          asyncConnect.EndInvoke(c.asyncResult);

          c.asyncResult = null;
        }
        catch {
          // ignore exceptions
        }

        try {
          if (c.socket != null)
            c.socket.Close();
        }
        catch {
          // ignore exceptions
        }

        if (c.cleanupWaitHandle != null)
          c.cleanupWaitHandle.Unregister(null);
      }

      private Socket socket;
      private IAsyncResult asyncResult;
      private RegisteredWaitHandle cleanupWaitHandle;
    }

    public static Stream CreateClientSslStream(ConnectionBase connection,
                                               Stream baseStream,
                                               X509Certificate2Collection clientCertificates,
                                               RemoteCertificateValidationCallback serverCertificateValidationCallback,
                                               LocalCertificateSelectionCallback clientCertificateSelectionCallback)
    {
      var sslStream = new SslStream(baseStream,
                                    false,
                                    serverCertificateValidationCallback,
                                    clientCertificateSelectionCallback);

      try {
        sslStream.AuthenticateAsClient(connection.host,
                                       clientCertificates,
                                       SslProtocols.Default,
                                       true);

        return sslStream;
      }
      catch (AuthenticationException) {
        sslStream.Close();

        throw;
      }
    }

    public virtual void UpgradeStream(UpgradeConnectionStreamCallback upgradeStreamCallback)
    {
      if (upgradeStreamCallback == null)
        throw new ArgumentNullException("upgradeStreamCallback");

      try {
        var upgradedStream = upgradeStreamCallback(this, RawStream);
        var sslStream = upgradedStream as SslStream;

        if (sslStream != null)
          ConnectionTrace.Log(this, "TLS started: {0}, {1}, {2}, {3}{4}",
                              sslStream.SslProtocol,
                              sslStream.CipherAlgorithm,
                              sslStream.HashAlgorithm,
                              sslStream.KeyExchangeAlgorithm,
                              sslStream.IsMutuallyAuthenticated ? ", mutually authenticated" : string.Empty);

        stream = CreateBufferedStream(ConnectionTrace.CreateTracingStream(this, upgradedStream));
      }
      catch (AuthenticationException ex) {
        ConnectionTrace.Log(this, ex);

        throw CreateUpgradeStreamException(string.Format("upgrading stream failed (callback: {0})",
                                                         upgradeStreamCallback.Method),
                                           ex);
      }
      catch (IOException ex) {
        ConnectionTrace.Log(this, ex);

        throw CreateUpgradeStreamException(string.Format("upgrading stream failed (callback: {0})",
                                                         upgradeStreamCallback.Method),
                                           ex);
      }
    }

    protected virtual LineOrientedBufferedStream CreateBufferedStream(Stream stream)
    {
      return new LineOrientedBufferedStream(stream);
    }

    protected abstract Exception CreateConnectException(string message, Exception innerException);
    protected abstract Exception CreateUpgradeStreamException(string message, Exception innerException);

    public override string ToString()
    {
      if (client == null)
        return string.Format("{{not connected}}");
      else
        return string.Format("{{Host={0}, RemoteEndPoint={1}, LocalEndPoint={2}, Connected={3}}}",
                             Host,
                             remoteEndPoint,
                             localEndPoint,
                             Socket.Connected);
    }

    protected void CheckConnected()
    {
      if (client == null)
        throw new InvalidOperationException("not connected");
    }

    private int id;
    private string host = null;
    private IPEndPoint remoteEndPoint;
    private IPEndPoint localEndPoint;
    private int sendTimeout = Timeout.Infinite;
    private int receiveTimeout = Timeout.Infinite;
    private bool isSecurePortConnection;
    private bool isSecureConnection;
    private Socket client = null;
    private LineOrientedBufferedStream stream = null;
  }
}
