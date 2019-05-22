using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSServer
{
    public class Question
    {
        public RecordType Type { get; private set; }
        public string Domain { get; private set; }

        public Question(string domainName, RecordType recordType)
        {
            Type = recordType;
            Domain = domainName;
        }

        public override int GetHashCode()
        {
            return ((int)Type) ^ Domain.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Question))
                return false;

            var other = (Question)obj;

            return Type == other.Type && Domain == other.Domain;

        }
    }
}
