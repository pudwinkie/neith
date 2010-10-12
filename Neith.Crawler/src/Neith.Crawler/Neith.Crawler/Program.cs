using System;
using System.Collections.Generic;
using System.Concurrency;
using System.Linq;
using System.Text;
using SITES = Neith.Crawler.Sites;

namespace Neith.Crawler
{
    class Program
    {
        static void Main(string[] args)
        {
            Run();
        }

        public static void Run()
        {
            EnCrawlTask()
                .AsParallel()
                .ForAll(a=>{});
        }

        private static IEnumerable<Unit> EnCrawlTask()
        {
            yield return SITES.Neith.Types.CrawlerTask.Run();
            yield return SITES.Zam.AbilityList.CrawlerTask.Run();
        }
    }
}
