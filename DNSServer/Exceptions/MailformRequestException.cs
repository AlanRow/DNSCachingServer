﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSServer
{
    class MailformRequestException : Exception
    {
        public MailformRequestException() : base() { }

        public MailformRequestException(string message) : base(message) { }
    }
}
