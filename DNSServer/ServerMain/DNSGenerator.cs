using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSServer
{
    public class DNSGenerator
    {

        public static byte[] GenerateResponse(Record[] records, Request req)
        {
            var response = new List<byte>();
            var names = new SuffixTree();

            response.AddRange(GenerateRequest(req.Questions[0], (short)req.ID, names));
            response[6] = (byte)(records.Length / 256);
            response[7] = (byte)(records.Length % 256);

            foreach (var record in records)
            {
                response.AddRange(GenerateAnswer(record, names, response.Count));
            }

            return response.ToArray();
        }



        private static byte[] GenerateAnswer(Record rec, SuffixTree names, int startAddr)
        {
            var answer = new List<byte>();

            answer.AddRange(GenerateName(rec.Name, answer.Count, names));
            answer.AddRange(To2Bytes((int)rec.Type));
            answer.AddRange(To2Bytes(1));

            var ttl = rec.ActualTo.Subtract(DateTime.Now).Seconds;
            answer.AddRange(To2Bytes(ttl / 65536));
            answer.AddRange(To2Bytes(ttl % 65536));

            switch (rec.Type)
            {
                case RecordType.A:
                    var address4 = (byte[])rec.Data;
                    answer.AddRange(To2Bytes(4));
                    answer.AddRange(address4);
                    break;
                case RecordType.AAA:
                    var address16 = (byte[])rec.Data;
                    answer.AddRange(To2Bytes(16));
                    answer.AddRange(address16);
                    break;
                case RecordType.NS:
                    var nsname = (string)rec.Data;
                    var nsData = GenerateName(nsname, answer.Count, names);
                    answer.AddRange(To2Bytes(nsData.Length));
                    answer.AddRange(nsData);
                    break;
                case RecordType.SOA:
                    var soaData = (SOAData)rec.Data;
                    var dat = new List<byte>();
                    
                    dat.AddRange(GenerateName(soaData.Server, answer.Count + dat.Count, names));
                    dat.AddRange(GenerateName(soaData.Mail, answer.Count + dat.Count, names));
                    dat.AddRange(To2Bytes((int)(soaData.Serial / 65536)));
                    dat.AddRange(To2Bytes((int)(soaData.Serial % 65536)));
                    dat.AddRange(To2Bytes(soaData.Refresh));
                    dat.AddRange(To2Bytes(soaData.Retry));
                    dat.AddRange(To2Bytes(soaData.Expire));
                    dat.AddRange(To2Bytes(soaData.MinTTL));

                    answer.AddRange(To2Bytes(dat.Count));
                    answer.AddRange(dat);
                    break;

            }

            return answer.ToArray();
        }

        public static byte[] GenerateRequest(Question quest, short id, SuffixTree names)
        {
            var request = new List<byte>();

            request.AddRange(To2Bytes(id));
            request.Add(1);//0... - request, .0000... - opcode (std. query), ...0.. - isn't truncated, ...0. - in't fragmented, ...1 - recursion
            request.Add(0);//0... - server supports recursion, .000... - reserved, ...0000 - RCODE (NOError)
            request.AddRange(To2Bytes(1));//questions count
            request.AddRange(To2Bytes(0));//answers count
            request.AddRange(To2Bytes(0));//authoritive RRs count
            request.AddRange(To2Bytes(0));//additional RRs count

            //simple single question
            int trash = 0;
            names.ReadNameAndUpdate(quest.Domain, ref trash, request.Count);
            request.AddRange(EncodeDName(quest.Domain));//name

            request.AddRange(To2Bytes((int)quest.Type));//Type
            request.AddRange(To2Bytes(1));//Class (IN)

            return request.ToArray();
        }

        //преобразование имени
        //доменное имя, есть набор подимен
        //парсер должен сделать следующее
        //1. прочитать имя и разбить на зоны
        //2. найти максимальное подимя в дереве суффиксов
        //3. заменить его на ссылку
        //4. остальное написать

        public static byte[] GenerateName(string name, int addr, SuffixTree names)
        {
            var bytes = new List<byte>();
            var codes = GenerateZones(name);

            int reference = 0;
            int suffix = suffix = names.ReadNameAndUpdate(name, ref reference, addr);

            foreach (var bs in codes)
            {
                bytes.Add((byte)bs.Length);
                bytes.AddRange(bs);
            }

            if (suffix > 0)
            {
                bytes.RemoveRange(bytes.Count - suffix, suffix);
                bytes.AddRange(To2Bytes(192 * 256 + reference));
            }

            return bytes.ToArray();
        }

        public static byte[][] GenerateZones(string name)
        {
            return name.Split('.').Where(x => x != "")
                                  .Select(x => Encoding.UTF8.GetBytes(x))
                                  .ToArray();
        }

        public static bool TryGenerateRecordType(ref RecordType type, int number)
        {
            switch (number)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 5:
                    break;
                case 6:
                    break;
                case 12:
                    break;
                case 15:
                    break;
                case 28:
                    break;
                default:
                    return false;
            }

            type = (RecordType)number;
            return true;
        }
        
        private static List<byte> EncodeDName(string name)
        {
            var zones = name.Split('.');
            var encName = new List<byte>();

            foreach (var z in zones)
            {
                if (z == "")
                    continue;

                var encZone = Encoding.UTF8.GetBytes(z);
                encName.Add((byte)encZone.Length);
                encName.AddRange(encZone);
            }

            encName.Add(0);

            return encName;
        }

        private static byte[] To2Bytes(int num)
        {
            return new byte[] { (byte)(num / 256), (byte)(num % 256) };
        }
    }
}
