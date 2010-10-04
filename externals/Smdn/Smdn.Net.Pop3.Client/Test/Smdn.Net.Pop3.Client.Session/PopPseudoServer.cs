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
      get { return server.LocalEndpoint as IPEndPoint; } 
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

    public PopPseudoServer()
    {
      server = new TcpListener(new IPEndPoint(IPAddress.Loopback, 0));
      server.Start();

      requestStream = new StrictLineOrientedStream(new MemoryStream());
    }

    public void Start()
    {
      thread = new Thread(ServerThread);
      thread.Start();
    }

    public void Dispose()
    {
      Stop();
    }

    public void Stop()
    {
      if (thread != null) {
        thread.Abort();
        thread = null;
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

    private void ServerThread()
    {
      var client = server.AcceptTcpClient();

      Console.WriteLine("accepted");

      NetworkStream stream = null;

      try {
        stream = client.GetStream();

        for (;;) {
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
              if (!(ex.InnerException is ThreadAbortException)) {
                Console.WriteLine("failed at receive, client disconnected?");
                Console.WriteLine(ex);
              }
              return;
            }
          }
        }
      }
      catch (ThreadAbortException) {
        client.Close();

        if (stream != null)
          stream.Close();
      }
    }

    private TcpListener server;
    private Thread thread;
    private readonly Queue<string> responseQueue = new Queue<string>();
    private long writeOffset = 0L;
    private long readOffset = 0L;
    private LineOrientedStream requestStream;
    private ReaderWriterLock streamLock = new ReaderWriterLock();
  }
}
