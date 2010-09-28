using System;
using System.Collections.Generic;
using System.Text;

namespace CSUtil.UnitTest
{
  public static class ConsoleRunner
  {
    public static int Run(string[] args) { return NUnit.ConsoleRunner.Program.Run(args); }
  }
}
