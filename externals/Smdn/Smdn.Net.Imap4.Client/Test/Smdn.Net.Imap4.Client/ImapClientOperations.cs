using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif
using Smdn.Net.Imap4.Client.Session;
using Smdn.Net.Imap4.Protocol;

namespace Smdn.Net.Imap4.Client {
  [TestFixture]
  public class ImapClientOperationsTests {
    [Test]
    public void TestGetInbox()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" INBOX\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.GetInbox();

        StringAssert.EndsWith("LIST \"\" INBOX\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("INBOX", mailbox.Name);
        Assert.AreEqual("INBOX", mailbox.FullName);
        Assert.IsTrue(mailbox.IsInbox);
      });
    }

    [Test]
    public void TestGetMailbox()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" Draft\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.GetMailbox("Draft");

        StringAssert.EndsWith("LIST \"\" Draft\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("Draft", mailbox.Name);
        Assert.AreEqual("Draft", mailbox.FullName);
        Assert.IsFalse(mailbox.IsInbox);
      });
    }

    [Test]
    public void TestGetMailboxNotFound()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        try {
          client.GetMailbox("NonExistent");
          Assert.Fail("ImapMailboxNotFoundException not thrown");
        }
        catch (ImapMailboxNotFoundException ex) {
          Assert.IsNotNull(ex.Mailbox);
          Assert.AreEqual("NonExistent", ex.Mailbox);

          Smdn.Net.TestUtils.SerializeBinary(ex, delegate(ImapMailboxNotFoundException deserialized) {
            Assert.IsNotNull(deserialized.Mailbox);
            Assert.AreEqual(ex.Mailbox, deserialized.Mailbox);
          });
        }
      });
    }

    [Test]
    public void TestGetMailboxNameContainsWildcard()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"/\" Box*/Child1\r\n" +
                                     "* LIST () \"/\" Box*/Child2\r\n" +
                                     "* LIST () \"/\" Box*\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.GetMailbox("Box*");

        StringAssert.EndsWith("LIST \"\" Box*\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("Box*", mailbox.Name);
        Assert.AreEqual("Box*", mailbox.FullName);
        Assert.IsFalse(mailbox.IsInbox);
      });
    }

    [Test]
    public void TestGetMailboxNameContainsWildcardRequestStatus()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"/\" Box*/Child1\r\n" +
                                     "* LIST () \"/\" Box*/Child2\r\n" +
                                     "* LIST () \"/\" Box*\r\n" +
                                     "$tag OK done\r\n");
        // STATUS
        server.EnqueueTaggedResponse("* STATUS \"Box*\" (MESSAGES 3 RECENT 1 UIDNEXT 4 UIDVALIDITY 1 UNSEEN 1)\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.GetMailbox("Box*", ImapMailboxListOptions.RequestStatus);

        StringAssert.EndsWith("LIST \"\" Box*\r\n", server.DequeueRequest());
        StringAssert.EndsWith("STATUS \"Box*\" (MESSAGES RECENT UIDNEXT UIDVALIDITY UNSEEN)\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("Box*", mailbox.Name);
        Assert.AreEqual("Box*", mailbox.FullName);
        Assert.AreEqual(3L, mailbox.ExistMessageCount, "ExistMessageCount");
        Assert.AreEqual(4L, mailbox.NextUid, "NextUid");
        Assert.IsFalse(mailbox.IsInbox);
      });
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestGetMailboxNameNull()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        client.GetMailbox((string)null);
      });
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestGetMailboxNameEmpty()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        client.GetMailbox(string.Empty);
      });
    }

    [Test]
    public void TestGetMailboxSpecialMailboxSpecialUse()
    {
      GetMailboxSpecialMailboxSpecialUse(false);
    }

    [Test]
    public void TestGetMailboxSpecialMailboxSpecialUseRequestStatus()
    {
      GetMailboxSpecialMailboxSpecialUse(true);
    }

    private void GetMailboxSpecialMailboxSpecialUse(bool requestStatus)
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.SpecialUse},
                                  delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST (\\Marked \\HasNoChildren) \"/\" Inbox\r\n" +
                                     "* LIST (\\HasNoChildren) \"/\" ToDo\r\n" +
                                     "* LIST (\\HasChildren) \"/\" Projects\r\n" +
                                     "* LIST (\\Sent \\HasNoChildren) \"/\" SentMail\r\n" +
                                     "* LIST (\\Marked \\Drafts \\HasNoChildren) \"/\" MyDrafts\r\n" +
                                     "* LIST (\\Trash \\HasNoChildren) \"/\" Trash\r\n" +
                                     "$tag OK done\r\n");
        if (requestStatus)
          // STATUS
          server.EnqueueTaggedResponse("* STATUS SentMail (MESSAGES 3 RECENT 1 UIDNEXT 4 UIDVALIDITY 1 UNSEEN 1)\r\n" +
                                       "$tag OK done\r\n");

        var mailbox = requestStatus
          ? client.GetMailbox(ImapSpecialMailbox.Sent, ImapMailboxListOptions.RequestStatus)
          : client.GetMailbox(ImapSpecialMailbox.Sent);

        StringAssert.EndsWith("LIST \"\" *\r\n", server.DequeueRequest());

        // STATUS
        if (requestStatus)
          StringAssert.EndsWith("STATUS SentMail (MESSAGES RECENT UIDNEXT UIDVALIDITY UNSEEN)\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("SentMail", mailbox.Name);
        Assert.AreEqual("SentMail", mailbox.FullName);
        CollectionAssert.Contains(mailbox.Flags, ImapMailboxFlag.Sent);

        if (requestStatus) {
          Assert.AreEqual(3L, mailbox.ExistMessageCount, "ExistMessageCount");
          Assert.AreEqual(4L, mailbox.NextUid, "NextUid");
        }
      });
    }

    [Test]
    [Ignore("to be added")]
    public void TestGetMailboxSpecialMailboxSpecialUseSubscribedOnly()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.SpecialUse},
                                  delegate(ImapPseudoServer server, ImapClient client) {
        // LSUB
        server.EnqueueTaggedResponse("$tag NO to be added\r\n");

        var mailbox = client.GetMailbox(ImapSpecialMailbox.Sent,
                                        ImapMailboxListOptions.SubscribedOnly);

        StringAssert.EndsWith("LSUB \"\" *\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
      });
    }

    [Test]
    [Ignore("to be added")]
    public void TestGetMailboxSpecialMailboxSpecialUseRemote()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.MailboxReferrals, ImapCapability.SpecialUse},
                                  delegate(ImapPseudoServer server, ImapClient client) {
        // RLIST
        server.EnqueueTaggedResponse("$tag NO to be added\r\n");

        var mailbox = client.GetMailbox(ImapSpecialMailbox.Sent,
                                        ImapMailboxListOptions.Remote);

        StringAssert.EndsWith("RLIST \"\" *\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
      });
    }

    [Test, ExpectedException(typeof(ImapMailboxNotFoundException))]
    public void TestGetMailboxSpecialMailboxSpecialUseNotFound()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.SpecialUse},
                                  delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST (\\Marked \\HasNoChildren) \"/\" Inbox\r\n" +
                                     "* LIST (\\HasNoChildren) \"/\" ToDo\r\n" +
                                     "* LIST (\\HasChildren) \"/\" Projects\r\n" +
                                     "* LIST (\\Sent \\HasNoChildren) \"/\" SentMail\r\n" +
                                     "* LIST (\\Marked \\Drafts \\HasNoChildren) \"/\" MyDrafts\r\n" +
                                     "* LIST (\\Trash \\HasNoChildren) \"/\" Trash\r\n" +
                                     "$tag OK done\r\n");

        client.GetMailbox(ImapSpecialMailbox.All);
      });
    }

    [Test]
    public void TestGetMailboxSpecialMailboxListExtended()
    {
      GetMailboxSpecialMailboxListExtended(false);
    }

    [Test]
    public void TestGetMailboxSpecialMailboxListExtendedRequestStatus()
    {
      GetMailboxSpecialMailboxListExtended(true);
    }

    private void GetMailboxSpecialMailboxListExtended(bool requestStatus)
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.ListExtended, ImapCapability.SpecialUse},
                                  delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST (\\Sent) \"/\" SentMail\r\n" +
                                     "* LIST (\\Marked \\Drafts) \"/\" MyDrafts\r\n" +
                                     "* LIST (\\Trash) \"/\" Trash\r\n" +
                                     "$tag OK done\r\n");
        if (requestStatus)
          // STATUS
          server.EnqueueTaggedResponse("* STATUS Trash (MESSAGES 3 RECENT 1 UIDNEXT 4 UIDVALIDITY 1 UNSEEN 1)\r\n" +
                                       "$tag OK done\r\n");

        var mailbox = requestStatus
          ? client.GetMailbox(ImapSpecialMailbox.Trash, ImapMailboxListOptions.RequestStatus)
          : client.GetMailbox(ImapSpecialMailbox.Trash);

        StringAssert.EndsWith("LIST (SPECIAL-USE) \"\" * RETURN (CHILDREN)\r\n", server.DequeueRequest());

        if (requestStatus)
          StringAssert.EndsWith("STATUS Trash (MESSAGES RECENT UIDNEXT UIDVALIDITY UNSEEN)\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("Trash", mailbox.Name);
        Assert.AreEqual("Trash", mailbox.FullName);
        CollectionAssert.Contains(mailbox.Flags, ImapMailboxFlag.Trash);

        if (requestStatus) {
          Assert.AreEqual(3L, mailbox.ExistMessageCount, "ExistMessageCount");
          Assert.AreEqual(4L, mailbox.NextUid, "NextUid");
        }
      });
    }

    [Test]
    public void TestGetMailboxSpecialMailboxListExtendedRequestStatusListStatusCapable()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.ListExtended, ImapCapability.SpecialUse, ImapCapability.ListStatus},
                                  delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST (\\Sent) \"/\" SentMail\r\n" +
                                     "* STATUS SentMail ()\r\n" +
                                     "* LIST (\\Marked \\Drafts) \"/\" MyDrafts\r\n" +
                                     "* STATUS MyDrafts (MESSAGES 3 RECENT 1 UIDNEXT 4 UIDVALIDITY 1 UNSEEN 1 HIGHESTMODSEQ 1)\r\n" +
                                     "* LIST (\\Trash) \"/\" Trash\r\n" +
                                     "* STATUS Trash ()\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.GetMailbox(ImapSpecialMailbox.Drafts, ImapMailboxListOptions.RequestStatus);

        StringAssert.EndsWith("LIST (SPECIAL-USE) \"\" * RETURN (CHILDREN STATUS (MESSAGES RECENT UIDNEXT UIDVALIDITY UNSEEN))\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("MyDrafts", mailbox.Name);
        Assert.AreEqual("MyDrafts", mailbox.FullName);
        CollectionAssert.Contains(mailbox.Flags, ImapMailboxFlag.Drafts);

        Assert.AreEqual(3L, mailbox.ExistMessageCount, "ExistMessageCount");
        Assert.AreEqual(4L, mailbox.NextUid, "NextUid");
      });
    }

    [Test]
    [Ignore("to be added")]
    public void TestGetMailboxSpecialMailboxListExtendedSubscribedOnly()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.ListExtended, ImapCapability.SpecialUse},
                                  delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("$tag NO to be added\r\n");

        var mailbox = client.GetMailbox(ImapSpecialMailbox.Sent,
                                        ImapMailboxListOptions.SubscribedOnly);

        StringAssert.EndsWith("LIST \"\" *\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
      });
    }

    [Test]
    [Ignore("to be added")]
    public void TestGetMailboxSpecialMailboxListExtendedRemote()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.ListExtended, ImapCapability.SpecialUse},
                                  delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("$tag NO to be added\r\n");

        var mailbox = client.GetMailbox(ImapSpecialMailbox.Sent,
                                        ImapMailboxListOptions.Remote);

        StringAssert.EndsWith("LIST \"\" *\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
      });
    }

    [Test, ExpectedException(typeof(ImapMailboxNotFoundException))]
    public void TestGetMailboxSpecialMailboxListExtendedNotFound()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.ListExtended, ImapCapability.SpecialUse},
                                  delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST (\\Sent) \"/\" SentMail\r\n" +
                                     "* LIST (\\Marked \\Drafts) \"/\" MyDrafts\r\n" +
                                     "* LIST (\\Trash) \"/\" Trash\r\n" +
                                     "$tag OK done\r\n");

        client.GetMailbox(ImapSpecialMailbox.Junk);
      });
    }

    [Test]
    public void TestGetMailboxSpecialMailboxXlist()
    {
      GetMailboxSpecialMailboxXlist(false);
    }

    [Test]
    public void TestGetMailboxSpecialMailboxXlistRequestStatus()
    {
      GetMailboxSpecialMailboxXlist(true);
    }

    private void GetMailboxSpecialMailboxXlist(bool requestStatus)
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.GimapXlist},
                                  delegate(ImapPseudoServer server, ImapClient client) {
        // XLIST
        server.EnqueueTaggedResponse("* XLIST (\\HasNoChildren \\Inbox) \"/\" \"&U9dP4TDIMOwwpA-\"\r\n" +
                                     "* XLIST (\\Noselect \\HasChildren) \"/\" \"[Gmail]\"\r\n" +
                                     "* XLIST (\\HasChildren \\HasNoChildren \\AllMail) \"/\" \"[Gmail]/&MFkweTBmMG4w4TD8MOs-\"\r\n" +
                                     "* XLIST (\\HasChildren \\HasNoChildren \\Trash) \"/\" \"[Gmail]/&MLQw33ux-\"\r\n" +
                                     "* XLIST (\\HasNoChildren \\Starred) \"/\" \"[Gmail]/&MLkwvzD8TtgwTQ-\"\r\n" +
                                     "* XLIST (\\HasNoChildren \\Drafts) \"/\" \"[Gmail]/&Tgtm+DBN-\"\r\n" +
                                     "* XLIST (\\HasNoChildren \\Spam) \"/\" \"[Gmail]/&j,dg0TDhMPww6w-\"\r\n" +
                                     "* XLIST (\\HasChildren \\HasNoChildren \\Sent) \"/\" \"[Gmail]/&kAFP4W4IMH8w4TD8MOs-\"\r\n" +
                                     "$tag OK done\r\n");
        if (requestStatus)
          // STATUS
          server.EnqueueTaggedResponse("* STATUS [Gmail]/&kAFP4W4IMH8w4TD8MOs- (MESSAGES 3 RECENT 1 UIDNEXT 4 UIDVALIDITY 1 UNSEEN 1)\r\n" +
                                       "$tag OK done\r\n");

        var mailbox = requestStatus
          ? client.GetMailbox(ImapSpecialMailbox.Sent, ImapMailboxListOptions.RequestStatus)
          : client.GetMailbox(ImapSpecialMailbox.Sent);

        StringAssert.EndsWith("XLIST \"\" *\r\n", server.DequeueRequest());

        if (requestStatus)
          StringAssert.EndsWith("STATUS [Gmail]/&kAFP4W4IMH8w4TD8MOs- (MESSAGES RECENT UIDNEXT UIDVALIDITY UNSEEN)\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("送信済みメール", mailbox.Name);
        Assert.AreEqual("[Gmail]/送信済みメール", mailbox.FullName);
        CollectionAssert.Contains(mailbox.Flags, ImapMailboxFlag.Sent);

        if (requestStatus) {
          Assert.AreEqual(3L, mailbox.ExistMessageCount, "ExistMessageCount");
          Assert.AreEqual(4L, mailbox.NextUid, "NextUid");
        }
      });
    }

    [Test]
    public void TestGetMailboxSpecialMailboxXlistGimapAllMail()
    {
      GetMailboxSpecialMailboxXlistGimapSpecials(ImapSpecialMailbox.All,
                                                 ImapMailboxFlag.GimapAllMail,
                                                 "[Gmail]/すべてのメール");
    }

    [Test]
    public void TestGetMailboxSpecialMailboxXlistGimapStarred()
    {
      GetMailboxSpecialMailboxXlistGimapSpecials(ImapSpecialMailbox.Flagged,
                                                 ImapMailboxFlag.GimapStarred,
                                                 "[Gmail]/スター付き");
    }

    [Test]
    public void TestGetMailboxSpecialMailboxXlistGimapSpam()
    {
      GetMailboxSpecialMailboxXlistGimapSpecials(ImapSpecialMailbox.Junk,
                                                 ImapMailboxFlag.GimapSpam,
                                                 "[Gmail]/迷惑メール");
    }

    private void GetMailboxSpecialMailboxXlistGimapSpecials(ImapSpecialMailbox specialMailbox,
                                                            ImapMailboxFlag expectedMailboxFlag,
                                                            string expectedMailboxFullName)
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.GimapXlist},
                                  delegate(ImapPseudoServer server, ImapClient client) {
        // XLIST
        server.EnqueueTaggedResponse("* XLIST (\\HasNoChildren \\Inbox) \"/\" \"&U9dP4TDIMOwwpA-\"\r\n" +
                                     "* XLIST (\\Noselect \\HasChildren) \"/\" \"[Gmail]\"\r\n" +
                                     "* XLIST (\\HasChildren \\HasNoChildren \\AllMail) \"/\" \"[Gmail]/&MFkweTBmMG4w4TD8MOs-\"\r\n" +
                                     "* XLIST (\\HasChildren \\HasNoChildren \\Trash) \"/\" \"[Gmail]/&MLQw33ux-\"\r\n" +
                                     "* XLIST (\\HasNoChildren \\Starred) \"/\" \"[Gmail]/&MLkwvzD8TtgwTQ-\"\r\n" +
                                     "* XLIST (\\HasNoChildren \\Drafts) \"/\" \"[Gmail]/&Tgtm+DBN-\"\r\n" +
                                     "* XLIST (\\HasNoChildren \\Spam) \"/\" \"[Gmail]/&j,dg0TDhMPww6w-\"\r\n" +
                                     "* XLIST (\\HasChildren \\HasNoChildren \\Sent) \"/\" \"[Gmail]/&kAFP4W4IMH8w4TD8MOs-\"\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.GetMailbox(specialMailbox);

        StringAssert.EndsWith("XLIST \"\" *\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual(expectedMailboxFullName, mailbox.FullName);
        CollectionAssert.Contains(mailbox.Flags, expectedMailboxFlag);
      });
    }

    [Test]
    public void TestGetMailboxSpecialMailboxXlistSubscribedOnly()
    {
      TestGetMailboxSpecialMailboxXlistOptions(ImapMailboxListOptions.SubscribedOnly);
    }

    [Test]
    public void TestGetMailboxSpecialMailboxXlistRemote()
    {
      TestGetMailboxSpecialMailboxXlistOptions(ImapMailboxListOptions.Remote);
    }

    private void TestGetMailboxSpecialMailboxXlistOptions(ImapMailboxListOptions options)
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.GimapXlist},
                                  delegate(ImapPseudoServer server, ImapClient client) {
        // XLIST
        server.EnqueueTaggedResponse("* XLIST (\\HasNoChildren \\Inbox) \"/\" \"&U9dP4TDIMOwwpA-\"\r\n" +
                                     "* XLIST (\\Noselect \\HasChildren) \"/\" \"[Gmail]\"\r\n" +
                                     "* XLIST (\\HasChildren \\HasNoChildren \\AllMail) \"/\" \"[Gmail]/&MFkweTBmMG4w4TD8MOs-\"\r\n" +
                                     "* XLIST (\\HasChildren \\HasNoChildren \\Trash) \"/\" \"[Gmail]/&MLQw33ux-\"\r\n" +
                                     "* XLIST (\\HasNoChildren \\Starred) \"/\" \"[Gmail]/&MLkwvzD8TtgwTQ-\"\r\n" +
                                     "* XLIST (\\HasNoChildren \\Drafts) \"/\" \"[Gmail]/&Tgtm+DBN-\"\r\n" +
                                     "* XLIST (\\HasNoChildren \\Spam) \"/\" \"[Gmail]/&j,dg0TDhMPww6w-\"\r\n" +
                                     "* XLIST (\\HasChildren \\HasNoChildren \\Sent) \"/\" \"[Gmail]/&kAFP4W4IMH8w4TD8MOs-\"\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.GetMailbox(ImapSpecialMailbox.All,
                                        options);

        StringAssert.EndsWith("XLIST \"\" *\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("すべてのメール", mailbox.Name);
        Assert.AreEqual("[Gmail]/すべてのメール", mailbox.FullName);
        CollectionAssert.Contains(mailbox.Flags, ImapMailboxFlag.GimapAllMail);
      });
    }

    [Test, ExpectedException(typeof(ImapMailboxNotFoundException))]
    public void TestGetMailboxSpecialMailboxXlistNotFound()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.ListExtended, ImapCapability.SpecialUse},
                                  delegate(ImapPseudoServer server, ImapClient client) {
        // XLIST
        server.EnqueueTaggedResponse("* XLIST (\\HasNoChildren \\Inbox) \"/\" \"&U9dP4TDIMOwwpA-\"\r\n" +
                                     "* XLIST (\\Noselect \\HasChildren) \"/\" \"[Gmail]\"\r\n" +
                                     "* XLIST (\\HasChildren \\HasNoChildren \\AllMail) \"/\" \"[Gmail]/&MFkweTBmMG4w4TD8MOs-\"\r\n" +
                                     "* XLIST (\\HasChildren \\HasNoChildren \\Trash) \"/\" \"[Gmail]/&MLQw33ux-\"\r\n" +
                                     "* XLIST (\\HasNoChildren \\Starred) \"/\" \"[Gmail]/&MLkwvzD8TtgwTQ-\"\r\n" +
                                     "* XLIST (\\HasNoChildren \\Drafts) \"/\" \"[Gmail]/&Tgtm+DBN-\"\r\n" +
                                     "* XLIST (\\HasNoChildren \\Spam) \"/\" \"[Gmail]/&j,dg0TDhMPww6w-\"\r\n" +
                                     "* XLIST (\\HasChildren \\HasNoChildren \\Sent) \"/\" \"[Gmail]/&kAFP4W4IMH8w4TD8MOs-\"\r\n" +
                                     "$tag OK done\r\n");

        client.GetMailbox(ImapSpecialMailbox.Archive);
      });
    }

    [Test, ExpectedException(typeof(ImapIncapableException))]
    public void TestGetMailboxSpecialMailboxIncapable()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        client.GetMailbox(ImapSpecialMailbox.All);
      });
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestGetMailboxSpecialMailboxInvalidSpecailMailbox()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.ListExtended, ImapCapability.SpecialUse},
                                  delegate(ImapPseudoServer server, ImapClient client) {
        var values = Enum.GetValues(typeof(ImapSpecialMailbox)).Cast<int>().ToArray();

        Array.Sort(values);

        var invalid = (ImapSpecialMailbox)(values[values.Length - 1] + 1);

        client.GetMailbox(invalid);
      });
    }

    [Test]
    public void TestGetOrCreateMailboxExistent()
    {
      GetOrCreateMailboxExistent(false);
    }

    [Test]
    public void TestGetOrCreateMailboxExistentSubscribe()
    {
      GetOrCreateMailboxExistent(true);
    }

    private void GetOrCreateMailboxExistent(bool subscribe)
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" Trash\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = subscribe
          ? client.GetOrCreateMailbox("Trash", true)
          : client.GetOrCreateMailbox("Trash");

        StringAssert.EndsWith("LIST \"\" Trash\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("Trash", mailbox.Name);
        Assert.AreEqual("Trash", mailbox.FullName);
        Assert.IsFalse(mailbox.IsInbox);
      });
    }

    [Test]
    public void TestGetOrCreateMailboxNonExistent()
    {
      GetOrCreateMailboxNonExistent(false);
    }

    [Test]
    public void TestGetOrCreateMailboxNonExistentSubscribe()
    {
      GetOrCreateMailboxNonExistent(true);
    }

    private void GetOrCreateMailboxNonExistent(bool subscribe)
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // CREATE
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        if (subscribe)
          // SUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" Trash\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = subscribe
          ? client.GetOrCreateMailbox("Trash", true)
          : client.GetOrCreateMailbox("Trash");

        StringAssert.EndsWith("LIST \"\" Trash\r\n", server.DequeueRequest());
        StringAssert.EndsWith("CREATE Trash\r\n", server.DequeueRequest());
        if (subscribe)
          StringAssert.EndsWith("SUBSCRIBE Trash\r\n", server.DequeueRequest());
        StringAssert.EndsWith("LIST \"\" Trash\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("Trash", mailbox.Name);
        Assert.AreEqual("Trash", mailbox.FullName);
        Assert.IsFalse(mailbox.IsInbox);
      });
    }

    [Test]
    public void TestGetOrCreateMailboxNameContainsWildcard()
    {
      GetOrCreateMailboxNameContainsWildcard(false);
    }

    [Test]
    public void TestGetOrCreateMailboxNameContainsWildcardSubscribe()
    {
      GetOrCreateMailboxNameContainsWildcard(true);
    }

    private void GetOrCreateMailboxNameContainsWildcard(bool subscribe)
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"/\" Box*Box/Child\r\n" +
                                     "$tag OK done\r\n");
        // CREATE
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        if (subscribe)
          // SUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"/\" Box*/Child\r\n" +
                                     "* LIST () \"/\" Box*Box/Child\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = subscribe
          ? client.GetOrCreateMailbox("Box*/Child", true)
          : client.GetOrCreateMailbox("Box*/Child");

        StringAssert.EndsWith("LIST \"\" Box*/Child\r\n", server.DequeueRequest());
        StringAssert.EndsWith("CREATE \"Box*/Child\"\r\n", server.DequeueRequest());
        if (subscribe)
          StringAssert.EndsWith("SUBSCRIBE \"Box*/Child\"\r\n", server.DequeueRequest());
        StringAssert.EndsWith("LIST \"\" Box*/Child\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("Child", mailbox.Name);
        Assert.AreEqual("Box*/Child", mailbox.FullName);
        Assert.IsFalse(mailbox.IsInbox);
      });
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestGetOrCreateMailboxNameNull()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        client.GetOrCreateMailbox((string)null);
      });
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestGetOrCreateMailboxNameEmpty()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        client.GetOrCreateMailbox(string.Empty);
      });
    }

    [Test]
    public void TestGetMailboxes()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        var mailboxes = client.GetMailboxes();

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" Trash\r\n" +
                                     "* LIST () \".\" INBOX\r\n" +
                                     "* LIST () \".\" INBOX.Child1\r\n" +
                                     "* LIST () \".\" INBOX.Child2\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Trash", enumerator.Current.Name);
        Assert.AreEqual("Trash", enumerator.Current.FullName);
        Assert.IsFalse(enumerator.Current.IsInbox);

        StringAssert.EndsWith("LIST \"\" *\r\n", server.DequeueRequest());

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("INBOX", enumerator.Current.Name);
        Assert.AreEqual("INBOX", enumerator.Current.FullName);
        Assert.IsTrue(enumerator.Current.IsInbox);

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Child1", enumerator.Current.Name);
        Assert.AreEqual("INBOX.Child1", enumerator.Current.FullName);
        Assert.IsFalse(enumerator.Current.IsInbox);

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Child2", enumerator.Current.Name);
        Assert.AreEqual("INBOX.Child2", enumerator.Current.FullName);
        Assert.IsFalse(enumerator.Current.IsInbox);

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestGetMailboxesNothingMatched()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        var mailboxes = client.GetMailboxes();

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LIST
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestGetMailboxesTopLevelOnly()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        var mailboxes = client.GetMailboxes(ImapMailboxListOptions.TopLevelOnly);

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" Trash\r\n" +
                                     "* LIST () \".\" INBOX\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Trash", enumerator.Current.Name);
        Assert.AreEqual("Trash", enumerator.Current.FullName);

        StringAssert.EndsWith("LIST \"\" %\r\n", server.DequeueRequest());

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("INBOX", enumerator.Current.Name);
        Assert.AreEqual("INBOX", enumerator.Current.FullName);

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestGetMailboxesSubscribedOnly()
    {
      GetMailboxesSubscribedOnly(false);
    }

    [Test]
    public void TestGetMailboxesSubscribedOnlyMailboxReferralsCapable()
    {
      GetMailboxesSubscribedOnly(true);
    }

    private void GetMailboxesSubscribedOnly(bool mailboxReferralsCapable)
    {
      var capas = mailboxReferralsCapable
        ? new ImapCapability[] {ImapCapability.MailboxReferrals}
        : new ImapCapability[] {};

      TestUtils.TestAuthenticated(capas, delegate(ImapPseudoServer server, ImapClient client) {
        var mailboxes = client.GetMailboxes(ImapMailboxListOptions.SubscribedOnly | ImapMailboxListOptions.Remote);

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LSUB/RLSUB
        server.EnqueueTaggedResponse("* LSUB () \".\" Trash\r\n" +
                                     "* LSUB () \".\" INBOX\r\n" +
                                     "* LSUB () \".\" INBOX.Child2\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Trash", enumerator.Current.Name);
        Assert.AreEqual("Trash", enumerator.Current.FullName);

        if (mailboxReferralsCapable)
          StringAssert.EndsWith("RLSUB \"\" *\r\n", server.DequeueRequest());
        else
          StringAssert.EndsWith("LSUB \"\" *\r\n", server.DequeueRequest());

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("INBOX", enumerator.Current.Name);
        Assert.AreEqual("INBOX", enumerator.Current.FullName);

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Child2", enumerator.Current.Name);
        Assert.AreEqual("INBOX.Child2", enumerator.Current.FullName);

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestGetMailboxesSubscribedOnlyListExtendedCapable()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.ListExtended}, delegate(ImapPseudoServer server, ImapClient client) {
        var mailboxes = client.GetMailboxes(ImapMailboxListOptions.SubscribedOnly);

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LIST
        server.EnqueueTaggedResponse("* LIST (\\Marked \\NoInferiors \\Subscribed) \"/\" \"inbox\"\r\n" +
                                     "* LIST (\\Subscribed) \"/\" \"Fruit/Banana\"\r\n" +
                                     "* LIST (\\Subscribed \\NonExistent) \"/\" \"Fruit/Peach\"\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("INBOX", enumerator.Current.Name);
        Assert.AreEqual("INBOX", enumerator.Current.FullName);
        Assert.IsFalse(enumerator.Current.CanHaveChild);

        StringAssert.Contains("LIST (SUBSCRIBED) \"\" *", server.DequeueRequest());

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Banana", enumerator.Current.Name);
        Assert.AreEqual("Fruit/Banana", enumerator.Current.FullName);
        Assert.IsTrue(enumerator.Current.Exists);
        Assert.IsTrue(enumerator.Current.CanHaveChild);

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Peach", enumerator.Current.Name);
        Assert.AreEqual("Fruit/Peach", enumerator.Current.FullName);
        Assert.IsFalse(enumerator.Current.Exists);
        Assert.IsTrue(enumerator.Current.CanHaveChild);

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestGetMailboxesRemoteListExtendedCapable()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.ListExtended}, delegate(ImapPseudoServer server, ImapClient client) {
        var mailboxes = client.GetMailboxes(ImapMailboxListOptions.Remote);

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LIST
        server.EnqueueTaggedResponse("* LIST () \"/\" \"Fruit/Banana\"\r\n" +
                                     "* LIST (\\Remote) \"/\" \"Bread\"\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Banana", enumerator.Current.Name);
        Assert.AreEqual("Fruit/Banana", enumerator.Current.FullName);

        StringAssert.Contains("LIST (REMOTE) \"\" *", server.DequeueRequest());

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Bread", enumerator.Current.Name);
        Assert.AreEqual("Bread", enumerator.Current.FullName);

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestGetMailboxesRemoteMailboxReferralsCapable()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.MailboxReferrals}, delegate(ImapPseudoServer server, ImapClient client) {
        var mailboxes = client.GetMailboxes(ImapMailboxListOptions.Remote);

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // RLIST
        server.EnqueueTaggedResponse("* LIST () \".\" Trash\r\n" +
                                     "* LIST () \".\" INBOX\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Trash", enumerator.Current.Name);
        Assert.AreEqual("Trash", enumerator.Current.FullName);

        StringAssert.EndsWith("RLIST \"\" *\r\n", server.DequeueRequest());

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("INBOX", enumerator.Current.Name);
        Assert.AreEqual("INBOX", enumerator.Current.FullName);

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestGetMailboxesRequestStatus()
    {
      GetMailboxesRequestStatus(false);
    }

    [Test]
    public void TestGetMailboxesRequestStatusCondStoreCapable()
    {
      GetMailboxesRequestStatus(true);
    }

    private void GetMailboxesRequestStatus(bool condStoreCapable)
    {
      var capas = condStoreCapable
        ? new ImapCapability[] {ImapCapability.CondStore}
        : new ImapCapability[] {};
      var expectedStatusDataItems = condStoreCapable
        ? "(MESSAGES RECENT UIDNEXT UIDVALIDITY UNSEEN HIGHESTMODSEQ)"
        : "(MESSAGES RECENT UIDNEXT UIDVALIDITY UNSEEN)";

      TestUtils.TestAuthenticated(capas, delegate(ImapPseudoServer server, ImapClient client) {
        var mailboxes = client.GetMailboxes(ImapMailboxListOptions.RequestStatus);

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" Trash\r\n" +
                                     "* LIST () \".\" INBOX\r\n" +
                                     "* LIST () \".\" INBOX.Child\r\n" +
                                     "$tag OK done\r\n");
        // STATUS
        server.EnqueueTaggedResponse("* STATUS Trash (MESSAGES 3 RECENT 1 UIDNEXT 4 UIDVALIDITY 1 UNSEEN 1 HIGHESTMODSEQ 1)\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Trash", enumerator.Current.Name);
        Assert.AreEqual("Trash", enumerator.Current.FullName);
        Assert.AreEqual(3L, enumerator.Current.ExistMessageCount);
        Assert.AreEqual(1L, enumerator.Current.RecentMessageCount);
        Assert.AreEqual(4L, enumerator.Current.NextUid);
        Assert.AreEqual(1L, enumerator.Current.UidValidity);
        if (condStoreCapable)
          Assert.AreEqual(1L, enumerator.Current.HighestModSeq);

        StringAssert.EndsWith("LIST \"\" *\r\n", server.DequeueRequest());
        StringAssert.EndsWith(string.Format("STATUS Trash {0}\r\n", expectedStatusDataItems), server.DequeueRequest());

        // STATUS
        server.EnqueueTaggedResponse("* STATUS INBOX ()\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("INBOX", enumerator.Current.Name);
        Assert.AreEqual("INBOX", enumerator.Current.FullName);

        StringAssert.EndsWith(string.Format("STATUS INBOX {0}\r\n", expectedStatusDataItems), server.DequeueRequest());

        // STATUS
        server.EnqueueTaggedResponse("* STATUS INBOX.Child ()\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Child", enumerator.Current.Name);
        Assert.AreEqual("INBOX.Child", enumerator.Current.FullName);

        StringAssert.EndsWith(string.Format("STATUS INBOX.Child {0}\r\n", expectedStatusDataItems), server.DequeueRequest());

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestGetMailboxesRequestStatusNothingMatched()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        var mailboxes = client.GetMailboxes(ImapMailboxListOptions.RequestStatus);

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LIST
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestGetMailboxesRequestStatusUnselectableMailbox()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        var mailboxes = client.GetMailboxes(ImapMailboxListOptions.RequestStatus);

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LIST
        server.EnqueueTaggedResponse("* LIST () \"/\" blurdybloop\r\n" +
                                     "* LIST (\\Noselect) \"/\" foo\r\n" +
                                     "* LIST () \"/\" foo/bar\r\n" +
                                     "$tag OK done\r\n");
        // STATUS
        server.EnqueueTaggedResponse("* STATUS blurdybloop ()\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("blurdybloop", enumerator.Current.Name);
        Assert.AreEqual("blurdybloop", enumerator.Current.FullName);

        StringAssert.EndsWith("LIST \"\" *\r\n", server.DequeueRequest());
        StringAssert.Contains("STATUS blurdybloop", server.DequeueRequest());

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("foo", enumerator.Current.Name);
        Assert.AreEqual("foo", enumerator.Current.FullName);
        Assert.IsTrue(enumerator.Current.IsUnselectable);

        // STATUS
        server.EnqueueTaggedResponse("* STATUS foo/bar ()\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("bar", enumerator.Current.Name);
        Assert.AreEqual("foo/bar", enumerator.Current.FullName);

        StringAssert.Contains("STATUS foo/bar", server.DequeueRequest());
      });
    }

    [Test]
    public void TestGetMailboxesRequestStatusSelectedMailbox()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        /*
         * OpenInbox
         */
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" INBOX\r\n" +
                                     "$tag OK done\r\n");
        // SELECT
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        Assert.IsNotNull(client.OpenInbox());
        Assert.IsNotNull(client.OpenedMailbox);

        server.DequeueRequest(); // LIST
        server.DequeueRequest(); // SELECT

        /*
         * GetMailboxes
         */
        var mailboxes = client.GetMailboxes(ImapMailboxListOptions.RequestStatus);

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" Trash\r\n" +
                                     "* LIST () \".\" INBOX\r\n" +
                                     "* LIST () \".\" INBOX.Child\r\n" +
                                     "$tag OK done\r\n");
        // STATUS
        server.EnqueueTaggedResponse("* STATUS Trash ()\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Trash", enumerator.Current.Name);
        Assert.AreEqual("Trash", enumerator.Current.FullName);

        StringAssert.EndsWith("LIST \"\" *\r\n", server.DequeueRequest());
        StringAssert.Contains("STATUS Trash", server.DequeueRequest());

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("INBOX", enumerator.Current.Name);
        Assert.AreEqual("INBOX", enumerator.Current.FullName);

        // STATUS
        server.EnqueueTaggedResponse("* STATUS INBOX.Child ()\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Child", enumerator.Current.Name);
        Assert.AreEqual("INBOX.Child", enumerator.Current.FullName);

        StringAssert.Contains("STATUS INBOX.Child", server.DequeueRequest());

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestGetMailboxesRequestStatusListStatusCapable()
    {
      GetMailboxesRequestStatusListStatusCapable(false);
    }

    [Test]
    public void TestGetMailboxesRequestStatusListStatusCapableCondStoreCapable()
    {
      GetMailboxesRequestStatusListStatusCapable(true);
    }

    private void GetMailboxesRequestStatusListStatusCapable(bool condStoreCapable)
    {
      var capas = condStoreCapable
        ? new ImapCapability[] {ImapCapability.ListExtended, ImapCapability.ListStatus, ImapCapability.CondStore}
        : new ImapCapability[] {ImapCapability.ListExtended, ImapCapability.ListStatus};
      var expectedStatusDataItems = condStoreCapable
        ? "(MESSAGES RECENT UIDNEXT UIDVALIDITY UNSEEN HIGHESTMODSEQ)"
        : "(MESSAGES RECENT UIDNEXT UIDVALIDITY UNSEEN)";

      TestUtils.TestAuthenticated(capas, delegate(ImapPseudoServer server, ImapClient client) {
        var mailboxes = client.GetMailboxes(ImapMailboxListOptions.RequestStatus);

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" Trash\r\n" +
                                     "* STATUS Trash (MESSAGES 3 RECENT 1 UIDNEXT 4 UIDVALIDITY 1 UNSEEN 1 HIGHESTMODSEQ 1)\r\n" +
                                     "* LIST () \".\" INBOX\r\n" +
                                     "* STATUS INBOX ()\r\n" +
                                     "* LIST () \".\" INBOX.Child\r\n" +
                                     "* STATUS INBOX.Child ()\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Trash", enumerator.Current.Name);
        Assert.AreEqual("Trash", enumerator.Current.FullName);
        Assert.AreEqual(3L, enumerator.Current.ExistMessageCount);
        Assert.AreEqual(1L, enumerator.Current.RecentMessageCount);
        Assert.AreEqual(4L, enumerator.Current.NextUid);
        Assert.AreEqual(1L, enumerator.Current.UidValidity);
        Assert.AreEqual(1L, enumerator.Current.UnseenMessageCount);
        if (condStoreCapable)
          Assert.AreEqual(1L, enumerator.Current.HighestModSeq);

        StringAssert.EndsWith(string.Format("LIST () \"\" * RETURN (CHILDREN STATUS {0})\r\n", expectedStatusDataItems),
                              server.DequeueRequest());

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("INBOX", enumerator.Current.Name);
        Assert.AreEqual("INBOX", enumerator.Current.FullName);

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Child", enumerator.Current.Name);
        Assert.AreEqual("INBOX.Child", enumerator.Current.FullName);

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestCreateMailbox()
    {
      CreateMailbox(false);
    }

    [Test]
    public void TestCreateMailboxSubscribe()
    {
      CreateMailbox(true);
    }

    private void CreateMailbox(bool subscribe)
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // CREATE
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        if (subscribe)
          // SUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" INBOX.Recent\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = subscribe
          ? client.CreateMailbox("INBOX.Recent", true)
          : client.CreateMailbox("INBOX.Recent");

        StringAssert.EndsWith("CREATE INBOX.Recent\r\n", server.DequeueRequest());
        if (subscribe)
          StringAssert.EndsWith("SUBSCRIBE INBOX.Recent\r\n", server.DequeueRequest());
        StringAssert.EndsWith("LIST \"\" INBOX.Recent\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("Recent", mailbox.Name);
        Assert.AreEqual("INBOX.Recent", mailbox.FullName);
      });
    }

    [Test]
    public void TestCreateMailboxNameContainsWildcard1()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // CREATE
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // LIST
        server.EnqueueTaggedResponse("* LIST (\\Marked) \".\" Box*\r\n" +
                                     "* LIST () \".\" Box\r\n" +
                                     "* LIST () \".\" Box.Child\r\n" +
                                     "* LIST () \".\" Boxes\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.CreateMailbox("Box*");

        StringAssert.EndsWith("CREATE \"Box*\"\r\n", server.DequeueRequest());
        StringAssert.EndsWith("LIST \"\" Box*\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("Box*", mailbox.Name);
        Assert.AreEqual("Box*", mailbox.FullName);
        Assert.AreEqual(1, mailbox.Flags.Count);
        CollectionAssert.Contains(mailbox.Flags, ImapMailboxFlag.Marked);
        Assert.IsFalse(mailbox.IsInbox);
      });
    }

    [Test]
    public void TestCreateMailboxNameContainsWildcard2()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // CREATE
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // LIST
        server.EnqueueTaggedResponse("* LIST (\\Marked) \".\" Box%\r\n" +
                                     "* LIST () \".\" Box\r\n" +
                                     "* LIST () \".\" Boxes\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.CreateMailbox("Box%");

        StringAssert.EndsWith("CREATE \"Box%\"\r\n", server.DequeueRequest());
        StringAssert.EndsWith("LIST \"\" Box%\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("Box%", mailbox.Name);
        Assert.AreEqual("Box%", mailbox.FullName);
        Assert.AreEqual(1, mailbox.Flags.Count);
        CollectionAssert.Contains(mailbox.Flags, ImapMailboxFlag.Marked);
        Assert.IsFalse(mailbox.IsInbox);
      });
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestCreateMailboxNameNull()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        client.CreateMailbox(null);
      });
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestCreateMailboxNameEmpty()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        client.CreateMailbox(string.Empty);
      });
    }

    [Test]
    public void TestCreateMailboxAlreadyExists()
    {
      CreateMailboxAlreadyExists(false);
    }

    [Test]
    public void TestCreateMailboxAlreadyExistsSubscribe()
    {
      CreateMailboxAlreadyExists(true);
    }

    private void CreateMailboxAlreadyExists(bool subscribe)
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // CREATE
        server.EnqueueTaggedResponse("$tag NO [ALREADYEXISTS] mailbox already exists\r\n");

        try {
          if (subscribe)
            client.CreateMailbox("INBOX.Recent", true);
          else
            client.CreateMailbox("INBOX.Recent");
        }
        catch (ImapErrorResponseException ex) {
          Assert.AreEqual(ImapResponseCode.AlreadyExists,
                          ex.Result.TaggedStatusResponse.ResponseText.Code);
        }

        StringAssert.EndsWith("CREATE INBOX.Recent\r\n", server.DequeueRequest());
      });
    }

    [Test]
    public void TestOpenInbox()
    {
      OpenInbox(false);
    }

    [Test]
    public void TestOpenInboxAsReadOnly()
    {
      OpenInbox(true);
    }

    private void OpenInbox(bool asReadOnly)
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" INBOX\r\n" +
                                     "$tag OK done\r\n");
        // SELECT/EXAMINE
        if (asReadOnly)
          server.EnqueueTaggedResponse("* OK [READ-ONLY]\r\n" +
                                       "$tag OK done\r\n");
        else
          server.EnqueueTaggedResponse("* OK [READ-WRITE]\r\n" +
                                       "$tag OK done\r\n");

        var mailbox = asReadOnly
          ? client.OpenInbox(true)
          : client.OpenInbox();

        StringAssert.EndsWith("LIST \"\" INBOX\r\n", server.DequeueRequest());

        if (asReadOnly)
          StringAssert.EndsWith("EXAMINE INBOX\r\n", server.DequeueRequest());
        else
          StringAssert.EndsWith("SELECT INBOX\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.IsNotNull(client.OpenedMailbox);
        Assert.AreSame(mailbox, client.OpenedMailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.IsTrue(mailbox.IsOpen);
        Assert.AreEqual("INBOX", mailbox.Name);
        Assert.AreEqual("INBOX", mailbox.FullName);

        if (asReadOnly)
          Assert.IsTrue(mailbox.IsReadOnly);
        else
          Assert.IsFalse(mailbox.IsReadOnly);
      });
    }

    [Test]
    public void TestOpenMailbox()
    {
      OpenMailbox(false);
    }

    [Test]
    public void TestOpenMailboxAsReadOnly()
    {
      OpenMailbox(true);
    }

    private void OpenMailbox(bool asReadOnly)
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" Draft\r\n" +
                                     "$tag OK done\r\n");
        // SELECT/EXAMINE
        if (asReadOnly)
          server.EnqueueTaggedResponse("* OK [READ-ONLY]\r\n" +
                                       "$tag OK done\r\n");
        else
          server.EnqueueTaggedResponse("* OK [READ-WRITE]\r\n" +
                                       "$tag OK done\r\n");

        var mailbox = asReadOnly
          ? client.OpenMailbox("Draft", true)
          : client.OpenMailbox("Draft");

        StringAssert.EndsWith("LIST \"\" Draft\r\n", server.DequeueRequest());

        if (asReadOnly)
          StringAssert.EndsWith("EXAMINE Draft\r\n", server.DequeueRequest());
        else
          StringAssert.EndsWith("SELECT Draft\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.IsNotNull(client.OpenedMailbox);
        Assert.AreSame(mailbox, client.OpenedMailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.IsTrue(mailbox.IsOpen);
        Assert.AreEqual("Draft", mailbox.Name);
        Assert.AreEqual("Draft", mailbox.FullName);
        Assert.IsFalse(mailbox.IsModSequencesAvailable);

        if (asReadOnly)
          Assert.IsTrue(mailbox.IsReadOnly);
        else
          Assert.IsFalse(mailbox.IsReadOnly);
      });
    }

    [Test]
    public void TestOpenMailboxCondStoreCapable()
    {
      OpenMailboxCondStore(false);
    }

    [Test]
    public void TestOpenMailboxCondStoreCapableAsReadOnly()
    {
      OpenMailboxCondStore(true);
    }

    private void OpenMailboxCondStore(bool asReadOnly)
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.CondStore}, delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" INBOX\r\n" +
                                     "$tag OK done\r\n");
        // SELECT/EXAMINE
        if (asReadOnly)
          server.EnqueueTaggedResponse("* OK [HIGHESTMODSEQ 1]\r\n" +
                                       "* OK [READ-ONLY]\r\n" +
                                       "$tag OK done\r\n");
        else
          server.EnqueueTaggedResponse("* OK [HIGHESTMODSEQ 1]\r\n" +
                                       "* OK [READ-WRITE]\r\n" +
                                       "$tag OK done\r\n");

        var mailbox = asReadOnly
          ? client.OpenMailbox("INBOX", true)
          : client.OpenMailbox("INBOX");

        StringAssert.EndsWith("LIST \"\" INBOX\r\n", server.DequeueRequest());

        if (asReadOnly)
          StringAssert.EndsWith("EXAMINE INBOX (CONDSTORE)\r\n", server.DequeueRequest());
        else
          StringAssert.EndsWith("SELECT INBOX (CONDSTORE)\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.IsNotNull(client.OpenedMailbox);
        Assert.AreSame(mailbox, client.OpenedMailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.IsTrue(mailbox.IsOpen);
        Assert.AreEqual("INBOX", mailbox.Name);
        Assert.AreEqual("INBOX", mailbox.FullName);
        Assert.IsTrue(mailbox.IsModSequencesAvailable);

        if (asReadOnly)
          Assert.IsTrue(mailbox.IsReadOnly);
        else
          Assert.IsFalse(mailbox.IsReadOnly);
      });
    }

    [Test]
    public void TestOpenMailboxCondStoreCapableNoModSeq()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.CondStore}, delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" INBOX\r\n" +
                                     "$tag OK done\r\n");
        // SELECT
        server.EnqueueTaggedResponse("* OK [NOMODSEQ]\r\n" +
                                     "* OK [READ-WRITE]\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.OpenMailbox("INBOX");

        StringAssert.EndsWith("LIST \"\" INBOX\r\n", server.DequeueRequest());
        StringAssert.EndsWith("SELECT INBOX (CONDSTORE)\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.IsNotNull(client.OpenedMailbox);
        Assert.AreSame(mailbox, client.OpenedMailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.IsTrue(mailbox.IsOpen);
        Assert.AreEqual("INBOX", mailbox.Name);
        Assert.AreEqual("INBOX", mailbox.FullName);
        Assert.IsFalse(mailbox.IsModSequencesAvailable);
        Assert.IsFalse(mailbox.IsReadOnly);
      });
    }

    [Test]
    public void TestOpenMailboxNonExistent()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.ListExtended}, delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST (\\Subscribed \\NonExistent) \"/\" \"INBOX/Child\"\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.GetMailbox("INBOX/Child", ImapMailboxListOptions.SubscribedOnly);

        StringAssert.Contains("LIST (SUBSCRIBED) \"\" INBOX/Child", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("Child", mailbox.Name);
        Assert.AreEqual("INBOX/Child", mailbox.FullName);
        Assert.IsFalse(mailbox.Exists);

        TestUtils.ExpectExceptionThrown<ImapProtocolViolationException>(delegate {
          client.OpenMailbox(mailbox);
        });
      });
    }

    [Test]
    public void TestOpenMailboxUnselectable()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST (\\Noselect \\HasChildren) \"/\" \"[Gmail]\"\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.GetMailbox("[Gmail]");

        StringAssert.EndsWith("LIST \"\" [Gmail]\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("[Gmail]", mailbox.Name);
        Assert.AreEqual("[Gmail]", mailbox.FullName);
        Assert.IsTrue(mailbox.IsUnselectable);

        TestUtils.ExpectExceptionThrown<ImapProtocolViolationException>(delegate {
          client.OpenMailbox(mailbox);
        });

        Assert.IsNull(client.OpenedMailbox);
      });
    }

    [Test]
    public void TestOpenMailboxDeleted()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"/\" \"INBOX/Child\"\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.GetMailbox("INBOX/Child");

        StringAssert.EndsWith("LIST \"\" INBOX/Child\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("Child", mailbox.Name);
        Assert.AreEqual("INBOX/Child", mailbox.FullName);
        Assert.IsTrue(mailbox.Exists);

        // DELETE
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        mailbox.Delete();

        TestUtils.ExpectExceptionThrown<ImapProtocolViolationException>(delegate {
          client.OpenMailbox(mailbox);
        });

        Assert.IsNull(client.OpenedMailbox);
      });
    }

    [Test]
    public void TestOpenMailboxDifferentSession()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server1, ImapClient client1) {
        // LIST
        server1.EnqueueTaggedResponse("* LIST () \"\" \"INBOX\"\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client1.GetMailbox("INBOX");

        StringAssert.EndsWith("LIST \"\" INBOX\r\n", server1.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client1);
        Assert.AreEqual("INBOX", mailbox.Name);
        Assert.AreEqual("INBOX", mailbox.FullName);
        Assert.IsTrue(mailbox.Exists);

        TestUtils.TestAuthenticated(delegate(ImapPseudoServer server2, ImapClient client2) {
          TestUtils.ExpectExceptionThrown<ArgumentException>(delegate {
            client2.OpenMailbox(mailbox);
          });

          Assert.IsNull(client1.OpenedMailbox);
          Assert.IsNull(client2.OpenedMailbox);
        });
      });
    }

    [Test]
    public void TestOpenMailboxNameContainsWildcard()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" Box*\r\n" +
                                     "* LIST () \".\" Box\r\n" +
                                     "* LIST () \".\" Box.Child\r\n" +
                                     "* LIST () \".\" Boxes\r\n" +
                                     "$tag OK done\r\n");
        // SELECT
        server.EnqueueTaggedResponse("* OK [READ-WRITE]\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.OpenMailbox("Box*");

        StringAssert.EndsWith("LIST \"\" Box*\r\n", server.DequeueRequest());
        StringAssert.EndsWith("SELECT \"Box*\"\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.IsNotNull(client.OpenedMailbox);
        Assert.AreSame(mailbox, client.OpenedMailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.IsTrue(mailbox.IsOpen);
        Assert.AreEqual("Box*", mailbox.Name);
        Assert.AreEqual("Box*", mailbox.FullName);
        Assert.IsFalse(mailbox.IsModSequencesAvailable);
        Assert.IsFalse(mailbox.IsReadOnly);
      });
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestOpenMailboxNameNull()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        client.OpenMailbox((string)null);
      });
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestOpenMailboxNameEmpty()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        client.OpenMailbox(string.Empty);
      });
    }

    [Test, Ignore("not implemented")]
    public void TestOpenMailboxReopenCurrentlySelected()
    {
    }

    [Test, Ignore("not implemented")]
    public void TestOpenMailboxOpenNewly()
    {
    }

    [Test]
    public void TestOpenMailboxError()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" INBOX\r\n" +
                                     "$tag OK done\r\n");
        // SELECT
        server.EnqueueTaggedResponse("$tag NO unselectable\r\n");

        Assert.IsNull(client.OpenedMailbox);

        TestUtils.ExpectExceptionThrown<ImapErrorResponseException>(delegate {
          client.OpenMailbox("INBOX");
        });

        StringAssert.EndsWith("LIST \"\" INBOX\r\n", server.DequeueRequest());
        StringAssert.EndsWith("SELECT INBOX\r\n", server.DequeueRequest());

        Assert.IsNull(client.OpenedMailbox);
      });
    }

    [Test]
    public void TestOpenOrCreateMailboxExistent()
    {
      OpenOrCreateMailbox(true, false);
    }

    [Test]
    public void TestOpenOrCreateMailboxExistentAsReadOnly()
    {
      OpenOrCreateMailbox(true, true);
    }

    [Test]
    public void TestOpenOrCreateMailboxNonExistent()
    {
      OpenOrCreateMailbox(false, false);
    }

    [Test]
    public void TestOpenOrCreateMailboxNonExistentAsReadOnly()
    {
      OpenOrCreateMailbox(false, true);
    }

    private void OpenOrCreateMailbox(bool exist, bool asReadOnly)
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        if (!exist) {
          // LIST
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // CREATE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // SUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
        }

        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" INBOX.child\r\n" +
                                     "$tag OK done\r\n");
        // SELECT/EXAMINE
        if (asReadOnly)
          server.EnqueueTaggedResponse("* OK [READ-ONLY]\r\n" +
                                       "$tag OK done\r\n");
        else
          server.EnqueueTaggedResponse("* OK [READ-WRITE]\r\n" +
                                       "$tag OK done\r\n");

        var mailbox = client.OpenOrCreateMailbox("INBOX.child",
                                                 true, // subscribe
                                                 asReadOnly,
                                                 ImapMailboxListOptions.Default);

        if (!exist) {
          StringAssert.EndsWith("LIST \"\" INBOX.child\r\n", server.DequeueRequest());
          StringAssert.EndsWith("CREATE INBOX.child\r\n", server.DequeueRequest());
          StringAssert.EndsWith("SUBSCRIBE INBOX.child\r\n", server.DequeueRequest());
        }

        StringAssert.EndsWith("LIST \"\" INBOX.child\r\n", server.DequeueRequest());

        if (asReadOnly)
          StringAssert.EndsWith("EXAMINE INBOX.child\r\n", server.DequeueRequest());
        else
          StringAssert.EndsWith("SELECT INBOX.child\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.IsNotNull(client.OpenedMailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.IsTrue(mailbox.IsOpen);
        Assert.AreEqual("child", mailbox.Name);
        Assert.AreEqual("INBOX.child", mailbox.FullName);
        Assert.IsFalse(mailbox.IsModSequencesAvailable);
        if (asReadOnly)
          Assert.IsTrue(mailbox.IsReadOnly);
        else
          Assert.IsFalse(mailbox.IsReadOnly);
      });
    }

    [Test]
    public void TestOpenOrCreateMailboxNameContainsWildcard()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" Box\r\n" +
                                     "* LIST () \".\" Box.Child\r\n" +
                                     "* LIST () \".\" Boxes\r\n" +
                                     "$tag OK done\r\n");
        // CREATE
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" Box*\r\n" +
                                     "* LIST () \".\" Box\r\n" +
                                     "* LIST () \".\" Box.Child\r\n" +
                                     "* LIST () \".\" Boxes\r\n" +
                                     "$tag OK done\r\n");
        // SELECT
        server.EnqueueTaggedResponse("* OK [READ-WRITE]\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.OpenOrCreateMailbox("Box*");

        StringAssert.EndsWith("LIST \"\" Box*\r\n", server.DequeueRequest());
        StringAssert.EndsWith("CREATE \"Box*\"\r\n", server.DequeueRequest());
        StringAssert.EndsWith("LIST \"\" Box*\r\n", server.DequeueRequest());
        StringAssert.EndsWith("SELECT \"Box*\"\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.IsNotNull(client.OpenedMailbox);
        Assert.AreSame(mailbox, client.OpenedMailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.IsTrue(mailbox.IsOpen);
        Assert.AreEqual("Box*", mailbox.Name);
        Assert.AreEqual("Box*", mailbox.FullName);
        Assert.IsFalse(mailbox.IsModSequencesAvailable);
        Assert.IsFalse(mailbox.IsReadOnly);
      });
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestOpenOrCreateMailboxNameNull()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        client.OpenOrCreateMailbox((string)null);
      });
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestOpenOrCreateMailboxNameEmpty()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        client.OpenOrCreateMailbox(string.Empty);
      });
    }

    [Test]
    public void TestCloseMailbox()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" INBOX\r\n" +
                                     "$tag OK done\r\n");
        // SELECT
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        var mailbox = client.OpenInbox();

        StringAssert.EndsWith("LIST \"\" INBOX\r\n", server.DequeueRequest());
        StringAssert.EndsWith("SELECT INBOX\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.IsNotNull(client.OpenedMailbox);
        Assert.AreSame(mailbox, client.OpenedMailbox);
        Assert.IsTrue(mailbox.IsOpen);
        Assert.AreEqual("INBOX", mailbox.Name);
        Assert.AreEqual("INBOX", mailbox.FullName);

        // CLOSE
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        client.CloseMailbox();

        Assert.IsNull(client.OpenedMailbox);
        Assert.IsFalse(mailbox.IsOpen);
      });
    }

    [Test]
    public void TestCloseMailboxNotOpened()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        Assert.IsNull(client.OpenedMailbox);

        client.CloseMailbox();

        Assert.IsNull(client.OpenedMailbox);

        client.CloseMailbox();
      });
    }

    [Test]
    public void TestGetQuota()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.Quota}, delegate(ImapPseudoServer server, ImapClient client) {
        // GETQUOTA
        server.EnqueueTaggedResponse("* QUOTA \"\" (STORAGE 10 512)\r\n" +
                                     "$tag OK done\r\n");

        var quota = client.GetQuota(string.Empty);

        Assert.That(server.DequeueRequest(), Text.EndsWith("GETQUOTA \"\"\r\n"));

        Assert.IsNotNull(quota);
        Assert.AreEqual(string.Empty, quota.Root);
        Assert.AreEqual(1, quota.Resources.Length);
        Assert.AreEqual("STORAGE", quota.Resources[0].Name);
        Assert.AreEqual(10, quota.Resources[0].Usage);
        Assert.AreEqual(512, quota.Resources[0].Limit);
      });
    }

    [Test]
    public void TestGetQuotaQuotaIncapable()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        var quota = client.GetQuota(string.Empty);

        Assert.IsNull(quota);
      });
    }

    [Test]
    public void TestGetQuotaUsage()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.Quota}, delegate(ImapPseudoServer server, ImapClient client) {
        // GETQUOTA
        server.EnqueueTaggedResponse("* QUOTA \"\" (STORAGE 10 512)\r\n" +
                                     "$tag OK done\r\n");

        var usage = client.GetQuotaUsage(string.Empty, "STORAGE");

        Assert.That(server.DequeueRequest(), Text.EndsWith("GETQUOTA \"\"\r\n"));

        Assert.AreEqual(10.0 / 512.0, usage);
      });
    }

    [Test, ExpectedException(typeof(ImapErrorResponseException))]
    public void TestGetQuotaUsageNoSuchQuotaRoot()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.Quota}, delegate(ImapPseudoServer server, ImapClient client) {
        // GETQUOTA
        server.EnqueueTaggedResponse("$tag NO no such quota root\r\n");

        client.GetQuotaUsage("NON-EXISTENT", "STORAGE");
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestGetQuotaUsageNoSuchResourceName()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.Quota}, delegate(ImapPseudoServer server, ImapClient client) {
        // GETQUOTA
        server.EnqueueTaggedResponse("* QUOTA \"\" (STORAGE 10 512)\r\n" +
                                     "$tag OK done\r\n");

        client.GetQuotaUsage(string.Empty, "NON-EXITENT");
      });
    }

    [Test]
    public void TestGetQuotaUsageQuotaIncapable()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        var usage = client.GetQuotaUsage(string.Empty, "STORAGE");

        Assert.AreEqual(0.0, usage);
      });
    }

    [Test]
    public void TestRefresh()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // NOOP
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        client.Refresh();

        StringAssert.EndsWith("NOOP\r\n", server.DequeueRequest());
      });
    }

    [Test]
    public void TestRefreshMailboxOpened()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" INBOX\r\n" +
                                     "$tag OK done\r\n");
        server.EnqueueTaggedResponse("* EXISTS 10\r\n" +
                                     "* RECENT 3\r\n" +
                                     "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)\r\n" +
                                     "* OK [READ-WRITE]\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.OpenInbox();

        StringAssert.EndsWith("LIST \"\" INBOX\r\n", server.DequeueRequest());
        StringAssert.EndsWith("SELECT INBOX\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.IsNotNull(client.OpenedMailbox);
        Assert.AreSame(mailbox, client.OpenedMailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.IsTrue(mailbox.IsOpen);
        Assert.AreEqual("INBOX", mailbox.Name);
        Assert.AreEqual("INBOX", mailbox.FullName);
        Assert.IsFalse(mailbox.IsReadOnly);
        Assert.AreEqual(10L, mailbox.ExistMessageCount);
        Assert.AreEqual(3L, mailbox.RecentMessageCount);
        Assert.IsNotNull(mailbox.ApplicableFlags);
        Assert.AreEqual(5, mailbox.ApplicableFlags.Count);
        Assert.IsTrue(mailbox.ApplicableFlags.Contains(ImapMessageFlag.Answered));
        Assert.IsTrue(mailbox.ApplicableFlags.Contains(ImapMessageFlag.Flagged));
        Assert.IsTrue(mailbox.ApplicableFlags.Contains(ImapMessageFlag.Deleted));
        Assert.IsTrue(mailbox.ApplicableFlags.Contains(ImapMessageFlag.Seen));
        Assert.IsTrue(mailbox.ApplicableFlags.Contains(ImapMessageFlag.Draft));

        // FETCH
        server.EnqueueTaggedResponse("* FETCH 3 (UID 3 FLAGS ())\r\n" +
                                     "$tag OK done\r\n");

        var message = mailbox.GetMessageByUid(3L, ImapMessageFetchAttributeOptions.DynamicAttributes);

        StringAssert.EndsWith("UID FETCH 3 (UID FLAGS)\r\n", server.DequeueRequest());

        Assert.AreEqual(3L, message.Sequence);
        Assert.AreEqual(3L, message.Uid);
        Assert.IsNotNull(message.Flags);
        Assert.AreEqual(0, message.Flags.Count);
        Assert.IsFalse(message.IsSeen);

        // NOOP
        server.EnqueueTaggedResponse("* EXPUNGE 1\r\n" +
                                     "* EXPUNGE 5\r\n" +
                                     "* EXISTS 8\r\n" +
                                     "* RECENT 4\r\n" +
                                     "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft $label1)\r\n" +
                                     "* FETCH 2 (FLAGS (\\Seen \\Deleted $label1))\r\n" +
                                     "$tag OK done\r\n");

        client.Refresh();

        StringAssert.EndsWith("NOOP\r\n", server.DequeueRequest());

        Assert.AreEqual(8L, mailbox.ExistMessageCount);
        Assert.AreEqual(4L, mailbox.RecentMessageCount);
        Assert.IsNotNull(mailbox.ApplicableFlags);
        Assert.AreEqual(6, mailbox.ApplicableFlags.Count);
        Assert.IsTrue(mailbox.ApplicableFlags.Contains(ImapMessageFlag.Answered));
        Assert.IsTrue(mailbox.ApplicableFlags.Contains(ImapMessageFlag.Flagged));
        Assert.IsTrue(mailbox.ApplicableFlags.Contains(ImapMessageFlag.Deleted));
        Assert.IsTrue(mailbox.ApplicableFlags.Contains(ImapMessageFlag.Seen));
        Assert.IsTrue(mailbox.ApplicableFlags.Contains(ImapMessageFlag.Draft));
        Assert.IsTrue(mailbox.ApplicableFlags.Contains("$label1"));

        Assert.AreEqual(2L, message.Sequence);
        Assert.AreEqual(3L, message.Uid);
        Assert.IsNotNull(message.Flags);
        Assert.AreEqual(3, message.Flags.Count);
        Assert.IsTrue(message.Flags.Contains(ImapMessageFlag.Seen));
        Assert.IsTrue(message.Flags.Contains(ImapMessageFlag.Deleted));
        Assert.IsTrue(message.Flags.Contains("$label1"));
        Assert.IsTrue(message.IsSeen);
        Assert.IsTrue(message.IsMarkedAsDeleted);
      });
    }

    [Test, ExpectedException(typeof(Smdn.Net.Imap4.Protocol.ImapConnectionException))]
    public void TestRefreshBye()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        server.EnqueueResponse("* BYE\r\n");
        server.Stop();

        client.Refresh();
      });
    }

    [Test, ExpectedException(typeof(Smdn.Net.Imap4.Protocol.ImapConnectionException))]
    public void TestRefreshDisconnected()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        server.Stop();

        client.Refresh();
      });
    }

    [Test]
    public void TestEventExistMessageCountChanged()
    {
      var selectResp =
        "* EXISTS 10\r\n" +
        "* RECENT 3\r\n" +
        "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)\r\n" +
        "* OK [READ-WRITE]\r\n" +
        "$tag OK done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.AreEqual(10L, mailbox.ExistMessageCount);

        var eventRaised = false;

        mailbox.Client.ExistMessageCountChanged += delegate(object sender, ImapMailboxSizeChangedEventArgs e) {
          Assert.AreSame(mailbox.Client, sender);
          Assert.AreEqual(mailbox, e.Mailbox);
          Assert.AreEqual(10L, e.PrevCount);
          Assert.AreEqual(12L, e.CurrentCount);

          eventRaised = true;
        };

        // NOOP
        server.EnqueueTaggedResponse("* EXISTS 12\r\n" +
                                     "$tag OK done\r\n");

        mailbox.Client.Refresh();

        StringAssert.EndsWith("NOOP\r\n", server.DequeueRequest());

        Assert.IsTrue(eventRaised);
        Assert.AreEqual(12L, mailbox.ExistMessageCount);
      });
    }

    [Test]
    public void TestEventRecentMessageCountChanged()
    {
      var selectResp =
        "* EXISTS 10\r\n" +
        "* RECENT 3\r\n" +
        "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)\r\n" +
        "* OK [READ-WRITE]\r\n" +
        "$tag OK done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.AreEqual(3L, mailbox.RecentMessageCount);

        var eventRaised = false;

        mailbox.Client.RecentMessageCountChanged += delegate(object sender, ImapMailboxSizeChangedEventArgs e) {
          Assert.AreSame(mailbox.Client, sender);
          Assert.AreEqual(mailbox, e.Mailbox);
          Assert.AreEqual(3L, e.PrevCount);
          Assert.AreEqual(4L, e.CurrentCount);

          eventRaised = true;
        };

        // NOOP
        server.EnqueueTaggedResponse("* RECENT 4\r\n" +
                                     "$tag OK done\r\n");

        mailbox.Client.Refresh();

        StringAssert.EndsWith("NOOP\r\n", server.DequeueRequest());

        Assert.IsTrue(eventRaised);
        Assert.AreEqual(4L, mailbox.RecentMessageCount);
      });
    }

    [Test]
    public void TestEventMessageStatusChanged()
    {
      var selectResp =
        "* EXISTS 3\r\n" +
        "* OK [READ-WRITE]\r\n" +
        "$tag OK done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.AreEqual(3L, mailbox.ExistMessageCount);

        // NOOP
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // FETCH
        server.EnqueueTaggedResponse("* FETCH 1 (UID 1 FLAGS (\\Seen))\r\n" +
                                     "* FETCH 2 (UID 2 FLAGS (\\Seen))\r\n" +
                                     "* FETCH 3 (UID 3 FLAGS (\\Seen))\r\n" +
                                     "$tag OK done\r\n");

        var messages = mailbox.GetMessages(ImapMessageFetchAttributeOptions.DynamicAttributes).ToArray();

        StringAssert.EndsWith("NOOP\r\n", server.DequeueRequest());
        StringAssert.EndsWith("FETCH 1:3 (UID FLAGS)\r\n", server.DequeueRequest());

        Assert.IsTrue (messages[0].IsSeen);
        Assert.IsFalse(messages[0].IsMarkedAsDeleted);
        Assert.IsTrue (messages[1].IsSeen);
        Assert.IsFalse(messages[1].IsMarkedAsDeleted);
        Assert.IsTrue (messages[2].IsSeen);
        Assert.IsFalse(messages[2].IsMarkedAsDeleted);

        var eventRaised = false;

        mailbox.Client.MessageStatusChanged += delegate(object sender, ImapMessageStatusChangedEventArgs e) {
          Assert.AreSame(mailbox.Client, sender);
          Assert.AreEqual(2, e.Messages.Length);

          Assert.AreEqual(1L, e.Messages[0].Sequence);
          Assert.IsTrue(e.Messages[0].IsSeen);
          Assert.IsTrue(e.Messages[0].IsMarkedAsDeleted);

          Assert.AreEqual(3L, e.Messages[1].Sequence);
          Assert.IsFalse(e.Messages[1].IsSeen);
          Assert.IsFalse(e.Messages[1].IsMarkedAsDeleted);

          eventRaised = true;
        };

        // NOOP
        server.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Seen \\Deleted))\r\n" +
                                     "* FETCH 3 (FLAGS ())\r\n" +
                                     "$tag OK done\r\n");

        mailbox.Client.Refresh();

        StringAssert.EndsWith("NOOP\r\n", server.DequeueRequest());

        Assert.IsTrue(eventRaised);

        Assert.IsTrue (messages[0].IsSeen);
        Assert.IsTrue (messages[0].IsMarkedAsDeleted);
        Assert.IsTrue (messages[1].IsSeen);
        Assert.IsFalse(messages[1].IsMarkedAsDeleted);
        Assert.IsFalse(messages[2].IsSeen);
        Assert.IsFalse(messages[2].IsMarkedAsDeleted);
      });
    }

    [Test]
    public void TestEventMessageDeleted()
    {
      var selectResp =
        "* EXISTS 3\r\n" +
        "* OK [READ-WRITE]\r\n" +
        "$tag OK done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.AreEqual(3L, mailbox.ExistMessageCount);

        // NOOP
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // FETCH
        server.EnqueueTaggedResponse("* FETCH 1 (UID 1 FLAGS (\\Seen))\r\n" +
                                     "* FETCH 2 (UID 2 FLAGS (\\Seen))\r\n" +
                                     "* FETCH 3 (UID 3 FLAGS (\\Seen))\r\n" +
                                     "$tag OK done\r\n");

        var messages = mailbox.GetMessages(ImapMessageFetchAttributeOptions.DynamicAttributes).ToArray();

        StringAssert.EndsWith("NOOP\r\n", server.DequeueRequest());
        StringAssert.EndsWith("FETCH 1:3 (UID FLAGS)\r\n", server.DequeueRequest());

        Assert.AreEqual(1L, messages[0].Sequence);
        Assert.AreEqual(1L, messages[0].Uid);
        Assert.IsFalse(messages[0].IsDeleted);
        Assert.AreEqual(2L, messages[1].Sequence);
        Assert.AreEqual(2L, messages[1].Uid);
        Assert.IsFalse(messages[0].IsDeleted);
        Assert.AreEqual(3L, messages[2].Sequence);
        Assert.AreEqual(3L, messages[2].Uid);
        Assert.IsFalse(messages[0].IsDeleted);

        var eventRaised = false;

        mailbox.Client.MessageDeleted += delegate(object sender, ImapMessageStatusChangedEventArgs e) {
          Assert.AreSame(mailbox.Client, sender);
          Assert.AreEqual(2, e.Messages.Length);

          Assert.AreEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, e.Messages[0].Sequence);
          Assert.AreEqual(3L, e.Messages[0].Uid);
          Assert.IsTrue(e.Messages[0].IsDeleted);

          Assert.AreEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, e.Messages[1].Sequence);
          Assert.AreEqual(1L, e.Messages[1].Uid);
          Assert.IsTrue(e.Messages[1].IsDeleted);

          eventRaised = true;
        };

        // NOOP
        server.EnqueueTaggedResponse("* EXPUNGE 3\r\n" +
                                     "* EXPUNGE 1\r\n" +
                                     "$tag OK done\r\n");

        mailbox.Client.Refresh();

        StringAssert.EndsWith("NOOP\r\n", server.DequeueRequest());

        Assert.IsTrue(eventRaised);

        Assert.AreEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, messages[0].Sequence);
        Assert.AreEqual(1L, messages[0].Uid);
        Assert.IsTrue(messages[0].IsDeleted);
        Assert.AreEqual(1L, messages[1].Sequence);
        Assert.AreEqual(2L, messages[1].Uid);
        Assert.IsFalse(messages[1].IsDeleted);
        Assert.AreEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, messages[2].Sequence);
        Assert.AreEqual(3L, messages[2].Uid);
        Assert.IsTrue(messages[2].IsDeleted);
      });
    }

    [Test]
    public void TestEventAlertReceivedUntaggedOkResponse()
    {
      EventAlertReceived("* OK [ALERT] operation successed\r\n" +
                         "$tag OK done\r\n",
                         "operation successed",
                         ImapResponseCondition.Ok);
    }

    [Test]
    public void TestEventAlertReceivedUntaggedNoResponse()
    {
      EventAlertReceived("* NO [ALERT] operation failed\r\n" +
                         "$tag OK done\r\n",
                         "operation failed",
                         ImapResponseCondition.No);
    }

    [Test]
    public void TestEventAlertReceivedTaggedOkResponse()
    {
      EventAlertReceived("$tag OK [ALERT] done\r\n",
                         "done",
                         ImapResponseCondition.Ok);
    }

    private void EventAlertReceived(string noopResponse,
                                    string expectedAlert,
                                    ImapResponseCondition expectedCondition)
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        var eventRaised = false;

        client.AlertReceived += delegate(object sender, ImapAlertReceivedEventArgs e) {
          Assert.AreSame(client, sender);
          Assert.AreEqual(expectedAlert, e.Alert);
          Assert.AreEqual(expectedCondition, e.Condition);

          eventRaised = true;
        };

        // NOOP
        server.EnqueueTaggedResponse(noopResponse);

        client.Refresh();

        StringAssert.EndsWith("NOOP\r\n", server.DequeueRequest());

        Assert.IsTrue(eventRaised);
      });
    }
  }
}
