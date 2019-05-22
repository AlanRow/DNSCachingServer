using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSServer
{
    class RequestTimeOutException : Exception
    {
        public RequestTimeOutException(string message) : base(message) { }
    }
}
