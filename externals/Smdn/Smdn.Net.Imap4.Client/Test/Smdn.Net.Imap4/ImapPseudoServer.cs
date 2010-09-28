using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using Smdn.IO;

namespace Smdn.Net.Imap4 {
  public class ImapPseudoServer : IDisposable {
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

    public bool IsStarted {
      get { return thread != null; }
    }

    public bool WaitForRequest {
      get; set;
    }

    public ImapPseudoServer()
    {
      WaitForRequest = true;

      server = new TcpListener(new IPEndPoint(IPAddress.Loopback, 0));
      server.Start();

      requestStream = new StrictLineOrientedStream(new MemoryStream());
    }

    public void Start()
    {
      responseTag = 0;

      thread = new Thread(ServerThread);
      thread.Start();
    }

    public void Dispose()
    {
      Stop();
    }

    public void Stop()
    {
      Stop(false);
    }

    public void Stop(bool sendEnqueued)
    {
      if (thread != null) {
        if (sendEnqueued)
          this.sendEnqueued = true;

        thread.Abort();
        thread = null;
      }
    }

    public void EnqueueResponse(string response)
    {
      responseQueue.Enqueue(response);
    }

    public void EnqueueTaggedResponse(string response)
    {
      responseQueue.Enqueue(response.Replace("$tag", (responseTag++).ToString("x4")));
    }

    public string DequeueAll()
    {
      return DequeueAll(Encoding.UTF8);
    }

    public string DequeueAll(Encoding encoding)
    {
      Thread.Sleep(100);

      for (var retry = 0; retry < 20; retry++) {
        if (readOffset == writeOffset)
          Thread.Sleep(100);
      }

      streamLock.AcquireReaderLock(1000);

      var text = new System.Text.StringBuilder();

      try {
        for (;;) {
          requestStream.Position = readOffset;

          var line = requestStream.ReadLine(true);

          if (line == null)
            return text.ToString();
          else
            text.Append(encoding.GetString(line));

          readOffset += line.Length;
        }
      }
      finally {
        streamLock.ReleaseReaderLock();
      }
    }

    public string DequeueRequest()
    {
      return DequeueRequest(Encoding.UTF8);
    }

    public string DequeueRequest(Encoding encoding)
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

      return encoding.GetString(line);
    }

    private void ServerThread()
    {
      var client = server.AcceptTcpClient();

      Console.WriteLine("accepted");

      NetworkStream stream = null;

      client.ReceiveTimeout = 250;

      try {
        stream = client.GetStream();

        for (;;) {
          if (responseQueue.Count == 0) {
            Thread.Sleep(25);
            continue;
          }

          // send response
          var responseText = responseQueue.Dequeue();
          var response = Encoding.UTF8.GetBytes(responseText);

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

              Console.WriteLine("C: {0}", Encoding.UTF8.GetString(buffer, 0, read));

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
                // FIXME:
                if (Runtime.IsRunningOnWindows) {
                  if (WaitForRequest && responseQueue.Count <= 0) {
                    continue;
                  }
                  else {
                    break;
                  }
                }
                else {
                  if (responseQueue.Count <= 0) {
                    continue;
                  }
                  else {
                    if (WaitForRequest)
                      continue;
                    else
                      break;
                  }
                }
              }
              else {
                if (!(ex.InnerException is ThreadAbortException)) {
                  Console.WriteLine("failed at receive, client disconnected?");
                  Console.WriteLine(ex);
                }
                return;
              }
            }
          }
        }
      }
      catch (ThreadAbortException) {
        if (sendEnqueued) {
          while (0 < responseQueue.Count) {
            var responseText = responseQueue.Dequeue();
            var response = Encoding.UTF8.GetBytes(responseText);

            stream.Write(response, 0, response.Length);

            Console.WriteLine("S: {0}", responseText);
          }

          client.Client.Shutdown(SocketShutdown.Both);
        }

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
    private int responseTag = 0;
    private bool sendEnqueued = false;
  }
}