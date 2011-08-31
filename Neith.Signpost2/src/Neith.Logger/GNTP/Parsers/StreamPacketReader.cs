using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Neith.Logger.GNTP.Parsers
{
    public class StreamPacketReader : IPacketReader
    {
        public Stream Stream { get; private set; }

        public StreamPacketReader(Stream stream)
        {
            Stream = stream;
            Stream.ReadTimeout = Timeout.Infinite;
        }

        public async Task<ArraySegment<byte>> ReadAsync(ArraySegment<byte> buffer)
        {
            var count = await Stream.ReadAsync(buffer.Array, buffer.Offset, buffer.Count);
            if (count == 0) throw new IOException("Stream読込の結果が0バイトでした。");
            return new ArraySegment<byte>(buffer.Array, buffer.Offset, count);
        }

    }
}
