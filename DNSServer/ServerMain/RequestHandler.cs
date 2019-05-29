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
        private SimpleDelegateResolver backupResolver;
        private byte[] requestPack;
        private Connect connect;
        private UserInterface mainUI;

        public RequestHandler(Connect userConnect, UserInterface ui, CashResolver cashResolver)
        {
            resolver = cashResolver;
            connect = userConnect;//user socket
            mainUI = ui;
            requestPack = connect.Packet;
            backupResolver = new SimpleDelegateResolver();
        }

        public void Resolve()
        {
            byte[] answerPack;
            try
            {
                var request = DNSParser.ParseRequest(requestPack);//DONE
                var answer = resolver.ResolveNames(request);//DONE
                answerPack = DNSGenerator.GenerateResponse(answer, request);//DONE
            }
            catch
            {
                answerPack = backupResolver.ResolveQuestion(requestPack);
            }
            mainUI.SendAnswer(connect, answerPack);
        }
    }
}
