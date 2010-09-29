using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            yield break;



        }
    }
}
