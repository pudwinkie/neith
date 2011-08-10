using System;
using System.Text;
using System.IO;

namespace Smdn.Net.Imap4.Protocol.Client {
  internal class ImapPseudoResponseReceiver {
    public ImapPseudoResponseReceiver()
    {
      baseStream = new MemoryStream();
      stream = new LineOrientedBufferedStream(baseStream);
      receiver = new ImapResponseReceiver(stream);
    }

    public void SetResponse(string response)
    {
      var resp = Encoding.ASCII.GetBytes(response);

      baseStream.Seek(0, SeekOrigin.Begin);
      baseStream.Write(resp, 0, resp.Length);
      baseStream.Seek(0, SeekOrigin.Begin);
    }

    public ImapResponse ReceiveResponse()
    {
      return receiver.ReceiveResponse();
    }

    private ImapResponseReceiver receiver;
    private MemoryStream baseStream;
    private LineOrientedBufferedStream stream;
  }
}

