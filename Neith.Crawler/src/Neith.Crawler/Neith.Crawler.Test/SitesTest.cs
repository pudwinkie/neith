using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SITES = Neith.Crawler.Sites;

namespace Neith.Crawler.Test
{
    using NUnit.Framework;
    //[TestFixture]
    public class SitesTest
    {
        [Test]
        public void NeithXFNTest()
        {
            SITES.Neith.Types.CrawlerTask.Run();
        }
    }
}
