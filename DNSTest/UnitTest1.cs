using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DNSServer;

namespace DNSTest
{
    [TestClass]
    public class DNSParserTests
    {
        [TestMethod]
        public void RecordTypeParseTest()
        {
            var type = RecordType.None;
            DNSParser.TryGenerateRecordType(ref type, 28);

            Assert.AreEqual(RecordType.AAA, type);
            Assert.IsFalse(DNSParser.TryGenerateRecordType(ref type, 56));

        }

        [TestMethod]
        public void RequestParseTest()
        {
            var pack = "00040100000100000000000006676f6f676c6503636f6d0000010001";
            var actual = DNSParser.ParseRequest(ParsePacket(pack));
            var expected = new Request(new Question[] { new Question("google.com.", RecordType.A) }, 4);

            Assert.AreEqual(actual, expected);
            Assert.AreEqual(actual.ID, 4);
        }



        private static byte[] ParsePacket(string pack)
        {

            if (pack.Length % 2 != 0)
                throw new FormatException();

            var bytes = new byte[pack.Length / 2];

            for (var i = 0; i < pack.Length; i += 2)
            {
                bytes[i / 2] = (byte)(HexParse(pack[i]) * 16);
            }

            for (var i = 1; i < pack.Length; i += 2)
            {
                bytes[i / 2] += HexParse(pack[i]);
            }

            return bytes;
        }

        private static byte HexParse(char hex)
        {
            if (Char.IsDigit(hex))
                return byte.Parse("" + hex);

            switch (Char.ToLower(hex))
            {
                case 'a':
                    return 10;

                case 'b':
                    return 11;

                case 'c':
                    return 12;

                case 'd':
                    return 13;

                case 'e':
                    return 14;

                case 'f':
                    return 15;

                default:
                    throw new FormatException("The symbol <" + hex + "> in't a hex-digit.");
            }
        }
    }
}
