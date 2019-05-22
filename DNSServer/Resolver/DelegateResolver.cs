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

        public DelegateResolver(Dictionary<Question, Record[]> extCash)
        {
            client = new UdpClient(master, 53);
            uidg = new UIDGenerator();
            cash = extCash;
        }

        public void ListenAndUpdate()
        {
            while (true)
            {
                var remote = new IPEndPoint(IPAddress.Parse(master), 53);
                var pack = client.Receive(ref remote);
                
                var record = DNSParser.ParseResponse(pack);
                
                lock (cash)
                {
                    cash[record.Item1] = record.Item2;
                }
            }
        }

        public void SendRequest(Question quest)
        {
            var id = uidg.Generate();
            var pack = DNSGenerator.GenerateRequest(quest, id, new SuffixTree());

            client.Send(pack, pack.Length);
        }

        //public Record[] ResolveName(Question question)
        //{
        //    var id = uidg.Generate();
        //    var pack = DNSParser.GenerateRequest(question, id);
        //    client.Send(pack, pack.Length);
            
        //}
    }
}
