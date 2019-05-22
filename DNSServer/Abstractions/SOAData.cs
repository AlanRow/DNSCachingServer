using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSServer
{
    public class SOAData
    {
        public readonly string Server;
        public readonly string Mail;
        public readonly ulong Serial;
        public readonly int Refresh;
        public readonly int Retry;
        public readonly int Expire;
        public readonly int MinTTL;

        public SOAData(string server, string mailbox, ulong serial, int refresh, int retry, int expire, int negativeTTL)
        {
            Server = server;
            Mail = mailbox;
            Serial = serial;
            Refresh = refresh;
            Retry = retry;
            Expire = expire;
            MinTTL = negativeTTL;
        }
    }
}