using System;
using System.Collections.Generic;
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
                .Merge(EnTask())
                .LastOrDefault();
        }

        private static IEnumerable<IObservable<bool>> EnTask()
        {
            yield return SITES.Neith.NeithXFN.ReadTypes();
        }
    }
}
