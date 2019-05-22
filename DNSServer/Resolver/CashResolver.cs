using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DNSServer
{
    class CashResolver
    {
        private Dictionary<Question, Record[]> cash;
        private DelegateResolver master;

        public CashResolver()
        {
            cash = new Dictionary<Question, Record[]>();
            master = new DelegateResolver(cash);
            new Thread(master.ListenAndUpdate).Start();
        }
        
        public Record[] ResolveNames(Request req)
        {
            var records = new List<Record>();

            foreach (var quest in req.Questions)
            {
                records.AddRange(ResolveQuestion(quest));
            }

            return records.ToArray();
        }

        private Record[] ResolveQuestion(Question quest)
        {
            var records = new Record[0];

            master.SendRequest(quest);

            for (var i = 0; i < 10; i++)
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
                Thread.Sleep(500);
            }
            
            throw new RequestTimeOutException(quest.Domain + " isn't resolvable now.");
        }
    }
}
