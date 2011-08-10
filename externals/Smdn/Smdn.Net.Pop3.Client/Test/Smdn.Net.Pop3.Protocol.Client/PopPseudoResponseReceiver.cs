using System;
using System.IO;

namespace Smdn.Net.Pop3.Protocol.Client {
  internal class PopPseudoResponseReceiver {
    public bool HandleAsMultiline {
      get { return receiver.HandleAsMultiline; }
      set { receiver.HandleAsMultiline = value; }
    }

    public PopPseudoResponseReceiver()
    {
      baseStream = new MemoryStream();
      stream = new LineOrientedBufferedStream(baseStream);
      receiver = new PopResponseReceiver(stream);
    }

    public void SetResponse(string response)
    {
      var resp = NetworkTransferEncoding.Transfer8Bit.GetBytes(response);

      baseStream.Seek(0, SeekOrigin.Begin);
      baseStream.Write(resp, 0, resp.Length);
      baseStream.Seek(0, SeekOrigin.Begin);
    }

    public PopResponse ReceiveResponse()
    {
      return receiver.ReceiveResponse();
    }

    public bool ReceiveLine(Stream stream)
    {
      return receiver.ReceiveLine(stream);
    }

    private PopResponseReceiver receiver;
    private MemoryStream baseStream;
    private LineOrientedBufferedStream stream;
  }
}

