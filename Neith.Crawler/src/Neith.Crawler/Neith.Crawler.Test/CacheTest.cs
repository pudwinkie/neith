using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Neith.Crawler;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Neith.Crawler.Test
{
    using NUnit.Framework;
    [TestFixture]
    public class CacheTest
    {
        [Test]
        public void HashTest()
        {
            "TestTest"
                .ToHashString()
                .AreEqual("ffcbc4GfLgxhgzmipFMIqXdePG8");

            var cache = CrawlerCache.Create("TestTest");
            cache.Hash.AreEqual("ffcbc4GfLgxhgzmipFMIqXdePG8");
        }

        private static readonly Random rand = new Random();

        [Test]
        public void CrearTest()
        {
            var contents = rand.Next().ToString();
            var cache = CrawlerCache.Create("Test1");
            cache.WriteAllText("test.txt", contents);
            cache.ReadAllText("test.txt").AreEqual(contents);
            cache.Clear();
            var nullVal = cache.ReadAllText("test.txt");
            Assert.IsNull(nullVal);
            // もう一回書いておく
            contents = rand.Next().ToString();
            cache.WriteAllText("test.txt", contents);
            cache.ReadAllText("test.txt").AreEqual(contents);
        }

    }
}
