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

        private static int ParseQuestion(Question[] questions, int current, byte[] packet, int startIndex)
        {
            int i = startIndex;
            var domain = "";

            i = ParseName(ref domain, packet, i);

            int typeNum = packet[i] * 256 + packet[i + 2];
            RecordType type = RecordType.None;
            
            if (!DNSGenerator.TryGenerateRecordType(ref type, typeNum))
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
            if (!DNSGenerator.TryGenerateRecordType(ref type, packet[i++] * 256 + packet[i++]))
                throw new MailformRequestException();

            i += 2;
            uint ttl = (uint)packet[i++] * (uint)16777216 + (uint)packet[i++] * (uint)65536 + (uint)packet[i++] * (uint)256 + (uint)packet[i++];
            var dLength = packet[i++] * 256 + packet[i++];

            object data = null;
            i = ParseRecordData(ref data, packet, type, i, dLength);

            records[current] = new Record(name, type, ttl, data);

            return i;
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
    }
}
