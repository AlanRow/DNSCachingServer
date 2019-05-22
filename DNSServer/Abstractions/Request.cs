using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSServer
{
    public class Request
    {
        public readonly int ID;
        public Question[] Questions { get; private set; }

        public Request(Question[] quests, int id)
        {
            Questions = quests;
            ID = id;
        }

        public override int GetHashCode()
        {
            if (Questions == null)
                return -1;
            if (Questions.Length == 0)
                return 0;

            var code = Questions[0].GetHashCode();

            foreach (var q in Questions.Skip(1))
                code = code ^ q.GetHashCode();

            return code;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Request))
                return false;

            var other = (Request)obj;

            if (Questions == null || other.Questions == null || other.Questions.Length != Questions.Length)
                return false;

            for (var i = 0; i < Questions.Length; i++)
                if (Questions[i] != null && !(Questions[i].Equals(other.Questions[i])))
                    return false;

            return true;
        }
    }
}
