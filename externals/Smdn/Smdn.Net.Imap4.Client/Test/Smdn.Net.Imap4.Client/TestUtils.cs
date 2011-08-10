using System;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

using Smdn.Net.Imap4.Client.Session;

namespace Smdn.Net.Imap4.Client {
  public static class TestUtils {
    public static void ExpectExceptionThrown<TException>(Action action) where TException : Exception
    {
      try {
        action();
        Assert.Fail("expected exception not thrown: {0}", typeof(TException));
      }
      catch (TException) {
      }
    }

    public static void TestAuthenticated(Action<ImapPseudoServer, ImapClient> testAction)
    {
      TestAuthenticated(new ImapCapability[0], testAction);
    }

    public static void TestAuthenticated(ImapCapability[] serverCapabilities, Action<ImapPseudoServer, ImapClient> testAction)
    {
      var capas = new ImapCapabilitySet(serverCapabilities ?? new ImapCapability[0]);

      try {
        capas.Add(ImapCapability.Imap4Rev1);
      }
      catch {
        // ignore exception
      }

      using (var server = new ImapPseudoServer()) {
        server.Start();

        // greeting
        server.EnqueueResponse("* OK ImapPseudoServer ready\r\n");
        // CAPABILITY
        server.EnqueueTaggedResponse("* CAPABILITY IMAP4rev1\r\n" +
                                     "$tag OK done\r\n");
        // LOGIN
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // CAPABILITY
        server.EnqueueTaggedResponse(string.Format("* CAPABILITY {0}\r\n", string.Join(" ", capas.ToStringArray())) +
                                     "$tag OK done\r\n");

        using (var client = new ImapClient(new Uri(string.Format("imap://user@{0}/", server.HostPort)))) {
          client.Timeout = 5000;
          client.Profile.AllowInsecureLogin = true;
          client.Profile.UsingSaslMechanisms = null;
          client.Profile.UseTlsIfAvailable = false;

          client.Connect("pass");

          StringAssert.EndsWith("CAPABILITY\r\n", server.DequeueRequest());
          StringAssert.EndsWith("LOGIN user pass\r\n", server.DequeueRequest());
          StringAssert.EndsWith("CAPABILITY\r\n", server.DequeueRequest());

          testAction(server, client);
        }
      }
    }

    public static void TestOpenedMailbox(string mailboxName, Action<ImapPseudoServer, ImapOpenedMailboxInfo> action)
    {
      TestOpenedMailbox(null, mailboxName, "$tag OK [READ-WRITE] done\r\n", action);
    }

    public static void TestOpenedMailbox(string mailboxName, string selectResponse, Action<ImapPseudoServer, ImapOpenedMailboxInfo> action)
    {
      TestOpenedMailbox(null, mailboxName, selectResponse, action);
    }

    public static void TestOpenedMailbox(ImapCapability[] capabilities, string mailboxName, string selectResponse, Action<ImapPseudoServer, ImapOpenedMailboxInfo> action)
    {
      TestUtils.TestAuthenticated(capabilities, delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse(string.Format("* LIST () \"\" {0}\r\n", mailboxName) +
                                     "$tag OK done\r\n");
        // SELECT
        server.EnqueueTaggedResponse(selectResponse);

        using (var opened = client.OpenMailbox(mailboxName)) {
          server.DequeueRequest(); // LIST
          server.DequeueRequest(); // SELECT

          try {
            Assert.IsTrue(opened.IsOpen);
            Assert.AreEqual(opened.IsReadOnly, selectResponse.Contains("OK [READ-ONLY]"));
            Assert.AreEqual(mailboxName, opened.FullName);
            Assert.IsNotNull(opened.ApplicableFlags);
            Assert.IsNotNull(opened.PermanentFlags);

            action(server, opened);
          }
          finally {
            // CLOSE
            if (opened.IsOpen)
              server.EnqueueTaggedResponse("$tag OK done\r\n");
          }
        }
      });
    }

    public static void TestClosedMailbox(string mailboxName, Action<ImapPseudoServer, ImapOpenedMailboxInfo> action)
    {
      TestOpenedMailbox(mailboxName, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        // CLOSE
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        mailbox.Close();

        Assert.IsFalse(mailbox.IsOpen);
        Assert.IsNull(mailbox.Client.OpenedMailbox);

        Assert.That(server.DequeueRequest(), Text.EndsWith("CLOSE\r\n"));

        action(server, mailbox);
      });
    }
  }
}
