using System;
using NUnit.Framework;

using Smdn.Net.Pop3.Client.Session;

namespace Smdn.Net.Pop3.Client {
  internal static class TestUtils {
    public static void ExpectExceptionThrown<TException>(Action action) where TException : Exception
    {
      try {
        action();
        Assert.Fail("expected exception not thrown: {0}", typeof(TException));
      }
      catch (TException) {
      }
    }

    public static void TestAuthenticated(Action<PopPseudoServer, PopClient> testAction)
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA
        server.EnqueueResponse("+OK\r\n" +
                               ".\r\n");
        // USER/PASS
        server.EnqueueResponse("+OK\r\n");
        server.EnqueueResponse("+OK\r\n");

        using (var client = new PopClient(new Uri(string.Format("pop://user@{0}/", server.HostPort)))) {
          client.Timeout = 5000;
          client.Profile.UsingSaslMechanisms = null;
          client.Profile.UseTlsIfAvailable = false;
          client.Profile.AllowInsecureLogin = true;

          client.Connect("pass");

          Assert.AreEqual(server.DequeueRequest(), "CAPA\r\n");
          Assert.AreEqual(server.DequeueRequest(), "USER user\r\n");
          Assert.AreEqual(server.DequeueRequest(), "PASS pass\r\n");

          testAction(server, client);
        }
      }
    }
  }
}
