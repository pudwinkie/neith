using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public static class MainClass {
  public static void Main(string[] args)
  {
    bool printName = false;
    string path = null;

    for (var i = 0; i < args.Length; i++) {
      switch (args[i]) {
        case "--with-name":
          printName = true;
          break;

        default:
          path = args[i];
          break;
      }
    }

    if (File.Exists(path)) {
      PrintVersion(Assembly.LoadFrom(path), printName);
    }
    else if (Directory.Exists(path)) {
      var assms = new List<Assembly>();

      foreach (var ext in new[] {"*.dll", "*.exe"}) {
        foreach (var file in Directory.GetFiles(path, ext, SearchOption.TopDirectoryOnly)) {
          var assm = Assembly.LoadFrom(file);

          assms.Add(assm);
        }
      }

      foreach (var targetAssm in assms.ToArray()) {
        var targetName = targetAssm.GetName();
        var referenced = 0;

        foreach (var assm in assms.ToArray()) {
          foreach (var referencedAssmName in assm.GetReferencedAssemblies()) {
            if (targetName.Name == referencedAssmName.Name)
              referenced++;
          }
        }

        if (referenced == 0) {
          PrintVersion(targetAssm, printName);
          return;
        }
      }

      Console.WriteLine("0.0");
    }
  }

  private static void PrintVersion(Assembly assm, bool printName)
  {
    if (printName)
      Console.Write("{0}-", assm.GetName().Name);

    Console.WriteLine("{0}.{1:D2}", assm.GetName().Version.Major, assm.GetName().Version.Minor);
  }
}
