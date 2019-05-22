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
    class UserInterface
    {
        private byte[] buffer;
        private Socket listener;

        public UserInterface(IPAddress serverAddress)
        {
            //прослушивание 53 порта на предмет подключений

            listener = new Socket(SocketType.Dgram, ProtocolType.Udp);
            var port = 53;
            listener.Bind(new IPEndPoint(serverAddress, port));
            buffer = new byte[65527];//max size of udp content 
        }

        public Connect GetNewConnect()
        {
            EndPoint userIP = new IPEndPoint(IPAddress.Parse("127.0.0.0"), 3479);

            var count = 0;
            lock (this)
            {
                count = listener.ReceiveFrom(buffer, ref userIP);
            }

            var pack = new byte[count];
            Array.Copy(buffer, pack, count);

            return new Connect(pack, userIP);
        }

        public void SendAnswer(Connect connect, byte[] packet)
        {
            listener.SendTo(packet, connect.User);
        }
    }
}
