using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSServer
{
    public class MXData
    {
        public readonly int Prefer;
        public readonly string Name;

        public MXData(int pref, string name)
        {
            Prefer = pref;
            Name = name;
        }
    }
}
