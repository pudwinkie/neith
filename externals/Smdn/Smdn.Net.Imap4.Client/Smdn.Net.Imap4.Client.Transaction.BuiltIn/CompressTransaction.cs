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
using System.IO;
using System.IO.Compression;

using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Transaction.BuiltIn {
  internal sealed class CompressTransaction : ImapTransactionBase {
    public CompressTransaction(ImapConnection connection)
      : base(connection)
    {
    }

    /*
     * http://tools.ietf.org/html/rfc4978
     * RFC 4978 - The IMAP COMPRESS Extension
     */
    // 3. The COMPRESS Command
    //    Arguments: Name of compression mechanism: "DEFLATE".
    //    Responses: None
    //    Result: OK The server will compress its responses and expects the
    //               client to compress its commands.
    //            NO Compression is already active via another layer.
    //           BAD Command unknown, invalid or unknown argument, or COMPRESS
    //               already active.
    protected override ImapCommand PrepareCommand()
    {
#if DEBUG
      if (!RequestArguments.ContainsKey("compression mechanism")) {
        FinishError(ImapCommandResultCode.RequestError, "arguments 'compression mechanism' must be setted");
        return null;
      }
#endif

      var compressionMechanism = RequestArguments["compression mechanism"] as ImapCompressionMechanism;

      foreach (var decompression in new[] {
        new {Mechanism = ImapCompressionMechanism.Deflate, CreateStreamCallback = new UpgradeConnectionStreamCallback(CreateDeflateStream)},
      }) {
        if (decompression.Mechanism != compressionMechanism)
          continue;

        createDecompressingStreamCallback = decompression.CreateStreamCallback;

        return Connection.CreateCommand("COMPRESS",
                                        RequestArguments["compression mechanism"]);
      }

      FinishError(ImapCommandResultCode.RequestError, "unsupported compression mechanism");

      return null;
    }

    protected override void OnTaggedStatusResponseReceived(ImapTaggedStatusResponse tagged)
    {
      if (tagged.Condition == ImapResponseCondition.Ok) {
        //    If the server issues an OK response, the server MUST compress
        //    starting immediately after the CRLF which ends the tagged OK
        //    response.  (Responses issued by the server before the OK response
        //    will, of course, still be uncompressed.)  If the server issues a BAD
        //    or NO response, the server MUST NOT turn on compression.
        Connection.UpgradeStream(createDecompressingStreamCallback);

        FinishOk(tagged);
      }
      else {
        base.OnTaggedStatusResponseReceived(tagged);
      }
    }

    private Stream CreateDeflateStream(ConnectionBase connection, Stream baseStream)
    {
      return new ImapDeflateStream(baseStream);
    }

    private UpgradeConnectionStreamCallback createDecompressingStreamCallback;
  }
}