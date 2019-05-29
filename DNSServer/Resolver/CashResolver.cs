using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DNSServer
{
    class CashResolver
    {
        private Dictionary<Question, Record[]> cash;
        private Dictionary<int, Queue<byte[]>> backupCash;
        private DelegateResolver master;

        public CashResolver()
        {
            cash = new Dictionary<Question, Record[]>();
            backupCash = new Dictionary<int, Queue<byte[]>>();
            master = new DelegateResolver(cash, backupCash);
            new Thread(master.ListenAndUpdate).Start();
        }
        
        public Record[] ResolveNames(Request req)
        {
            var records = new List<Record>();

            foreach (var quest in req.Questions)
            {
                try
                {
                    records.AddRange(ResolveQuestion(quest));
                } catch (RequestTimeOutException ex)
                {
                    throw new TimeoutException();
                }
            }

            return records.ToArray();
        }

        private Record[] ResolveQuestion(Question quest)
        {
            var records = new Record[0];

            try
            {
                master.SendRequest(quest);
            } catch (SocketException ex)
            {
                var recordsOutdated = false;

                if(cash.TryGetValue(quest, out records))
                    foreach (var record in records)
                    {
                        if (record.IsOutdated())
                            recordsOutdated = true;
                    }
                else
                {
                    throw new RequestTimeOutException(quest.Domain + " isn't connection to network!");
                }

                if (!recordsOutdated)
                    return records;
            }

            for (var i = 0; i < 2; i++)
            {
                lock (cash)
                {
                    var recordsOutdated = false;
                    if (cash.TryGetValue(quest, out records))
                    {

                        foreach (var record in records)
                        {
                            if (record.IsOutdated())
                                recordsOutdated = true;
                        }

                        if (!recordsOutdated)
                            return records;
                    }
                }
                Thread.Sleep(50);
            }


            
            throw new RequestTimeOutException(quest.Domain + " isn't resolvable now.");
        }
    }
}
