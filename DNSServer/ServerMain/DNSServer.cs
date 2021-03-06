﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace DNSServer
{
    class DNSServer
    {
        private UserInterface ui;
        private CashResolver resolver;

        public DNSServer(IPAddress serverInterface)
        {
            ui = new UserInterface(serverInterface);
            resolver = new CashResolver();
        }

        public void Listen()
        {
            while (true)
            {
                //при получении пакета создание нового потока и активация в нем RequestHandler'a
                var connect = ui.GetNewConnect();

                var handler = new RequestHandler(connect, ui, resolver);
                var handling = new Thread(handler.Resolve);
                handling.Start();
            }
        }
    }
}
