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
using System.Net;
using System.Net.Sockets;

using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Server;

namespace Smdn.Net.Imap4.Server.Session {
  public class ImapServer : IImapServer, IDisposable {
    public virtual string Name {
      get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name; }
    }

    protected virtual ImapUpgradeConnectionStreamCallback CreateSllStreamCallback {
      get { return null; }
    }

    public ImapServer()
      : this(new IPEndPoint(IPAddress.Any, Protocol.ImapDefaultPort.Imap))
    {
    }

    public ImapServer(IPAddress localAddress)
      : this(new IPEndPoint(localAddress, Protocol.ImapDefaultPort.Imap))
    {
    }

    public ImapServer(IPAddress localAddress, int port)
      : this(new IPEndPoint(localAddress, port))
    {
    }

    public ImapServer(IPEndPoint localEndPoint)
    {
      listener = new TcpListener(localEndPoint);
    }

    ~ImapServer()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing) {
        if (listener != null) {
          listener.Stop();
          listener = null;
        }
      }

      disposed = true;
    }

    public void Start()
    {
      CheckDisposed();

      listener.Start();

      Trace.Info("{0} started ({1})", GetType().Name, listener.LocalEndpoint);
    }

    public ImapSession AcceptSession()
    {
      return AcceptSession<ImapSession>();
    }

    public TImapSession AcceptSession<TImapSession>() where TImapSession : ImapSession, new()
    {
      for (;;) {
        var connection = AcceptConnection(listener.AcceptTcpClient());

        if (connection == null)
          continue;

        var session = new TImapSession();

        session.StartSession(this, connection);

        return session;
      }
    }

    protected virtual ImapConnection AcceptConnection(TcpClient client)
    {
      return new ImapConnection(client, CreateSllStreamCallback);
    }

    /*
     * server
     */
    ImapMailbox IImapServer.OpenMailbox(string name, bool asReadOnly)
    {
      return OpenMailbox(name, asReadOnly);
    }

    protected virtual ImapMailbox OpenMailbox(string name, bool asReadOnly)
    {
      if (string.Equals(name, "INBOX", StringComparison.InvariantCultureIgnoreCase))
        return new ImapMailbox();
      else
        return null;
    }

    void IImapServer.CloseMailbox(ImapMailbox mailbox)
    {
      CloseMailbox(mailbox);
    }

    protected virtual void CloseMailbox(ImapMailbox mailbox)
    {
    }

    /*
     * methods for checking and validating session status
     */
    private void CheckDisposed()
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private TcpListener listener;
    private bool disposed = false;
  }
}
