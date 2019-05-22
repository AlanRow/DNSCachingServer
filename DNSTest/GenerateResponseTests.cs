using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DNSServer;

namespace DNSTest
{
    [TestClass]
    public class GenerateResponseTests
    {
        [TestMethod]
        public void PacketTests()
        {
            var request = new Request(new Question[] { new Question("solod.000webhostapp.com", RecordType.A) }, 14);
            var record1 = new Record("solod.000webhostapp.com", RecordType.A, 60, new byte[] { 145, 14, 145, 12});
            var resp = DNSGenerator.GenerateResponse(new Record[] { record1}, request);

            var ext = DNSParserTests.ParsePacket("00148180000100010000000005736f6c6f640d303030776562686f737461707003636f6d0000010001c00c000100010000003c0004910e910c");

            for (var i = 4; i < Math.Min(ext.Length, resp.Length); i++)
            {
                Assert.AreEqual(ext[i], resp[i]);
            }
            Assert.AreEqual(ext, resp);
        }
    }
}
