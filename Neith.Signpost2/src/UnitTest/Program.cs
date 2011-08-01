using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitTest
{
    class Program
    {
        static int Main(string[] args)
        {
            var p = new List<string>(args);
            p.Add("/failstop");
            return Neith.XUnit.Console.Program.Main(p.ToArray());
        }
    }
}
