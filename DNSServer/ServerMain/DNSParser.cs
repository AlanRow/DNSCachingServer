using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSServer
{
    public static class DNSParser
    {
        public static Request ParseRequest(byte[] pack)
        {
            CheckPackIsValid(pack);

            var quests = new Question[pack[5]];
            
            int id = pack[0] * 256 + pack[1];

            for (var i = 6; i < 12; i++)
                if (pack[i] != 0)
                    throw new MailformRequestException("On the " + i + " byte. This should was be 0, but was " + pack[i]);

            var ind = 12;
            for (var i = 0; i < quests.Length; i++)
            {
                try
                {
                    ind = ParseQuestion(quests, i, pack, ind);
                } catch (MailformRequestException ex)
                {
                    throw new MailformRequestException("In " + (i + 1) + " question: " + ex.Message);
                }
            }

            return new Request(quests, id);
        }

        public static Tuple<Question, Record[]> ParseResponse(byte[] packet)
        {
            CheckPackIsValid(packet);
            var quests = new Question[1];
            int start = 12;

            start = ParseQuestion(quests, 0, packet, start);

            var quest = quests[0];
            var ansCount = packet[7] + packet[9] + packet[11];
            var answers = new Record[ansCount];

            for (var i = 0; i < ansCount; i++)
            {
                start = ParseAnswer(answers, i, packet, start);
            }

            return Tuple.Create(quest, answers);
        }

        private static void CheckPackIsValid(byte[] packet)
        {
            if (packet.Length < 12)
                throw new MailformRequestException("DNS-Packet too short (< 12 bytes)");
            

        }
        
        public static byte[] GenerateResponse(Record[] records, Request req)
        {
            var response = new List<byte>();

            response.AddRange(GenerateRequest(req.Questions[0], (short)req.ID));
            response[6] = (byte)(records.Length / 256);
            response[7] = (byte)(records.Length % 256);

            foreach (var record in records)
            {
                response.AddRange(GenerateAnswer(record));
            }

            return response.ToArray();
        }

        private static byte[] GenerateAnswer(Record rec)
        {
            var answer = new List<byte>();

            answer.AddRange(GenerateName(rec.Name));
            answer.AddRange(To2Bytes((int)rec.Type));
            answer.AddRange(To2Bytes(1));

            var ttl = rec.ActualTo.Subtract(DateTime.Now).Seconds;
            answer.AddRange(To2Bytes(ttl / 65536));
            answer.AddRange(To2Bytes(ttl % 65536));

            switch (rec.Type)
            {
                case RecordType.A:
                    var address4 = (byte[])rec.Data;
                    answer.AddRange(address4);
                    break;
                case RecordType.AAA:
                    var address16 = (byte[])rec.Data;
                    answer.AddRange(address16);
                    break;
                case RecordType.NS:
                    var nsname = (string)rec.Data;
                    answer.AddRange(GenerateName(nsname));
                    break;
                case RecordType.SOA:
                    var soaData = (SOAData)rec.Data;
                    answer.AddRange(GenerateName(soaData.Server));
                    answer.AddRange(GenerateName(soaData.Mail));
                    answer.AddRange(To2Bytes((int)(soaData.Serial / 65536)));
                    answer.AddRange(To2Bytes((int)(soaData.Serial % 65536)));
                    answer.AddRange(To2Bytes(soaData.Refresh));
                    answer.AddRange(To2Bytes(soaData.Retry));
                    answer.AddRange(To2Bytes(soaData.Expire));
                    answer.AddRange(To2Bytes(soaData.MinTTL));
                    break;

            }

            return answer.ToArray();
        }

        public static byte[] GenerateRequest(Question quest, short id)
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
            request.AddRange(EncodeDName(quest.Domain));//name
            request.AddRange(To2Bytes((int)quest.Type));//Type
            request.AddRange(To2Bytes(1));//Class (IN)

            return request.ToArray();
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

        private static int ParseQuestion(Question[] questions, int current, byte[] packet, int startIndex)
        {
            int i = startIndex;
            var domain = "";

            i = ParseName(ref domain, packet, i);

            int typeNum = packet[i] * 256 + packet[i + 2];
            RecordType type = RecordType.None;
            
            if (!DNSParser.TryGenerateRecordType(ref type, typeNum))
                throw new MailformRequestException("Type with number " + typeNum + " has not found!");

            var quest = new Question(domain, type);
            questions[current] = quest;

            return i + 5;
        }

        private static int ParseAnswer(Record[] records, int current, byte[] packet, int start)
        {
            var i = start;

            var name = "";
            i = ParseName(ref name, packet, i);

            var type = RecordType.None;
            if (!TryGenerateRecordType(ref type, packet[i++] * 256 + packet[i++]))
                throw new MailformRequestException();

            i += 2;
            uint ttl = (uint)packet[i++] * (uint)16777216 + (uint)packet[i++] * (uint)65536 + (uint)packet[i++] * (uint)256 + (uint)packet[i++];
            var dLength = packet[i++] * 256 + packet[i++];

            object data = null;
            i = ParseRecordData(ref data, packet, type, i, dLength);

            records[current] = new Record(name, type, ttl, data);

            return i;
        }

        private static byte[] GenerateName(string name)
        {
            var zones = name.Split('.');
            var bytes = new List<byte>();
            
            foreach (var z in zones)
            {
                if (z == "")
                    continue;

                var code = Encoding.UTF8.GetBytes(z);
                bytes.Add((byte)code.Length);
                bytes.AddRange(code);
            }

            bytes.Add(0);

            return bytes.ToArray();
        }

        private static int ParseName(ref string name, byte[] pack, int start)
        {
            var i = start;

            while (i < pack.Length && pack[i] != 0)
            {
                if (pack[i] >= 192)
                {
                    ParseName(ref name, pack, pack[i + 1]);
                    i += 2;
                    break;
                }
                i = ReadWord(ref name, pack, i);
            }

            return i;
        }


        //DO OTHER
        private static int ParseRecordData(ref object data, byte[] pack, RecordType type, int start, int length)
        {
            var i = start;

            switch (type)
            {
                case RecordType.A:
                    data = new byte[] { pack[i++], pack[i++], pack[i++], pack[i++] };
                    break;
                case RecordType.AAA:
                    data = new byte[] { pack[i++], pack[i++], pack[i++], pack[i++],
                                        pack[i++], pack[i++], pack[i++], pack[i++],
                                        pack[i++], pack[i++], pack[i++], pack[i++],
                                        pack[i++], pack[i++], pack[i++], pack[i++] };
                    break;
                case RecordType.NS:
                    var name = "";
                    i = ParseName(ref name, pack, i);
                    data = name;
                    break;
                case RecordType.SOA:
                    var serverName = "";
                    i = ParseName(ref serverName, pack, i);
                    var mail = "";
                    i = ParseName(ref mail, pack, i);
                    ulong serial = (uint)pack[i++] * (uint)16777216 + (uint)pack[i++] * (uint)65536 + (uint)pack[i++] * (uint)256 + (uint)pack[i++];
                    var refresh = pack[i++] * 256 + pack[i++];
                    var retry = pack[i++] * 256 + pack[i++];
                    var expire = pack[i++] * 256 + pack[i++];
                    var ttl = pack[i++] * 256 + pack[i++];

                    data = new SOAData(serverName, mail, serial, refresh, retry, expire, ttl);
                    break;
            }

            return i;
        }

        private static int ReadWord(ref string name, byte[] pack, int start)
        {
            var i = start;
            var length = pack[i];
            var nameCode = new byte[length];
            Array.Copy(pack, i + 1, nameCode, 0, length);
            var zone = Encoding.UTF8.GetString(nameCode);
            name += zone + ".";

            return i + length + 1;
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
    }
}
