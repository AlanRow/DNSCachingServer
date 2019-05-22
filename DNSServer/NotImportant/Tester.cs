using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Util;

namespace DNSServer
{
    static class Tester
    {
        //static void DoTest()
        //{
        //    var pack = "1c7ee591c94050465d8a17430800450000384459000080110000c0a80014c0a80001e2e100350024819b00040100000100000000000006676f6f676c6503636f6d0000010001"
        //    Console.WriteLine(TestParseRequest(pack, new Request(new Question[] {new Question("google.com.", RecordType.A) }, 4)));
        //}

        //static string TestParseRequest(string pack, Request expected)
        //{
        //    var bytePack = ParsePacket(pack);

        //    var actual = DNSParser.ParseRequest(bytePack);

        //    if (actual.ID == expected.ID && Questions.Equals(expected.Questions, actual.Questions))
        //        return "Parse-request-test has passed";
        //    else
        //        return "Parse-request-test failed.";

        //}

        //private static byte[] ParsePacket(string pack)
        //{

        //    if (pack.Length % 2 != 0)
        //        throw new FormatException();

        //    var bytes = new byte[pack.Length / 2];

        //    for (var i = 0; i < pack.Length; i += 2)
        //    {
        //        bytes[i / 2] = HexParse(pack[i]) * 16;
        //    }

        //    for (var i = 1; i < pack.Length; i += 2)
        //    {
        //        bytes[i / 2] += HexParse(pack[i]);
        //    }

        //    return bytes;
        //}
    }

}
