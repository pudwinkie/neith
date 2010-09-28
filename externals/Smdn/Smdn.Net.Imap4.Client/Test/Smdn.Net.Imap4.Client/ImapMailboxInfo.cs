using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif

namespace Smdn.Net.Imap4.Client {
  [TestFixture]
  public class ImapMailboxInfoTests {
    private void TestMailbox(string name, Action<ImapPseudoServer, ImapMailboxInfo> action)
    {
      TestMailbox(null, null, ".", name, action);
    }

    private void TestMailbox(string hierarchyDelimiter, string name, Action<ImapPseudoServer, ImapMailboxInfo> action)
    {
      TestMailbox(null, null, hierarchyDelimiter, name, action);
    }

    private void TestMailbox(ImapCapability[] capabilities,
                             ImapMailboxFlag[] flags,
                             string name,
                             Action<ImapPseudoServer, ImapMailboxInfo> action)
    {
      TestMailbox(capabilities, flags, ".", name, action);
    }

    private void TestMailbox(ImapCapability[] capabilities,
                             ImapMailboxFlag[] flags,
                             string hierarchyDelimiter,
                             string name,
                             Action<ImapPseudoServer, ImapMailboxInfo> action)
    {
      capabilities = capabilities ?? new ImapCapability[0];

      TestUtils.TestAuthenticated(capabilities, delegate(ImapPseudoServer server, ImapClient client) {
        var joinedFlags = string.Join(" ", Array.ConvertAll(flags ?? new ImapMailboxFlag[0], delegate(ImapMailboxFlag f) {
          return f.ToString();
        }).ToArray());

        // LIST
        server.EnqueueTaggedResponse(string.Format("* LIST ({0}) \"{1}\" {2}\r\n", joinedFlags, hierarchyDelimiter, name) +
                                     "$tag OK done\r\n");

        var mailbox = client.GetMailbox(name);

        Assert.IsNotNull(mailbox);
        Assert.AreEqual(name, mailbox.FullName);
        Assert.AreEqual(hierarchyDelimiter, mailbox.MailboxSeparator);

        var requested = server.DequeueRequest();

        if (Array.Exists(capabilities, delegate(ImapCapability capa) { return capa == ImapCapability.ListExtended; })) {
          Assert.That(requested, Text.Contains("LIST ("));
          Assert.That(requested, Text.Contains(string.Format(") \"\" \"{0}\"", name)));
        }
        else {
          Assert.That(requested, Text.EndsWith(string.Format("LIST \"\" \"{0}\"\r\n", name)));
        }

        action(server, mailbox);
      });
    }

    [Test]
    public void TestRefresh()
    {
      TestMailbox(null,
                  new[] {ImapMailboxFlag.NoInferiors},
                  "INBOX",
                  delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        // STATUS
        server.EnqueueTaggedResponse("* STATUS INBOX ()\r\n" +
                                     "$tag OK done\r\n");

        mailbox.Refresh();

        var requested = server.DequeueRequest();

        Assert.That(requested, Text.Contains("STATUS \"INBOX\" ("));
        Assert.That(requested, Text.DoesNotContain("HIGHESTMODSEQ"));
      });
    }

