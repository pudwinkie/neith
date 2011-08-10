using System;
using System.Collections.Generic;
using System.Text;

namespace Neith.Util.UnitTest
{
    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            return Neith.Util.UnitTest.ConsoleRunner.Run(args);
        }
    }
}
