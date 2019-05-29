using System.Net.Sockets;
using System.Net;

namespace DNSServer
{
    public class SimpleDelegateResolver
    {
        private static string master = "8.8.8.8";
        private UdpClient client;

        public SimpleDelegateResolver()
        {
            client = new UdpClient(master, 53);
        }

        public byte[] ResolveQuestion(byte[] quest)
        {
            client.Send(quest, quest.Length);
            //var plchld = new IPEndPoint(IPAddress.Parse("127.0.0.2"), 42356);
            IPEndPoint emptyPoint = null;
            return client.Receive(ref emptyPoint);
        }
    }
}