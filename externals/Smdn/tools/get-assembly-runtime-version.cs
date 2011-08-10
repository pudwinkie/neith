using System;
using System.Reflection;

public static class MainClass {
  public static void Main(string[] args)
  {
    try {
      var assm = Assembly.LoadFrom(args[0]);

      Console.WriteLine(assm.ImageRuntimeVersion);
    }
    catch (Exception ex) {
      Console.Error.WriteLine("error occurred, input file = {0}", args[0]);
      Console.Error.WriteLine(ex);
    }
  }
}
