using System;
using System.Collections.Generic;
using NUnit.Framework;

using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Session {
  [TestFixture]
  public class ImapSessionCommandsAnyStateTests : ImapSessionTestsBase {
    [Test]
    public void TestCapability()
    {
      Connect(delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsNotNull(session.ServerCapabilities);
        Assert.AreEqual(0, session.ServerCapabilities.Count);

        server.EnqueueResponse("* CAPABILITY IMAP4rev1 CHILDREN\r\n" +
                               "0000 OK done\r\n");

        ImapCapabilitySet capabilities;

        Assert.IsTrue((bool)session.Capability(out capabilities));

        Assert.AreEqual("0000 CAPABILITY\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(capabilities);
        Assert.AreEqual(2, capabilities.Count);
        Assert.IsTrue(capabilities.Contains(ImapCapability.Imap4Rev1));
        Assert.IsTrue(capabilities.Contains(ImapCapability.Children));

        Assert.IsNotNull(session.ServerCapabilities);
        Assert.AreEqual(2, session.ServerCapabilities.Count);
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.Imap4Rev1));
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.Children));

        try {
          session.ServerCapabilities.Add(ImapCapability.Imap4);
          Assert.Fail("NotSupportedException not thrown");
        }
        catch (NotSupportedException) {
        }
      });
    }

    [Test]
    public void TestCapabilityNotHandleIncapableAsException()
    {
      Connect(delegate(ImapSession session, ImapPseudoServer server) {
        session.HandlesIncapableAsException = true;

        Assert.IsNotNull(session.ServerCapabilities);
        Assert.IsFalse(session.ServerCapabilities.Contains(ImapCapability.Imap4Rev1));

        // CAPABILITY transaction 1
        server.EnqueueResponse("* CAPABILITY IMAP4\r\n" +
                               "0000 OK done\r\n");

        Assert.IsTrue((bool)session.Capability());

        Assert.AreEqual("0000 CAPABILITY\r\n",
                        server.DequeueRequest());

        Assert.IsFalse(session.ServerCapabilities.Contains(ImapCapability.Imap4Rev1));

        // CAPABILITY transaction 2
        server.EnqueueResponse("* CAPABILITY IMAP4\r\n" +
                               "0001 OK done\r\n");

        Assert.IsTrue((bool)session.Capability());

        Assert.AreEqual("0001 CAPABILITY\r\n",
                        server.DequeueRequest());

        Assert.IsFalse(session.ServerCapabilities.Contains(ImapCapability.Imap4Rev1));
      });
    }

    [Test]
    public void TestNoOpInNonAuthenticatedState()
    {
      Connect(delegate(ImapSession session, ImapPseudoServer server) {
        Assert.AreEqual(ImapSessionState.NotAuthenticated, session.State);

        // NOOP transaction
        server.EnqueueResponse("0000 OK done\r\n");

        Assert.IsTrue((bool)session.NoOp());

        Assert.AreEqual("0000 NOOP\r\n",
                        server.DequeueRequest());
      });
    }

    [Test]
    public void TestNoOpInAuthenticatedState()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        Assert.AreEqual(ImapSessionState.Authenticated, session.State);

        // NOOP transaction
        server.EnqueueResponse("0002 OK done\r\n");

        Assert.IsTrue((bool)session.NoOp());

        Assert.AreEqual("0002 NOOP\r\n",
                        server.DequeueRequest());
      });
    }

    [Test]
    public void TestNoOpInSelectedState()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        Assert.AreEqual(ImapSessionState.Selected, session.State);

        // NOOP transaction
        server.EnqueueResponse("0004 OK NOOP completed\r\n");

        Assert.IsTrue((bool)session.NoOp());

        Assert.AreEqual("0004 NOOP\r\n",
                        server.DequeueRequest());

        return 1;
      });
    }

    [Test]
    public void TestNoOpStatusUpdate()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        // NOOP transaction
        server.EnqueueResponse("* 22 EXPUNGE\r\n" +
                               "* 18 EXPUNGE\r\n" +
                               "* 23 EXISTS\r\n" +
                               "* 3 RECENT\r\n" +
                               "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)\r\n" +
                               "* 14 FETCH (FLAGS (\\Seen \\Deleted))\r\n" +
                               "0004 OK NOOP completed\r\n");

        Assert.IsTrue((bool)session.NoOp());

        Assert.AreEqual("0004 NOOP\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(23L, session.SelectedMailbox.ExistsMessage, "selected mailbox exist message count");
        Assert.AreEqual(3L, session.SelectedMailbox.RecentMessage, "selected mailbox recent message count");
        Assert.IsNotNull(session.SelectedMailbox.ApplicableFlags, "selected mailbox applicable flags");
        Assert.AreEqual(5, session.SelectedMailbox.ApplicableFlags.Count);
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Contains(ImapMessageFlag.Answered));
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Contains(ImapMessageFlag.Flagged));
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Contains(ImapMessageFlag.Deleted));
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Contains(ImapMessageFlag.Seen));
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Contains(ImapMessageFlag.Draft));

        return 1;
      });
    }

    [Test]
    public void TestNoOpStatusUpdateExistsBroken()
    {
      SelectMailbox(1, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.AreEqual(1L, session.SelectedMailbox.ExistsMessage);

        // NOOP transaction
        server.EnqueueResponse("* 1 EXPUNGE\r\n" +
                               "* 1 EXPUNGE\r\n" +
                               "* 1 EXPUNGE\r\n" +
                               "0004 OK NOOP completed\r\n");

        Assert.IsTrue((bool)session.NoOp());

        Assert.AreEqual("0004 NOOP\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(0L, session.SelectedMailbox.ExistsMessage);

        return 1;
      });
    }

    [Test]
    public void TestLogoutInNonAuthenticatedState()
    {
      using (var server = CreateServer()) {
        server.EnqueueResponse("* OK ImapSimulatedServer ready\r\n");
        server.EnqueueResponse("* BYE Logging out\r\n" + 
                               "0000 OK logged out.\r\n");

        using (var session = new ImapSession(server.Host, server.Port)) {
          Assert.IsTrue(session.Logout().Code == ImapCommandResultCode.Bye);

          Assert.AreEqual(ImapSessionState.NotConnected, session.State);
          Assert.AreEqual(null, session.Authority);
        }
      }
    }

    [Test]
    public void TestLogoutInAuthenticatedState()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        server.EnqueueResponse("* BYE Logging out\r\n" + 
                               "0001 OK logged out.\r\n");

        Assert.IsTrue(session.Logout().Code == ImapCommandResultCode.Bye);

        Assert.AreEqual("0002 LOGOUT\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(ImapSessionState.NotConnected, session.State);
        Assert.AreEqual(null, session.Authority);
      });
    }

    [Test]
    public void TestLogoutInSelectedState()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsNotNull(session.SelectedMailbox);

        server.EnqueueResponse("* BYE Logging out\r\n" + 
                               "0004 OK logged out.\r\n");

        Assert.IsTrue(session.Logout().Code == ImapCommandResultCode.Bye);

        Assert.AreEqual("0004 LOGOUT\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(ImapSessionState.NotConnected, session.State);
        Assert.AreEqual(null, session.Authority);
        Assert.IsNull(session.SelectedMailbox);

        return -1;
      });
    }

    [Test]
    public void TestID()
    {
      Authenticate(new[] {"ID"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.ID));

        // ID transaction
        server.EnqueueResponse("* ID NIL\r\n" +
                               "0002 OK ID completed\r\n");

        IDictionary<string, string> serverParameterList;

        Assert.IsTrue((bool)session.ID(new Dictionary<string, string>() {
          {"name", "sodr"},
          {"version", "19.34"},
          {"vendor", "Pink Floyd Music Limited"},
        }, out serverParameterList));

        Assert.AreEqual("0002 ID (\"name\" \"sodr\" \"version\" \"19.34\" \"vendor\" \"Pink Floyd Music Limited\")\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(serverParameterList);
        Assert.AreEqual(0, serverParameterList.Count);
        Assert.IsTrue(serverParameterList.IsReadOnly, "returns read-only dictionary");
        Assert.AreEqual(0, session.ServerID.Count);

        try {
          session.ServerID.Add("key", "value");
          Assert.Fail("NotSupportedException not thrown");
        }
        catch (NotSupportedException) {
        }
      });
    }

    [Test]
    public void TestIDParameterNil()
    {
      Authenticate(new[] {"ID"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.ID));

        // ID transaction
        server.EnqueueResponse("* ID (\"name\" \"Cyrus\" \"version\" \"1.5\")\r\n" +
                               "0002 OK ID completed\r\n");

        Assert.IsTrue((bool)session.ID(new Dictionary<string, string>() {
          {"name", null},
          {"version", null},
          {"vendor", null},
        }));

        Assert.AreEqual("0002 ID (\"name\" NIL \"version\" NIL \"vendor\" NIL)\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(2, session.ServerID.Count);
        Assert.IsTrue(session.ServerID.IsReadOnly, "dictionary is read-only");
        Assert.AreEqual("Cyrus", session.ServerID["name"]);
        Assert.AreEqual("Cyrus", session.ServerID["NAME"]);
        Assert.AreEqual("1.5", session.ServerID["version"]);
        Assert.AreEqual("1.5", session.ServerID["VERSION"]);

        try {
          session.ServerID.Add("key", "value");
          Assert.Fail("NotSupportedException not thrown");
        }
        catch (NotSupportedException) {
        }
      });
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestIDTooMatchFieldValuePair()
    {
      Authenticate(new[] {"ID"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.ID));

        var paramList = new Dictionary<string, string>();

        for (var i = 0; i < 31; i++) {
          paramList.Add(string.Format("field{0}", i), string.Format("value{0}", i));
        }

        session.ID(paramList);
      });
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestIDTooLongField()
    {
      Authenticate(new[] {"ID"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.ID));

        var paramList = new Dictionary<string, string>();

        Assert.IsTrue((bool)session.ID(new Dictionary<string, string>() {
          {new string('x', 31), "value"},
        }));

        session.ID(paramList);
      });
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestIDTooLongValue()
    {
      Authenticate(new[] {"ID"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.ID));

        var paramList = new Dictionary<string, string>();

        Assert.IsTrue((bool)session.ID(new Dictionary<string, string>() {
          {"field", new string('x', 1025)},
        }));

        session.ID(paramList);
      });
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestIDContaisNoAsciiChar()
    {
      Authenticate(new[] {"ID"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.ID));

        var paramList = new Dictionary<string, string>();

        Assert.IsTrue((bool)session.ID(new Dictionary<string, string>() {
          {"オワタ", "＼(＾o＾)／"},
        }));

        session.ID(paramList);
      });
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestLanguageIncapable()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsFalse(session.ServerCapabilities.Contains(ImapCapability.Language));

        session.HandlesIncapableAsException = true;

        session.Language();
      });
    }

    [Test]
    public void TestLanguageListSupported()
    {
      Authenticate(new[] {"LANGUAGE"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.Language));
        Assert.IsNull(session.SelectedLanguage);

        // LANGUAGE transaction
        server.EnqueueResponse("* LANGUAGE (EN DE IT i-default)\r\n" +
                               "0002 OK Supported languages have been enumerated\r\n");

        string[] supportedLanguageTags;

        Assert.IsTrue((bool)session.Language(out supportedLanguageTags));

        Assert.AreEqual("0002 LANGUAGE\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(4, supportedLanguageTags.Length);
        Assert.AreEqual("EN", supportedLanguageTags[0]);
        Assert.AreEqual("DE", supportedLanguageTags[1]);
        Assert.AreEqual("IT", supportedLanguageTags[2]);
        Assert.AreEqual("i-default", supportedLanguageTags[3]);
      });
    }

    [Test]
    public void TestLanguageSelectDefault()
    {
      Authenticate(new[] {"LANGUAGE"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.Language));
        Assert.IsNull(session.SelectedLanguage);

        // LANGUAGE transaction
        server.EnqueueResponse("* LANGUAGE (DE)\r\n" +
                               "0002 OK Sprachwechsel durch LANGUAGE-Befehl ausgeführt\r\n");

        string selectedLanguageTag;

        var result = session.Language(out selectedLanguageTag);

        Assert.IsTrue((bool)result);
        Assert.AreEqual("DE", selectedLanguageTag);
        Assert.AreEqual("Sprachwechsel durch LANGUAGE-Befehl ausgeführt", result.TaggedStatusResponse.ResponseText.Text);

        Assert.AreEqual("0002 LANGUAGE default\r\n",
                        server.DequeueRequest());

        Assert.AreEqual("DE", session.SelectedLanguage);
      });
    }

    [Test]
    public void TestLanguageSelectSupported()
    {
      Authenticate(new[] {"LANGUAGE"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.Language));
        Assert.IsNull(session.SelectedLanguage);

        // LANGUAGE transaction
        server.EnqueueResponse("* LANGUAGE (EN)\r\n" +
                               "0002 OK Now speaking English\r\n");

        string selectedLanguageTag;

        var result = session.Language(out selectedLanguageTag, "FR-CA", "EN-CA");

        Assert.IsTrue((bool)result);
        Assert.AreEqual("EN", selectedLanguageTag);
        Assert.AreEqual("Now speaking English", result.TaggedStatusResponse.ResponseText.Text);

        Assert.AreEqual("0002 LANGUAGE FR-CA EN-CA\r\n",
                        server.DequeueRequest());

        Assert.AreEqual("EN", session.SelectedLanguage);
      });
    }

    [Test]
    public void TestLanguageSelectWithCultureInfo()
    {
      Authenticate(new[] {"LANGUAGE"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.Language));
        Assert.IsNull(session.SelectedLanguage);

        // LANGUAGE transaction
        server.EnqueueResponse("0002 NO Unsupported language\r\n");

        var result = session.Language(System.Globalization.CultureInfo.GetCultureInfo("ja-JP"));

        Assert.IsFalse((bool)result);
        Assert.AreEqual("Unsupported language", result.TaggedStatusResponse.ResponseText.Text);

        Assert.AreEqual("0002 LANGUAGE ja-JP\r\n",
                        server.DequeueRequest());

        Assert.IsNull(session.SelectedLanguage);
      });
    }

    [Test]
    public void TestLanguageNamespaceResponse()
    {
      Authenticate(new[] {"LANGUAGE"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.Language));
        Assert.IsNull(session.SelectedLanguage);

        // LANGUAGE transaction
        server.EnqueueResponse("* LANGUAGE (DE)\r\n" +
                               "* NAMESPACE ((\"\" \"/\")) ((\"Other Users/\" \"/\" \"TRANSLATION\"" +
                                  " (\"Andere Ben&APw-tzer/\"))) ((\"Public Folders/\" \"/\" " +
                                  " \"TRANSLATION\" (\"Gemeinsame Postf&AM8-cher/\")))\r\n" +
                               "0002 OK Sprachwechsel durch LANGUAGE-Befehl ausgeführt\r\n");

        var result = session.Language("DE");

        Assert.IsTrue((bool)result);
        Assert.AreEqual("Sprachwechsel durch LANGUAGE-Befehl ausgeführt", result.TaggedStatusResponse.ResponseText.Text);

        Assert.AreEqual("0002 LANGUAGE DE\r\n",
                        server.DequeueRequest());

        Assert.AreEqual("DE", session.SelectedLanguage);

        Assert.AreEqual(1, session.Namespaces.PersonalNamespaces.Length);
        Assert.AreEqual(string.Empty, session.Namespaces.PersonalNamespaces[0].Prefix);
        Assert.AreEqual("/", session.Namespaces.PersonalNamespaces[0].HierarchyDelimiter);

        Assert.AreEqual(1, session.Namespaces.OtherUsersNamespaces.Length);
        Assert.AreEqual("Other Users/", session.Namespaces.OtherUsersNamespaces[0].Prefix);
        Assert.AreEqual("/", session.Namespaces.OtherUsersNamespaces[0].HierarchyDelimiter);
        Assert.AreEqual("Andere Benützer/", session.Namespaces.OtherUsersNamespaces[0].TranslatedPrefix);

        Assert.AreEqual(1, session.Namespaces.SharedNamespaces.Length);
        Assert.AreEqual("Public Folders/", session.Namespaces.SharedNamespaces[0].Prefix);
        Assert.AreEqual("/", session.Namespaces.SharedNamespaces[0].HierarchyDelimiter);
        // RFC error? (Gemeinsame Postfächer)
        Assert.AreEqual("Gemeinsame PostfÏcher/", session.Namespaces.SharedNamespaces[0].TranslatedPrefix);
      });
    }

    [Test]
    public void TestLanguageReceiveResponseAsUTF8()
    {
      Authenticate(new[] {"LANGUAGE"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.Language));
        Assert.IsNull(session.SelectedLanguage);

        // LANGUAGE transaction
        server.EnqueueResponse("* LANGUAGE (JA)\r\n" +
                               "0002 OK 完了\r\n");

        var result = session.Language("JA");

        Assert.IsTrue((bool)result);
        Assert.AreEqual("完了", result.TaggedStatusResponse.ResponseText.Text);

        Assert.AreEqual("0002 LANGUAGE JA\r\n",
                        server.DequeueRequest());

        // following command/response
        server.EnqueueResponse("0003 OK 完了\r\n");

        session.NoOp();

        Assert.IsTrue((bool)result);
        Assert.AreEqual("完了", result.TaggedStatusResponse.ResponseText.Text);
      });
    }
  }
}
