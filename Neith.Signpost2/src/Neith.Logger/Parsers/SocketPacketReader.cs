using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Neith.Logger.Parsers
{
    public class SocketPacketReader : IPacketReader
    {
        public Socket Socket { get; private set; }


        public SocketPacketReader(Socket soc)
        {
            Socket = soc;
        }

        public async Task<int> ReceiveTaskAsync(IList<ArraySegment<byte>> buffers, int langth)
        {
            throw new NotImplementedException();
        }

    }
}
