using System;
using System.Collections.Generic;
using System.Text;

namespace Neith.Util.UnitTest
{
  public static class ConsoleRunner
  {
    public static int Run(string[] args) { return NUnit.ConsoleRunner.Program.Run(args); }
  }
}
