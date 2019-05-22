using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace DNSServer
{
    class RequestHandler
    {
        private CashResolver resolver;
        private byte[] requestPack;
        private Connect connect;
        private UserInterface mainUI;

        public RequestHandler(Connect userConnect, UserInterface ui, CashResolver cashResolver)
        {
            resolver = cashResolver;
            connect = userConnect;//user socket
            mainUI = ui;
            requestPack = connect.Packet;
        }

        public void Resolve()
        {
            var request = DNSParser.ParseRequest(requestPack);//DONE
            var answer = resolver.ResolveNames(request);//DONE
            var answerPack = DNSGenerator.GenerateResponse(answer, request);//DONE
            mainUI.SendAnswer(connect, answerPack);
        }
    }
}
