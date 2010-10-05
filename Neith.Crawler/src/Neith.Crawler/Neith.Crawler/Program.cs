﻿using System;
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
            Observable
                .Merge(EnCrawlTask(), Scheduler.ThreadPool)
                .Run();
        }

        private static IEnumerable<IObservable<Unit>> EnCrawlTask()
        {
            yield return SITES.Neith.Types.Task();
            yield return SITES.Zam.AbilityList.Task();
        }
    }
}
