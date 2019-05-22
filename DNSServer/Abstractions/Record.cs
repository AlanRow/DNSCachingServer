using System;

namespace DNSServer
{
    public enum RecordType
    {
        None = 0,
        A = 1,
        NS = 2,
        CNAME = 5,
        SOA = 6,
        PTR = 12,
        MX = 15,
        AAA = 28
    }

    enum EntryClass
    {
        IN
    }

    public class Record
    {
        public readonly string Name;
        public readonly RecordType Type;
        public readonly DateTime ActualTo;
        public readonly object Data;

        public Record(string domName, RecordType recordType, uint TTL, object recordData)
        {
            Name = domName;
            Type = recordType;
            ActualTo = DateTime.Now.AddSeconds(TTL);

            var validData = true;

            switch (Type)
            {
                case RecordType.A:
                    validData = recordData is byte[];
                    break;
                case RecordType.AAA:
                    validData = recordData is byte[];
                    break;
                case RecordType.CNAME:
                    validData = recordData is string;
                    break;
                case RecordType.MX:
                    validData = recordData is MXData;
                    break;
                case RecordType.NS:
                    validData = recordData is string;
                    break;
                case RecordType.PTR:
                    validData = recordData is string;
                    break;
                case RecordType.SOA:
                    validData = recordData is SOAData;
                    break;
                default:
                    throw new MailformRequestException("Not known type <" + Type + ">");
            }

            if (!validData)
                throw new MailformRequestException("The type of request (" + Type + ") doesn't correspond to data.");

            Data = recordData;
        }

        public bool IsOutdated()
        {
            return ActualTo.CompareTo(DateTime.Now) <= 0;
        }
    }
}
