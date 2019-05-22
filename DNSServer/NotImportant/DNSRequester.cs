using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace DNSServer
{
    class DNSRequester
    {
        private static string serverAddr = "8.8.8.8";
        //private int clientPort = 3030;
        private static int serverPort = 53;
        private UdpClient client;
        private UIDGenerator uidg;

        public DNSRequester(UIDGenerator generator, UdpClient udp)
        {
            uidg = generator;
            client = udp;
        }

        public int Request(string domainName)
        {
            var id = uidg.Generate();
            var pack = GenerateRequest(domainName, id);

            lock (client)
            {
                client.Send(pack, pack.Length);
            }

            return id;
        }

        //simple request
        private byte[] GenerateRequest(string name, int id)
        {
            var request = new List<byte>();

            request.AddRange(To2Bytes(id));
            request.Add(1);//0... - request, .0000... - opcode (std. query), ...0.. - isn't truncated, ...0. - in't fragmented, ...1 - recursion
            request.Add(0);//0... - server supports recursion, .000... - reserved, ...0000 - RCODE (NOError)
            request.AddRange(To2Bytes(1));//questions count (for now - one)
            request.AddRange(To2Bytes(0));//answers count
            request.AddRange(To2Bytes(0));//authoritive RRs count
            request.AddRange(To2Bytes(0));//additional RRs count

            //simple single question
            request.AddRange(EncodeDName(name));//name
            request.AddRange(To2Bytes(1));//Type (A)
            request.AddRange(To2Bytes(1));//Class (IN)

            return request.ToArray();
        }

        private List<byte> EncodeDName(string name)
        {
            var zones = name.Split('.');
            var encName = new List<byte>();

            foreach (var z in zones)
            {
                var encZone = Encoding.UTF8.GetBytes(z);
                encName.Add((byte)encZone.Length);
                encName.AddRange(encZone);
            }

            encName.Add(0);

            return encName;
        }

        private byte[] To2Bytes(int num)
        {
            return new byte[] { (byte)(num / 256), (byte)(num % 256) };
        }

        //private byte[] ToBytes(int number, int count)
        //{
        //    var pow = (int) Math.Round(Math.Pow(2, count - 1));

        //    for (var p = pow; p > 0; p/= 2)
        //    {
        //        if (number )
        //    }
        //}
    }
}
