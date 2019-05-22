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
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Введите адрес интерфейса на который сервер будет принимать соединения: ");
            IPAddress serverInt = IPAddress.Parse(Console.ReadLine());

            //init
            var server = new DNSServer(serverInt);

            var thread = new Thread(server.Listen);
            thread.Start();
        }
    }
}
