using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Crawler.Test
{
    using NUnit.Framework;
    [TestFixture]
    public class SitesTest
    {
        [Test]
        public void NeithXFNTest()
        {
            Neith.Crawler.Sites.Neith.NeithXFN.ReadTypes()
                .Run();
        }
    }
}
