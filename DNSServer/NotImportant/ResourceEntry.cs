using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSServer
{

    class ResourceEntry
    {
        public RecordType Type { get; private set; }
        public EntryClass Class {get; private set;}
        public string Name { get; private set; }

        public ResourceEntry(RecordType type, string name)
        {
            Type = type;
            Name = name;
            Class = EntryClass.IN;
        }

        public static ResourceEntry GetFromQuestion(byte[] quest)
        {
            var name = "";
            var index = 0;
            for (var i = 0; i < quest.Length; i++)
            {
                if (quest[i] == 0)
                    break;

                var length = quest[i];

                var zone = quest.Skip(i + 1).Take(length).ToArray();
                name += Encoding.UTF8.GetString(zone) + ".";
                i += length;
                index = i;
            }
            index += 3;

            var type = RecordType.A;

            switch (quest[index]) {

                case 1:
                    break;
                case 2:
                    type = RecordType.NS;
                    break;
                default:
                    throw new InvalidCastException("Type was " + quest[index]);
            }

            if (quest[index + 2] != 1)
                throw new InvalidCastException("Class was " + quest[index + 2]);

            return new ResourceEntry(type, name);
        }


    }
}
