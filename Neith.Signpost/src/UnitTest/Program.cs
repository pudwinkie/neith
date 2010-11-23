using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitTest
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Neith.Util.UnitTest.ConsoleRunner.Run(args);
        }
    }
}
