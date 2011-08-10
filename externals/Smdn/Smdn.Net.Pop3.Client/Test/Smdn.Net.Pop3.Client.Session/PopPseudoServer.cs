using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Smdn.IO;

namespace Smdn.Net.Pop3.Client.Session {
  public class PopPseudoServer : IDisposable {
    public IPEndPoint ServerEndPoint {
      get { return serverEndPoint; }
    }

    public string Host {
      get { return ServerEndPoint.Address.ToString(); }
    }

    public int Port {
      get { return ServerEndPoint.Port; }
    }

    public string HostPort {
      get { return string.Format("{0}:{1}", Host, Port); }
    }

    public bool IsStarted {
      get { return serverProcRunning != 0; }
    }

    public PopPseudoServer()
    {
      server = new TcpListener(new IPEndPoint(IPAddress.Loopback, 0));
      server.Start();

      serverEndPoint = server.LocalEndpoint as IPEndPoint;

      requestStream = new StrictLineOrientedStream(new MemoryStream());
    }

    public void Start()
    {
      if (IsStarted)
        throw new InvalidOperationException("already started");

      serverStopped = false;

      server.BeginAcceptTcpClient(AcceptClient, null);
    }

    public void Dispose()
    {
      serverStopped = true;

      if (server != null) {
        server.Stop();
        server = null;
      }

      if (serverProcWaitHandle != null) {
        serverProcWaitHandle.WaitOne();
        serverProcWaitHandle.Close();
        serverProcWaitHandle = null;
      }
    }

    public void Stop()
    {
      if (IsStarted) {
        serverStopped = true;

        serverProcWaitHandle.WaitOne();
      }
    }

    public void EnqueueResponse(string response)
    {
      responseQueue.Enqueue(response);
    }

    public string DequeueRequest()
    {
      for (var retry = 0; retry < 20; retry++) {
        if (readOffset == writeOffset)
          Thread.Sleep(100);
      }

      streamLock.AcquireReaderLock(1000);

      byte[] line;

      try {
        requestStream.Position = readOffset;

        line = requestStream.ReadLine(true);

        if (line == null)
          return null;

        readOffset += line.Length;
      }
      finally {
        streamLock.ReleaseReaderLock();
      }

      return NetworkTransferEncoding.Transfer8Bit.GetString(line);
    }

    private void AcceptClient(IAsyncResult asyncResult)
    {
      if (serverStopped) // XXX: ?
        return;

#pragma warning disable 0420
      Interlocked.Exchange(ref serverProcRunning, 1);
#pragma warning restore 0420

      serverProcWaitHandle.Reset();

      try {
        TcpClient client = null;

        for (;;) {
          if (asyncResult.AsyncWaitHandle.WaitOne(25)) {
            if (serverStopped) {
              Console.WriteLine("disposed before accept");
              return;
            }

            client = server.EndAcceptTcpClient(asyncResult);
            break;
          }
          else {
            if (serverStopped) {
              Console.WriteLine("stopped before accept");
              return;
            }
          }
        }

        Console.WriteLine("accepted");

        client.ReceiveTimeout = 250;

        ServerProc(client);
      }
      finally {
        if (serverProcWaitHandle != null)
          serverProcWaitHandle.Set();

#pragma warning disable 0420
        Interlocked.Exchange(ref serverProcRunning, 0);
#pragma warning restore 0420
      }
    }

    private void ServerProc(TcpClient client)
    {
      var stream = client.GetStream();

      for (;;) {
        if (serverStopped)
          break;

        if (responseQueue.Count == 0) {
          Thread.Sleep(25);
          continue;
        }

        // send response
        var responseText = responseQueue.Dequeue();
        var response = NetworkTransferEncoding.Transfer8Bit.GetBytes(responseText);

        try {
          stream.Write(response, 0, response.Length);
        }
        catch (IOException ex) {
          Console.WriteLine("failed at send, client disconnected?");
          Console.WriteLine(ex);
          return;
        }

        Console.WriteLine("S: {0}", responseText);

        // receive request
        var buffer = new byte[0x1000];

        for (;;) {
          if (serverStopped)
            break;

          try {
            var read = stream.Read(buffer, 0, buffer.Length);

            if (read == 0) {
              if (client.Connected) {
                continue;
              }
              else {
                Console.WriteLine("client disconnected");
                stream.Close();
                client.Close();
                return;
              }
            }

            Console.WriteLine("C: {0}", NetworkTransferEncoding.Transfer8Bit.GetString(buffer, 0, read));

            streamLock.AcquireWriterLock(1000);

            try {
              requestStream.Position = writeOffset;
              requestStream.Write(buffer, 0, read);
              requestStream.Flush();
              writeOffset += read;
            }
            finally {
              streamLock.ReleaseWriterLock();
            }

            if (read < buffer.Length)
              break;
          }
          catch (IOException ex) {
            var sockEx = ex.InnerException as SocketException;

            if (sockEx != null && (sockEx.SocketErrorCode == SocketError.WouldBlock || sockEx.SocketErrorCode == SocketError.TimedOut)) {
              break;
            }
            else {
              Console.WriteLine("failed at receive, client disconnected?");
              Console.WriteLine(ex);
              return;
            }
          }
        }
      }

      client.Client.Shutdown(SocketShutdown.Both);
      client.Close();

      if (stream != null)
        stream.Close();
    }

    private TcpListener server;
    private IPEndPoint serverEndPoint;
    private volatile int serverProcRunning = 0;
    private ManualResetEvent serverProcWaitHandle = new ManualResetEvent(true);
    private readonly Queue<string> responseQueue = new Queue<string>();
    private long writeOffset = 0L;
    private long readOffset = 0L;
    private LineOrientedStream requestStream;
    private ReaderWriterLock streamLock = new ReaderWriterLock();
    private volatile bool serverStopped = false;
  }
}
