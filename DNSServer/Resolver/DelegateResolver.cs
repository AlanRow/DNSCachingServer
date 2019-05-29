using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace DNSServer
{
    class DelegateResolver
    {
        private static string master = "8.8.8.8";
        private UdpClient client;
        private UIDGenerator uidg;
        Dictionary<Question, Record[]> cash;
        Dictionary<int, Queue<byte[]>> backup;

        public DelegateResolver(Dictionary<Question, Record[]> extCash, Dictionary<int, Queue<byte[]>> backupCash)
        {
            client = new UdpClient(master, 53);
            uidg = new UIDGenerator();
            cash = extCash;
            backup = backupCash;
        }

        public void ListenAndUpdate()
        {
            while (true)
            {
                var remote = new IPEndPoint(IPAddress.Parse(master), 53);
                var pack = client.Receive(ref remote);

                try
                {
                    var record = DNSParser.ParseResponse(pack);

                    lock (cash)
                    {
                        cash[record.Item1] = record.Item2;
                    }
                }
                catch (MailformRequestException ex)
                {
                    var id = pack[0] * 256 + pack[1];
                    if (!backup.ContainsKey(id))
                    {
                        backup[id] = new Queue<byte[]>();
                    }
                    backup[id].Enqueue(pack);
                }
            }
        }

        public void SendRequest(Question quest)
        {
            var id = uidg.Generate();
            var pack = DNSGenerator.GenerateRequest(quest, id, new SuffixTree());

            client.Send(pack, pack.Length);
        }
    }
}