    [Test]
    public void TestRefreshCondStoreCapable()
    {
      TestMailbox(new[] {ImapCapability.CondStore},
                  new[] {ImapMailboxFlag.Marked},
                  "INBOX",
                  delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        // STATUS
        server.EnqueueTaggedResponse("* STATUS INBOX ()\r\n" +
                                     "$tag OK done\r\n");

        mailbox.Refresh();

        var requested = server.DequeueRequest();

        Assert.That(requested, Text.Contains("STATUS \"INBOX\" ("));
        Assert.That(requested, Text.Contains("HIGHESTMODSEQ"));
      });
    }

    [Test]
    public void TestRefreshNonExistentMailbox()
    {
      TestMailbox(null,
                  new[] {ImapMailboxFlag.NonExistent},
                  "INBOX",
                  delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        Assert.IsFalse(mailbox.Exists);

        // do nothing
        mailbox.Refresh();
      });
    }

    [Test]
    public void TestRefreshUnselectableMailbox()
    {
      TestMailbox(null,
                  new[] {ImapMailboxFlag.NoSelect},
                  "INBOX",
                  delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        Assert.IsTrue(mailbox.IsUnselectable);

        // do nothing
        mailbox.Refresh();
      });
    }

    [Test]
    public void TestOpen()
    {
      Open(false);
    }

    [Test]
    public void TestOpenReadOnly()
    {
      Open(true);
    }

    private void Open(bool readOnly)
    {
      TestMailbox("INBOX", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        // SELECT/EXAMINE
        server.EnqueueTaggedResponse("* 172 EXISTS\r\n" +
                                     "* 1 RECENT\r\n" +
                                     "* OK [UNSEEN 12] Message 12 is first unseen\r\n" +
                                     "* OK [UIDVALIDITY 3857529045] UIDs valid\r\n" +
                                     "* OK [UIDNEXT 4392] Predicted next UID\r\n" +
                                     "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)\r\n" +
                                     "* OK [PERMANENTFLAGS (\\Deleted \\Seen \\*)] Limited\r\n" +
                                     string.Format("$tag OK [{0}] done\r\n", readOnly ? "READ-ONLY" : "READ-WRITE"));

        Assert.IsFalse(mailbox.IsOpen);

        using (var opened = readOnly ? mailbox.Open(true) : mailbox.Open()) {
          try {
            if (readOnly)
              Assert.That(server.DequeueRequest(), Text.EndsWith("EXAMINE \"INBOX\"\r\n"));
            else
              Assert.That(server.DequeueRequest(), Text.EndsWith("SELECT \"INBOX\"\r\n"));

            Assert.IsNotNull(opened);
            Assert.IsTrue(opened.IsOpen);

            Assert.AreEqual(172L, opened.ExistMessageCount);
            Assert.AreEqual(1L, opened.RecentMessageCount);
            Assert.AreEqual(12L, opened.FirstUnseenMessageNumber);
            Assert.AreEqual(4392L, opened.NextUid);

            Assert.IsTrue(mailbox.IsOpen);
            Assert.AreEqual(opened.ExistMessageCount, mailbox.ExistMessageCount);
            Assert.AreEqual(opened.UnseenMessageCount, mailbox.UnseenMessageCount);
            Assert.AreEqual(opened.NextUid, mailbox.NextUid);

            Assert.IsNotNull(opened.ApplicableFlags);
            Assert.AreEqual(5, opened.ApplicableFlags.Count);

            Assert.IsNotNull(opened.PermanentFlags);
            Assert.AreEqual(3, opened.PermanentFlags.Count);

            Assert.IsTrue(opened.IsAllowedToCreateKeywords);
            Assert.IsTrue(opened.IsUidPersistent);

            if (readOnly)
              Assert.IsTrue(opened.IsReadOnly);
            else
              Assert.IsFalse(opened.IsReadOnly);
          }
          finally {
            // CLOSE
            server.EnqueueTaggedResponse("$tag OK done\r\n");
          }
        }

        Assert.That(server.DequeueRequest(), Text.EndsWith("CLOSE\r\n"));

        Assert.IsFalse(mailbox.IsOpen);
      });
    }

    [Test, Ignore("not implemented")]
    public void TestOpenReopenItself()
    {
    }

    [Test, Ignore("not implemented")]
    public void TestOpenOpenAnotherNewly()
    {
    }

    [Test]
    public void TestDelete()
    {
      TestMailbox("Trash", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        // DELETE
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        Assert.IsTrue(mailbox.Exists);

        mailbox.Delete();

        Assert.IsFalse(mailbox.Exists);

        Assert.That(server.DequeueRequest(), Text.EndsWith("DELETE \"Trash\"\r\n"));
      });
    }

    [Test]
    public void TestDeleteUnsubscribe()
    {
      TestMailbox("Trash", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        // DELETE
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // UNSUBSCRIBE
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        Assert.IsTrue(mailbox.Exists);

        mailbox.Delete(true);

        Assert.IsFalse(mailbox.Exists);

        Assert.That(server.DequeueRequest(), Text.EndsWith("DELETE \"Trash\"\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("UNSUBSCRIBE \"Trash\"\r\n"));
      });
    }

    [Test]
    public void TestDeleteOpened()
    {
      TestMailbox("Trash", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        // SELECT
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        var opened = mailbox.Open();

        Assert.That(server.DequeueRequest(), Text.EndsWith("SELECT \"Trash\"\r\n"));

        // CLOSE
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // DELETE
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        Assert.IsTrue(opened.Exists);

        opened.Delete();

        Assert.IsFalse(opened.Exists);

        Assert.That(server.DequeueRequest(), Text.EndsWith("CLOSE\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("DELETE \"Trash\"\r\n"));
      });
    }

    [Test]
    public void TestDeleteNonExistentMailbox()
    {
      TestMailbox(null,
                  new[] {ImapMailboxFlag.NonExistent},
                  "Trash",
                  delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        Assert.IsFalse(mailbox.Exists);

        // UNSUBSCRIBE
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        mailbox.Delete(true);

        Assert.IsFalse(mailbox.Exists);

        Assert.That(server.DequeueRequest(), Text.EndsWith("UNSUBSCRIBE \"Trash\"\r\n"));
      });
    }

    [Test]
    public void TestCreateRecreateDeleted()
    {
      CreateRecreateDeleted(false);
    }

    [Test]
    public void TestCreateRecreateDeletedSubscribe()
    {
      CreateRecreateDeleted(true);
    }

    private void CreateRecreateDeleted(bool subscribe)
    {
      TestMailbox("Trash", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        Assert.IsTrue(mailbox.Exists);

        // DELETE
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // UNSUBSCRIBE
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        mailbox.Delete(true);

        server.DequeueRequest();
        server.DequeueRequest();

        Assert.IsFalse(mailbox.Exists);

        // CREATE
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        if (subscribe)
          // SUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");

        if (subscribe)
          mailbox.Create(true);
        else
          mailbox.Create();

        Assert.IsTrue(mailbox.Exists);

        Assert.That(server.DequeueRequest(), Text.EndsWith("CREATE \"Trash\"\r\n"));
        if (subscribe)
          Assert.That(server.DequeueRequest(), Text.EndsWith("SUBSCRIBE \"Trash\"\r\n"));
      });
    }

    [Test]
    public void TestCreateNonExistent()
    {
      CreateNonExistent(false);
    }

    [Test]
    public void TestCreateNonExistentSubscribe()
    {
      CreateNonExistent(true);
    }

    private void CreateNonExistent(bool subscribe)
    {
      TestMailbox(null,
                  new[] {ImapMailboxFlag.NonExistent},
                  "deleted",
                  delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        Assert.IsFalse(mailbox.Exists);

        // CREATE
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        if (subscribe)
          // SUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");

        if (subscribe)
          mailbox.Create(true);
        else
          mailbox.Create();

        Assert.IsTrue(mailbox.Exists);

        Assert.That(server.DequeueRequest(), Text.EndsWith("CREATE \"deleted\"\r\n"));
        if (subscribe)
          Assert.That(server.DequeueRequest(), Text.EndsWith("SUBSCRIBE \"deleted\"\r\n"));
      });
    }

    [Test, Ignore("not implemented")]
    public void TestCreateAlreadyExists()
    {
    }

    [Test]
    [ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestMoveToInbox()
    {
      MoveToInbox(false);
    }

    [Test]
    [ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestMoveToInboxSubscribe()
    {
      MoveToInbox(true);
    }

    private void MoveToInbox(bool subscribe)
    {
      TestMailbox("/", "Draft", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        Assert.AreEqual("Draft", mailbox.Name);
        Assert.AreEqual("Draft", mailbox.FullName);
        Assert.AreEqual(string.Empty, mailbox.ParentMailboxName);
        Assert.IsFalse(mailbox.IsInbox);

        if (subscribe)
          mailbox.MoveTo("INBOX", true);
        else
          mailbox.MoveTo("INBOX");
      });
    }

    [Test]
    public void TestMoveToMoveInbox()
    {
      MoveToMoveInbox(false);
    }

    [Test]
    public void TestMoveToMoveInboxSubscribe()
    {
      MoveToMoveInbox(true);
    }

    private void MoveToMoveInbox(bool subscribe)
    {
      TestMailbox("/", "INBOX", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        Assert.AreEqual("INBOX", mailbox.Name);
        Assert.AreEqual("INBOX", mailbox.FullName);
        Assert.AreEqual(string.Empty, mailbox.ParentMailboxName);
        Assert.IsTrue(mailbox.IsInbox);

        // RENAME
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        if (subscribe)
          // SUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");

        if (subscribe)
          mailbox.MoveTo("INBOX/backup", true);
        else
          mailbox.MoveTo("INBOX/backup");

        Assert.That(server.DequeueRequest(), Text.EndsWith("RENAME \"INBOX\" \"INBOX/backup\"\r\n"));

        if (subscribe)
          Assert.That(server.DequeueRequest(), Text.EndsWith("SUBSCRIBE \"INBOX/backup\"\r\n"));

        Assert.AreEqual("backup", mailbox.Name);
        Assert.AreEqual("INBOX/backup", mailbox.FullName);
        Assert.AreEqual("INBOX", mailbox.ParentMailboxName);
      });
    }

    [Test]
    public void TestMoveToDestinationMailbox()
    {
      MoveToDestinationMailbox(false);
    }

    [Test]
    public void TestMoveToDestinationMailboxSubscribe()
    {
      MoveToDestinationMailbox(true);
    }

    private void MoveToDestinationMailbox(bool subscribe)
    {
      TestMailbox("/", "src", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        Assert.AreEqual("src", mailbox.Name);
        Assert.AreEqual("src", mailbox.FullName);
        Assert.AreEqual(string.Empty, mailbox.ParentMailboxName);

        // LIST
        server.EnqueueTaggedResponse("* LIST () \"/\" dest\r\n" +
                                     "$tag OK done\r\n");

        var destMailbox = mailbox.Client.GetMailbox("dest");

        Assert.That(server.DequeueRequest(), Text.EndsWith("LIST \"\" \"dest\"\r\n"));

        if (subscribe) {
          // LSUB
          server.EnqueueTaggedResponse("* LSUB () \"/\" src/sub1\r\n" +
                                       "* LSUB () \"/\" src/sub2/sub3\r\n" +
                                       "* LSUB () \"/\" src/sub4/sub5/sub6\r\n" +
                                       "$tag OK done\r\n");
        }

        // RENAME
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        if (subscribe) {
          // UNSUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // SUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");

          // UNSUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // SUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // UNSUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // SUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // UNSUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // SUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
        }

        if (subscribe)
          mailbox.MoveTo(destMailbox, true);
        else
          mailbox.MoveTo(destMailbox);

        if (subscribe) {
          Assert.That(server.DequeueRequest(), Text.Contains("LSUB \"\" \"src/*\""));
        }

        Assert.That(server.DequeueRequest(), Text.EndsWith("RENAME \"src\" \"dest/src\"\r\n"));

        if (subscribe) {
          Assert.That(server.DequeueRequest(), Text.EndsWith("UNSUBSCRIBE \"src\"\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("SUBSCRIBE \"dest/src\"\r\n"));

          Assert.That(server.DequeueRequest(), Text.EndsWith("UNSUBSCRIBE \"src/sub1\"\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("SUBSCRIBE \"dest/src/sub1\"\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("UNSUBSCRIBE \"src/sub2/sub3\"\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("SUBSCRIBE \"dest/src/sub2/sub3\"\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("UNSUBSCRIBE \"src/sub4/sub5/sub6\"\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("SUBSCRIBE \"dest/src/sub4/sub5/sub6\"\r\n"));
        }

        Assert.AreEqual("src", mailbox.Name);
        Assert.AreEqual("dest/src", mailbox.FullName);
        Assert.AreEqual("dest", mailbox.ParentMailboxName);
      });
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestMoveToDestinationMailboxDestinationNonHierarchical()
    {
      TestMailbox(null,
                  null,
                  string.Empty,
                  "dest",
                  MoveToDestinationMailboxDestinationCanNotHaveChild);
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestMoveToDestinationMailboxDestinationNoInferiors()
    {
      TestMailbox(null,
                  new[] {ImapMailboxFlag.NoInferiors},
                  ".",
                  "dest",
                  MoveToDestinationMailboxDestinationCanNotHaveChild);
    }

    private void MoveToDestinationMailboxDestinationCanNotHaveChild(ImapPseudoServer server, ImapMailboxInfo destMailbox)
    {
      // LIST
      server.EnqueueTaggedResponse("* LIST () \"/\" src\r\n" +
                                   "$tag OK done\r\n");

      var sourceMailbox = destMailbox.Client.GetMailbox("src");

      Assert.That(server.DequeueRequest(), Text.EndsWith("LIST \"\" \"src\"\r\n"));

      sourceMailbox.MoveTo(destMailbox);
    }

    [Test, Ignore("not implemented")]
    public void TestMoveToDestinationMailboxAlreadyExists()
    {
    }

    [Test]
    [ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestMoveToSameName1()
    {
      TestMailbox("/", "Draft", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        Assert.AreEqual("Draft", mailbox.Name);
        Assert.AreEqual("Draft", mailbox.FullName);
        Assert.AreEqual(string.Empty, mailbox.ParentMailboxName);
        Assert.IsFalse(mailbox.IsInbox);

        mailbox.MoveTo("Draft");
      });
    }

    [Test]
    [ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestMoveToSameName2()
    {
      TestMailbox("/", "INBOX", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        Assert.AreEqual("INBOX", mailbox.Name);
        Assert.AreEqual("INBOX", mailbox.FullName);
        Assert.AreEqual(string.Empty, mailbox.ParentMailboxName);
        Assert.IsTrue(mailbox.IsInbox);

        mailbox.MoveTo("inbox");
      });
    }

    [Test]
    public void TestMoveToNewNameDeepen()
    {
      MoveToNewNameDeepen(false);
    }

    [Test]
    public void TestMoveToNewNameDeepenSubscribe()
    {
      MoveToNewNameDeepen(true);
    }

    private void MoveToNewNameDeepen(bool subscribe)
    {
      TestMailbox(new[] {ImapCapability.ListExtended},
                  null,
                  ".",
                  "src",
                  delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        if (subscribe) {
          // LIST
          server.EnqueueTaggedResponse("* LIST (\\Subscribed) \".\" src.sub1\r\n" +
                                       "* LIST (\\Subscribed \\NonExistent) \".\" src.sub2\r\n" +
                                       "$tag OK done\r\n");
        }

        // RENAME
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        if (subscribe) {
          // UNSUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // SUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");

          // UNSUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // UNSUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // SUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
        }

        if (subscribe)
          mailbox.MoveTo("Trash.src", true);
        else
          mailbox.MoveTo("Trash.src");

        if (subscribe) {
          Assert.That(server.DequeueRequest(), Text.Contains("LIST (SUBSCRIBED) \"\" \"src.*\""));
        }

        Assert.That(server.DequeueRequest(), Text.EndsWith("RENAME \"src\" \"Trash.src\"\r\n"));

        if (subscribe) {
          Assert.That(server.DequeueRequest(), Text.EndsWith("UNSUBSCRIBE \"src\"\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("SUBSCRIBE \"Trash.src\"\r\n"));

          Assert.That(server.DequeueRequest(), Text.EndsWith("UNSUBSCRIBE \"src.sub1\"\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("SUBSCRIBE \"Trash.src.sub1\"\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("UNSUBSCRIBE \"src.sub2\"\r\n"));
        }

        Assert.AreEqual("src", mailbox.Name);
        Assert.AreEqual("Trash.src", mailbox.FullName);
        Assert.AreEqual("Trash", mailbox.ParentMailboxName);
      });
    }

    [Test]
    public void TestMoveToNewNameShallow()
    {
      MoveToNewNameShallow(false);
    }

    [Test]
    public void TestMoveToNewNameShallowSubscribe()
    {
      MoveToNewNameShallow(true);
    }

    private void MoveToNewNameShallow(bool subscribe)
    {
      TestMailbox("/",
                  "nested/src",
                  delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        Assert.AreEqual("src", mailbox.Name);
        Assert.AreEqual("nested/src", mailbox.FullName);
        Assert.AreEqual("nested", mailbox.ParentMailboxName);

        if (subscribe) {
          // LSUB
          server.EnqueueTaggedResponse("* LSUB () \"/\" nested/src/sub1\r\n" +
                                       "* LSUB () \"/\" nested/src/sub2/sub3\r\n" +
                                       "$tag OK done\r\n");
        }

        // RENAME
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        if (subscribe) {
          // UNSUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // SUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");

          // UNSUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // SUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // UNSUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // SUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
        }

        if (subscribe)
          mailbox.MoveTo("dest", true);
        else
          mailbox.MoveTo("dest");

        if (subscribe) {
          Assert.That(server.DequeueRequest(), Text.Contains("LSUB \"\" \"nested/src/*\""));
        }

        Assert.That(server.DequeueRequest(), Text.EndsWith("RENAME \"nested/src\" \"dest\"\r\n"));

        if (subscribe) {
          Assert.That(server.DequeueRequest(), Text.EndsWith("UNSUBSCRIBE \"nested/src\"\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("SUBSCRIBE \"dest\"\r\n"));

          Assert.That(server.DequeueRequest(), Text.EndsWith("UNSUBSCRIBE \"nested/src/sub1\"\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("SUBSCRIBE \"dest/sub1\"\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("UNSUBSCRIBE \"nested/src/sub2/sub3\"\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("SUBSCRIBE \"dest/sub2/sub3\"\r\n"));
        }

        Assert.AreEqual("dest", mailbox.Name);
        Assert.AreEqual("dest", mailbox.FullName);
        Assert.AreEqual(string.Empty, mailbox.ParentMailboxName);
      });
    }

    [Test]
    public void TestMoveToNewNameOpened()
    {
      TestMailbox(".", "INBOX", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        // SELECT
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        mailbox.Open();

        Assert.That(server.DequeueRequest(), Text.EndsWith("SELECT \"INBOX\"\r\n"));

        Assert.IsTrue(mailbox.IsOpen);

        // CLOSE
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // RENAME
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // SELECT
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        mailbox.MoveTo("Trash.INBOX");

        Assert.That(server.DequeueRequest(), Text.EndsWith("CLOSE\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("RENAME \"INBOX\" \"Trash.INBOX\"\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("SELECT \"Trash.INBOX\"\r\n"));

        Assert.AreEqual("INBOX", mailbox.Name);
        Assert.AreEqual("Trash.INBOX", mailbox.FullName);
        Assert.AreEqual("Trash", mailbox.ParentMailboxName);
        Assert.IsTrue(mailbox.IsOpen);
      });
    }

    [Test]
    public void TestMoveToNewNameSubscribeHasNoChildren()
    {
      TestMailbox(null,
                  new[] {ImapMailboxFlag.HasNoChildren},
                  ".",
                  "Draft",
                  delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        // LSUB
        server.EnqueueTaggedResponse("* LSUB () \".\" Draft.Created\r\n" + // other client created children
                                     "$tag OK done\r\n");
        // RENAME
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // UNSUBSCRIBE
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // SUBSCRIBE
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // UNSUBSCRIBE
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // SUBSCRIBE
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        mailbox.MoveTo("Sent", true);

        Assert.That(server.DequeueRequest(), Text.Contains("LSUB \"\" \"Draft.*\""));
        Assert.That(server.DequeueRequest(), Text.EndsWith("RENAME \"Draft\" \"Sent\"\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("UNSUBSCRIBE \"Draft\"\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("SUBSCRIBE \"Sent\"\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("UNSUBSCRIBE \"Draft.Created\"\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("SUBSCRIBE \"Sent.Created\"\r\n"));

        Assert.AreEqual("Sent", mailbox.Name);
        Assert.AreEqual("Sent", mailbox.FullName);
        Assert.AreEqual(string.Empty, mailbox.ParentMailboxName);
      });
    }

    [Test]
    public void TestMoveToNewNameSubscribeNonHierarchical()
    {
      TestMailbox(null,
                  null,
                  string.Empty,
                  "Sent",
                  MoveToNewNameSubscribeCanNotHaveChild);
    }

    [Test]
    public void TestMoveToNewNameSubscribeNoInferiors()
    {
      TestMailbox(null,
                  new[] {ImapMailboxFlag.NoInferiors},
                  ".",
                  "Sent",
                  MoveToNewNameSubscribeCanNotHaveChild);
    }

    private void MoveToNewNameSubscribeCanNotHaveChild(ImapPseudoServer server, ImapMailboxInfo mailbox)
    {
      // RENAME
      server.EnqueueTaggedResponse("$tag OK done\r\n");
      // UNSUBSCRIBE
      server.EnqueueTaggedResponse("$tag OK done\r\n");
      // SUBSCRIBE
      server.EnqueueTaggedResponse("$tag OK done\r\n");

      mailbox.MoveTo("OldSent", true);

      Assert.That(server.DequeueRequest(), Text.EndsWith("RENAME \"Sent\" \"OldSent\"\r\n"));
      Assert.That(server.DequeueRequest(), Text.EndsWith("UNSUBSCRIBE \"Sent\"\r\n"));
      Assert.That(server.DequeueRequest(), Text.EndsWith("SUBSCRIBE \"OldSent\"\r\n"));
    }

    [Test, Ignore("not implemented")]
    public void TestMoveToNewNameAlreadyExists()
    {
    }

    [Test]
    public void TestSubscribe()
    {
      Subscribe(false);
    }

    [Test]
    public void TestSubscribeRecursive()
    {
      Subscribe(true);
    }

    private void Subscribe(bool recursive)
    {
      TestMailbox("INBOX", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        // SUBSCRIBE
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        if (recursive) {
          // LIST
          server.EnqueueTaggedResponse("* LIST () \".\" INBOX.Child1\r\n" +
                                       "* LIST () \".\" INBOX.Child2\r\n" +
                                       "$tag OK done\r\n");
          // SUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          server.EnqueueTaggedResponse("$tag OK done\r\n");
        }

        if (recursive)
          mailbox.Subscribe(true);
        else
          mailbox.Subscribe();

        Assert.That(server.DequeueRequest(), Text.EndsWith("SUBSCRIBE \"INBOX\"\r\n"));

        if (recursive) {
          Assert.That(server.DequeueRequest(), Text.EndsWith("LIST \"\" \"INBOX.*\"\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("SUBSCRIBE \"INBOX.Child1\"\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("SUBSCRIBE \"INBOX.Child2\"\r\n"));
        }
      });
    }

    [Test]
    public void TestUnsubscribe()
    {
      Unsubscribe(false);
    }

    [Test]
    public void TestUnsubscribeRecursive()
    {
      Unsubscribe(true);
    }

    private void Unsubscribe(bool recursive)
    {
      TestMailbox("INBOX", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        // UNSUBSCRIBE
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        if (recursive) {
          // LSUB
          server.EnqueueTaggedResponse("* LSUB () \".\" INBOX.Child1\r\n" +
                                       "* LSUB () \".\" INBOX.Child2\r\n" +
                                       "$tag OK done\r\n");
          // UNSUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          server.EnqueueTaggedResponse("$tag OK done\r\n");
        }

        if (recursive)
          mailbox.Unsubscribe(true);
        else
          mailbox.Unsubscribe();

        Assert.That(server.DequeueRequest(), Text.EndsWith("UNSUBSCRIBE \"INBOX\"\r\n"));

        if (recursive) {
          Assert.That(server.DequeueRequest(), Text.EndsWith("LSUB \"\" \"INBOX.*\"\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("UNSUBSCRIBE \"INBOX.Child1\"\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("UNSUBSCRIBE \"INBOX.Child2\"\r\n"));
        }
      });
    }

    [Test]
    public void TestGetMailboxes()
    {
      GetMailboxes(new ImapMailboxFlag[0]);
    }

    [Test]
    public void TestGetMailboxesHasNoChildren()
    {
      GetMailboxes(new[] {ImapMailboxFlag.HasNoChildren});
    }

    private void GetMailboxes(ImapMailboxFlag[] flags)
    {
      TestMailbox(null, flags, "INBOX", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        var mailboxes = mailbox.GetMailboxes();

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" INBOX.Child1\r\n" +
                                     "* LIST () \".\" INBOX.Child2\r\n" +
                                     "* LIST () \".\" INBOX.Child2.Child3\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreEqual("Child1", enumerator.Current.Name);
        Assert.AreEqual("INBOX.Child1", enumerator.Current.FullName);

        StringAssert.EndsWith("LIST \"\" \"INBOX.*\"\r\n", server.DequeueRequest());

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreEqual("Child2", enumerator.Current.Name);
        Assert.AreEqual("INBOX.Child2", enumerator.Current.FullName);

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreEqual("Child3", enumerator.Current.Name);
        Assert.AreEqual("INBOX.Child2.Child3", enumerator.Current.FullName);

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestGetMailboxesNonHierarchical()
    {
      TestMailbox(null,
                  null,
                  string.Empty,
                  "INBOX",
                  GetMailboxesNoInferiorsOrNonHierarchical);
    }

    [Test]
    public void TestGetMailboxesNoInferiors()
    {
      TestMailbox(null,
                  new[] {ImapMailboxFlag.NoInferiors},
                  ".",
                  "INBOX",
                  GetMailboxesNoInferiorsOrNonHierarchical);
    }

    private void GetMailboxesNoInferiorsOrNonHierarchical(ImapPseudoServer server, ImapMailboxInfo mailbox)
    {
      var mailboxes = mailbox.GetMailboxes();

      Assert.IsNotNull(mailboxes);

      var enumerator = mailboxes.GetEnumerator();

      Assert.IsNotNull(enumerator);
      Assert.IsFalse(enumerator.MoveNext());
    }

    [Test]
    public void TestGetParent()
    {
      TestMailbox(".", "INBOX.Child", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" INBOX\r\n" +
                                     "$tag OK done\r\n");

        var parent = mailbox.GetParent();

        Assert.That(server.DequeueRequest(), Text.EndsWith("LIST \"\" \"INBOX\"\r\n"));

        Assert.IsNotNull(parent);
        Assert.AreEqual("INBOX", parent.Name);
        Assert.AreEqual("INBOX", parent.FullName);
      });
    }

    [Test]
    public void TestGetParentRoot()
    {
      TestMailbox(".", "INBOX", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        var parent = mailbox.GetParent();

        Assert.IsNull(parent);
      });
    }

    [Test]
    public void TestGetParentNotFound()
    {
      TestMailbox(".", "Sent.Child", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        // LIST
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        try {
          mailbox.GetParent();
          Assert.Fail("ImapMailboxNotFoundException not thrown");
        }
        catch (ImapMailboxNotFoundException ex) {
          Assert.IsNotNull(ex.Mailbox);
          Assert.AreEqual("Sent", ex.Mailbox);

          Smdn.Net.TestUtils.SerializeBinary(ex, delegate(ImapMailboxNotFoundException deserialized) {
            Assert.IsNotNull(deserialized.Mailbox);
            Assert.AreEqual(ex.Mailbox, deserialized.Mailbox);
          });
        }
      });
    }

    [Test]
    public void TestGetOrCreateParent()
    {
      GetOrCreateParent(false);
    }

    [Test]
    public void TestGetOrCreateParentSubscribe()
    {
      GetOrCreateParent(true);
    }

    private void GetOrCreateParent(bool subscribe)
    {
      TestMailbox(".", "Sent.Child", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        // LIST
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // CREATE
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        if (subscribe)
          // SUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" Sent\r\n" +
                                     "$tag OK done\r\n");

        var parent = subscribe
          ? mailbox.GetOrCreateParent(true)
          : mailbox.GetOrCreateParent();

        Assert.That(server.DequeueRequest(), Text.EndsWith("LIST \"\" \"Sent\"\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("CREATE \"Sent\"\r\n"));
        if (subscribe)
          Assert.That(server.DequeueRequest(), Text.EndsWith("SUBSCRIBE \"Sent\"\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("LIST \"\" \"Sent\"\r\n"));

        Assert.IsNotNull(parent);
        Assert.AreEqual("Sent", parent.Name);
        Assert.AreEqual("Sent", parent.FullName);
      });
    }

    [Test]
    public void TestGetFullNameOf()
    {
      TestMailbox(".", "INBOX", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        Assert.AreEqual("INBOX.Child", mailbox.GetFullNameOf("Child"));
      });

      TestMailbox("/", "INBOX", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        Assert.AreEqual("INBOX/Child", mailbox.GetFullNameOf("Child"));
      });
    }

    [Test]
    public void TestCreateChild()
    {
      CreateChild(false);
    }

    [Test]
    public void TestCreateChildSubscribe()
    {
      CreateChild(true);
    }

    private void CreateChild(bool subscribe)
    {
      TestMailbox(".", "Sent", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        // CREATE
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        if (subscribe)
          // SUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" Sent.Child\r\n" +
                                     "$tag OK done\r\n");

        var child = subscribe
          ? mailbox.CreateChild("Child", true)
          : mailbox.CreateChild("Child");

        Assert.That(server.DequeueRequest(), Text.EndsWith("CREATE \"Sent.Child\"\r\n"));
        if (subscribe)
          Assert.That(server.DequeueRequest(), Text.EndsWith("SUBSCRIBE \"Sent.Child\"\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("LIST \"\" \"Sent.Child\"\r\n"));

        Assert.IsNotNull(child);
        Assert.AreEqual("Child", child.Name);
        Assert.AreEqual("Sent.Child", child.FullName);
      });
    }

    [Test, Ignore("not implemented")]
    public void TestCreateChildAlreadyExists()
    {
    }

    [Test]
    public void TestGetChild()
    {
      TestMailbox(".", "INBOX", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" INBOX.Child\r\n" +
                                     "$tag OK done\r\n");

        var child = mailbox.GetChild("Child");

        Assert.That(server.DequeueRequest(), Text.EndsWith("LIST \"\" \"INBOX.Child\"\r\n"));

        Assert.IsNotNull(child);
        Assert.AreEqual("Child", child.Name);
        Assert.AreEqual("INBOX.Child", child.FullName);
      });
    }

    [Test]
    public void TestGetChildNotFound()
    {
      TestMailbox(".", "INBOX", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        // LIST
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        try {
          mailbox.GetChild("Child");
          Assert.Fail("ImapMailboxNotFoundException not thrown");
        }
        catch (ImapMailboxNotFoundException ex) {
          Assert.IsNotNull(ex.Mailbox);
          Assert.AreEqual("INBOX.Child", ex.Mailbox);

          Smdn.Net.TestUtils.SerializeBinary(ex, delegate(ImapMailboxNotFoundException deserialized) {
            Assert.IsNotNull(deserialized.Mailbox);
            Assert.AreEqual(ex.Mailbox, deserialized.Mailbox);
          });
        }
      });
    }

    [Test]
    public void TestGetOrCreateChild()
    {
      GetOrCreateChild(false);
    }

    [Test]
    public void TestGetOrCreateChildSubscribe()
    {
      GetOrCreateChild(true);
    }

    private void GetOrCreateChild(bool subscribe)
    {
      TestMailbox(".", "Sent", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        // LIST
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // CREATE
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        if (subscribe)
          // SUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" Sent.Child\r\n" +
                                     "$tag OK done\r\n");

        var child = subscribe
          ? mailbox.GetOrCreateChild("Child", true)
          : mailbox.GetOrCreateChild("Child");

        Assert.That(server.DequeueRequest(), Text.EndsWith("LIST \"\" \"Sent.Child\"\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("CREATE \"Sent.Child\"\r\n"));
        if (subscribe)
          Assert.That(server.DequeueRequest(), Text.EndsWith("SUBSCRIBE \"Sent.Child\"\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("LIST \"\" \"Sent.Child\"\r\n"));

        Assert.IsNotNull(child);
        Assert.AreEqual("Child", child.Name);
        Assert.AreEqual("Sent.Child", child.FullName);
      });
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void TestGetChildNonHierarchical()
    {
      TestNonHierarchicalMailbox(delegate(ImapMailboxInfo mailbox) {
        mailbox.GetChild("Child");
      });
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void TestGetOrCreateChildNonHierarchical()
    {
      TestNonHierarchicalMailbox(delegate(ImapMailboxInfo mailbox) {
        mailbox.GetOrCreateChild("Child");
      });
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void TestCreateChildNonHierarchical()
    {
      TestNonHierarchicalMailbox(delegate(ImapMailboxInfo mailbox) {
        mailbox.CreateChild("Child");
      });
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void TestGetFullNameOfNonHierarchical()
    {
      TestNonHierarchicalMailbox(delegate(ImapMailboxInfo mailbox) {
        mailbox.GetFullNameOf("Child");
      });
    }

    private void TestNonHierarchicalMailbox(Action<ImapMailboxInfo> action)
    {
      TestMailbox(string.Empty, "INBOX", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        Assert.IsFalse(mailbox.CanHaveChild);

        action(mailbox);
      });
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void TestGetChildNoInferiors()
    {
      TestNoInferiorsMailbox(delegate(ImapMailboxInfo mailbox) {
        mailbox.GetChild("Child");
      });
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void TestGetOrCreateChildNoInferiors()
    {
      TestNoInferiorsMailbox(delegate(ImapMailboxInfo mailbox) {
        mailbox.GetOrCreateChild("Child");
      });
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void TestCreateChildNoInferiors()
    {
      TestNoInferiorsMailbox(delegate(ImapMailboxInfo mailbox) {
        mailbox.CreateChild("Child");
      });
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void TestGetFullNameOfNoInferiors()
    {
      TestNoInferiorsMailbox(delegate(ImapMailboxInfo mailbox) {
        mailbox.GetFullNameOf("Child");
      });
    }

    private void TestNoInferiorsMailbox(Action<ImapMailboxInfo> action)
    {
      TestMailbox(null,
                  new[] {ImapMailboxFlag.NoInferiors},
                  string.Empty,
                  "INBOX",
                  delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        Assert.IsFalse(mailbox.CanHaveChild);

        action(mailbox);
      });
    }

    [Test, Ignore("to be written")]
    public void TestAppendMessage()
    {
    }

    [Test, Ignore("to be written")]
    public void TestAppendMessages()
    {
    }

    [Test, Ignore("to be written")]
    public void TestWriteMessage()
    {
    }

    [Test]
    public void TestGetQuota()
    {
      TestMailbox(new[] {ImapCapability.Quota},
                  null,
                  "INBOX",
                  delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        // GETQUOTAROOT
        server.EnqueueTaggedResponse("* QUOTAROOT INBOX \"\"\r\n" +
                                     "* QUOTA \"\" (STORAGE 10 512)\r\n" +
                                     "$tag OK done\r\n");

        var quotas = new List<ImapQuota>(mailbox.GetQuota());

        Assert.That(server.DequeueRequest(), Text.EndsWith("GETQUOTAROOT \"INBOX\"\r\n"));

        Assert.AreEqual(1, quotas.Count);

        var quota = quotas[0];

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
      TestMailbox("INBOX", delegate(ImapPseudoServer server, ImapMailboxInfo mailbox) {
        var quotas = new List<ImapQuota>(mailbox.GetQuota());

        Assert.AreEqual(0, quotas.Count());
      });
    }
  }
}
