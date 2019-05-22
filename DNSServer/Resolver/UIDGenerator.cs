using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSServer
{
    class UIDGenerator
    {
        private short lastNumber;
        private Queue<short> released;

        public UIDGenerator()
        {
            lastNumber = 0;
            released = new Queue<short>();
        }

        public short Generate()
        {
            lock (this)
            {
                if (released.Count == 0)
                    return ++lastNumber;

                else
                    return released.Dequeue();
            }
        }

        public void Release(short id)
        {
            lock (this)
            {
                released.Enqueue(id);
            }
        }
    }
}
