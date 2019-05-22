using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DNSServer;
using System.Text;

namespace DNSTest
{
    [TestClass]
    public class TreeTests
    {
        [TestMethod]
        public void SimpleTreeTest()
        {
            var tree = new SuffixTree();
            var outAddr = 0;

            var len1 = tree.ReadNameAndUpdate("a.b.c", ref outAddr, 125);
            Assert.AreEqual(0, len1);

            var len2 = tree.ReadNameAndUpdate("A.B.C", ref outAddr, 450);
            Assert.AreEqual(0, len2);

            var len3 = tree.ReadNameAndUpdate("A.b.c", ref outAddr, 780);
            Assert.AreEqual(4, len3);
            Assert.AreEqual(127, outAddr);
        }


        [TestMethod]
        public void ComplexTreeTest()
        {
            var tree = new SuffixTree();
            var outAddr = 0;

            var len1 = tree.ReadNameAndUpdate("alba.bibi.cum.dipra.", ref outAddr, 125);
            Assert.AreEqual(0, len1);

            var len2 = tree.ReadNameAndUpdate("cum.dipra", ref outAddr, 450);
            Assert.AreEqual(Encoding.UTF8.GetBytes("cumdipra").Length + 2, len2);
            Assert.AreEqual(135, outAddr);

            var len3 = tree.ReadNameAndUpdate("bibi.cum.dipra", ref outAddr, 780);
            Assert.AreEqual(15, len3);
            Assert.AreEqual(130, outAddr);
        }
    }
}
