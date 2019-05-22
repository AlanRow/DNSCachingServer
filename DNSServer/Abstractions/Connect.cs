using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace DNSServer
{

    class Connect
    {
        public EndPoint User { get; private set; }
        public byte[] Packet { get; private set; }

        public Connect(byte[] pack, EndPoint userPoint)
        {
            User = userPoint;
            Packet = pack;
        }
    }
}
