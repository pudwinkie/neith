using System;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif

namespace Smdn.Net.Imap4.Client {
  [TestFixture]
  public class ImapMessageInfoBaseTests {
    private class MessageSet : ImapMessageInfoBase {
      public MessageSet(ImapOpenedMailboxInfo mailbox, ImapSequenceSet sequenceOrUidSet)
        : base(mailbox)
      {
        this.sequenceOrUidSet = sequenceOrUidSet;
      }

      protected override ImapSequenceSet GetSequenceOrUidSet()
      {
        return sequenceOrUidSet;
      }

      private ImapSequenceSet sequenceOrUidSet;
    }

    private void TestMessage(int existMesasgeCount, ImapSequenceSet sequenceOrUidSet, Action<ImapPseudoServer, ImapMessageInfoBase, ImapMessageInfo[]> action)
    {
      TestMessage(existMesasgeCount, null, sequenceOrUidSet, action);
    }

    private void TestMessage(int existMessageCount, ImapCapability[] capabilities, ImapSequenceSet sequenceOrUidSet, Action<ImapPseudoServer, ImapMessageInfoBase, ImapMessageInfo[]> action)
    {
      TestUtils.TestAuthenticated(capabilities, delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" INBOX\r\n" +
                                     "$tag OK done\r\n");
        // SELECT
        server.EnqueueTaggedResponse(string.Format("* EXISTS {0}\r\n", existMessageCount) +
                                     "* OK [UIDVALIDITY 1]\r\n" +
                                     "$tag OK done\r\n");

        using (var inbox = client.OpenInbox()) {
          server.DequeueRequest(); // LIST
          server.DequeueRequest(); // SELECT

          var messages = new ImapMessageInfo[0];

          if (0 < existMessageCount) {
            // NOOP
            server.EnqueueTaggedResponse("$tag OK done\r\n");
            // FETCH
            var fetchResp = new StringBuilder();

            for (var seq = 1; seq <= existMessageCount; seq++) {
              fetchResp.AppendFormat("* FETCH {0} (UID {0})\r\n", seq);
            }

            fetchResp.Append("$tag OK done\r\n");

            server.EnqueueTaggedResponse(fetchResp.ToString());

            messages = inbox.GetMessages().ToArray();

            server.DequeueRequest(); // NOOP
            server.DequeueRequest(); // FETCH
          }

          try {
            var messageSet = new MessageSet(inbox, sequenceOrUidSet);

            Assert.AreSame(messageSet.Client, client);
            Assert.AreSame(messageSet.Mailbox, inbox);
            Assert.AreEqual(1L, messageSet.UidValidity);

            action(server, messageSet, messages);
          }
          finally {
            if (inbox.IsOpen)
              // CLOSE
              server.EnqueueTaggedResponse("$tag OK done\r\n");
          }
        }
      });
    }

    private ImapSequenceSet CreateEmptySet()
    {
      return ImapSequenceSet.CreateSet(new long[0]);
    }

    [Test]
    public void TestAddFlags()
    {
      TestMessage(1, ImapSequenceSet.CreateUidSet(1L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // UID STORE
        server.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Seen \\Draft))\r\n" +
                                     "$tag OK done\r\n");

        messageSet.AddFlags(ImapMessageFlag.Draft);

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID STORE 1 +FLAGS (\\Draft)\r\n"));

        Assert.IsNotNull(messages[0].Flags);
        Assert.AreEqual(2, messages[0].Flags.Count);
        Assert.IsTrue(messages[0].Flags.Contains(ImapMessageFlag.Seen));
        Assert.IsTrue(messages[0].Flags.Contains(ImapMessageFlag.Draft));
        Assert.IsTrue(messages[0].IsSeen);
        Assert.IsTrue(messages[0].IsDraft);
      });
    }

    [Test]
    public void TestRemoveFlags()
    {
      TestMessage(1, ImapSequenceSet.CreateUidSet(1L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // UID STORE
        server.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Seen))\r\n" +
                                     "$tag OK done\r\n");

        messageSet.RemoveFlags(ImapMessageFlag.Draft);

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID STORE 1 -FLAGS (\\Draft)\r\n"));

        Assert.IsNotNull(messages[0].Flags);
        Assert.AreEqual(1, messages[0].Flags.Count);
        Assert.IsTrue(messages[0].Flags.Contains(ImapMessageFlag.Seen));
        Assert.IsTrue(messages[0].IsSeen);
      });
    }

    [Test]
    public void TestReplaceFlags()
    {
      TestMessage(1, ImapSequenceSet.CreateUidSet(1L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // UID STORE
        server.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Draft \\Flagged))\r\n" +
                                     "$tag OK done\r\n");

        messageSet.ReplaceFlags(ImapMessageFlag.Draft, ImapMessageFlag.Flagged);

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID STORE 1 FLAGS (\\Draft \\Flagged)\r\n"));

        Assert.IsNotNull(messages[0].Flags);
        Assert.AreEqual(2, messages[0].Flags.Count);
        Assert.IsTrue(messages[0].Flags.Contains(ImapMessageFlag.Draft));
        Assert.IsTrue(messages[0].Flags.Contains(ImapMessageFlag.Flagged));
        Assert.IsTrue(messages[0].IsDraft);
        Assert.IsTrue(messages[0].IsFlagged);
      });
    }

    [Test]
    public void TestStoreEmptySet()
    {
      TestMessage(0, CreateEmptySet(),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // do nothing
        messageSet.Store(ImapStoreDataItem.AddFlags(ImapMessageFlag.Seen));
      });
    }

    [Test]
    public void TestStoreUidSingle()
    {
      TestMessage(1, ImapSequenceSet.CreateUidSet(1L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // UID STORE
        server.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Seen))\r\n" +
                                     "$tag OK done\r\n");

        messageSet.Store(ImapStoreDataItem.AddFlags(ImapMessageFlag.Seen));

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID STORE 1 +FLAGS (\\Seen)\r\n"));

        Assert.IsNotNull(messages[0].Flags);
        Assert.AreEqual(1, messages[0].Flags.Count);
        Assert.IsTrue(messages[0].Flags.Contains(ImapMessageFlag.Seen));
        Assert.IsTrue(messages[0].IsSeen);
      });
    }

    [Test]
    public void TestStoreSequenceRangeSet()
    {
      TestMessage(3, ImapSequenceSet.CreateRangeSet(1L, 3L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // STORE
        server.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Draft))\r\n" +
                                     "* FETCH 2 (FLAGS ($label1))\r\n" +
                                     "* FETCH 3 (FLAGS ())\r\n" +
                                     "$tag OK done\r\n");

        messageSet.Store(ImapStoreDataItem.RemoveFlags(ImapMessageFlag.Seen));

        Assert.That(server.DequeueRequest(), Text.EndsWith("STORE 1:3 -FLAGS (\\Seen)\r\n"));

        Assert.IsNotNull(messages[0].Flags);
        Assert.AreEqual(1, messages[0].Flags.Count);
        Assert.IsTrue(messages[0].Flags.Contains(ImapMessageFlag.Draft));
        Assert.IsTrue(messages[0].IsDraft);

        Assert.IsNotNull(messages[1].Flags);
        Assert.AreEqual(1, messages[1].Flags.Count);
        Assert.IsTrue(messages[1].Flags.Contains("$label1"));

        Assert.IsNotNull(messages[2].Flags);
        Assert.AreEqual(0, messages[2].Flags.Count);
      });
    }

    [Test]
    public void TestStoreMailboxClosed()
    {
      TestMessage(10, ImapSequenceSet.CreateRangeSet(1L, 10L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // CLOSE
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        messageSet.Mailbox.Close();

        Assert.IsFalse(messageSet.Mailbox.IsOpen);

        try {
          messageSet.Store(ImapStoreDataItem.RemoveFlags(ImapMessageFlag.Seen));
          Assert.Fail("ImapMailboxClosedException not thrown");
        }
        catch (ImapMailboxClosedException ex) {
          Assert.IsNotNull(ex.Mailbox);

          Smdn.Net.TestUtils.SerializeBinary(ex, delegate(ImapMailboxClosedException deserialized) {
            Assert.IsNotNull(deserialized.Mailbox);
            Assert.AreEqual(ex.Mailbox, deserialized.Mailbox);
          });
        }
      });
    }

    [Test, Ignore("not implemented")]
    public void TestStoreUidValidityChanged()
    {
    }

    [Test, Ignore("not implemented")]
    public void TestStoreNonApplicableFlags()
    {
    }

    [Test]
    public void TestMarkAsDeleted()
    {
      TestMessage(3, ImapSequenceSet.CreateRangeSet(1L, 3L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // STORE
        server.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Deleted))\r\n" +
                                     "* FETCH 2 (FLAGS (\\Deleted \\Seen))\r\n" +
                                     "* FETCH 3 (FLAGS (\\Deleted $label1))\r\n" +
                                     "$tag OK done\r\n");

        messageSet.MarkAsDeleted();

        Assert.That(server.DequeueRequest(), Text.EndsWith("STORE 1:3 +FLAGS (\\Deleted)\r\n"));

        Assert.IsNotNull(messages[0].Flags);
        Assert.AreEqual(1, messages[0].Flags.Count);
        Assert.IsTrue(messages[0].Flags.Contains(ImapMessageFlag.Deleted));
        Assert.IsTrue(messages[0].IsMarkedAsDeleted);

        Assert.IsNotNull(messages[1].Flags);
        Assert.AreEqual(2, messages[1].Flags.Count);
        Assert.IsTrue(messages[1].Flags.Contains(ImapMessageFlag.Deleted));
        Assert.IsTrue(messages[1].Flags.Contains(ImapMessageFlag.Seen));
        Assert.IsTrue(messages[1].IsSeen);
        Assert.IsTrue(messages[1].IsMarkedAsDeleted);

        Assert.IsNotNull(messages[2].Flags);
        Assert.AreEqual(2, messages[2].Flags.Count);
        Assert.IsTrue(messages[2].Flags.Contains(ImapMessageFlag.Deleted));
        Assert.IsTrue(messages[2].Flags.Contains("$label1"));
        Assert.IsTrue(messages[2].IsMarkedAsDeleted);
      });
    }

    [Test]
    public void TestMarkAsSeen()
    {
      TestMessage(3, ImapSequenceSet.CreateRangeSet(1L, 3L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // STORE
        server.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Seen))\r\n" +
                                     "* FETCH 2 (FLAGS (\\Seen \\Draft))\r\n" +
                                     "* FETCH 3 (FLAGS (\\Seen $label1))\r\n" +
                                     "$tag OK done\r\n");

        messageSet.MarkAsSeen();

        Assert.That(server.DequeueRequest(), Text.EndsWith("STORE 1:3 +FLAGS (\\Seen)\r\n"));

        Assert.IsNotNull(messages[0].Flags);
        Assert.AreEqual(1, messages[0].Flags.Count);
        Assert.IsTrue(messages[0].Flags.Contains(ImapMessageFlag.Seen));
        Assert.IsTrue(messages[0].IsSeen);

        Assert.IsNotNull(messages[1].Flags);
        Assert.AreEqual(2, messages[1].Flags.Count);
        Assert.IsTrue(messages[1].Flags.Contains(ImapMessageFlag.Seen));
        Assert.IsTrue(messages[1].Flags.Contains(ImapMessageFlag.Draft));
        Assert.IsTrue(messages[1].IsSeen);
        Assert.IsTrue(messages[1].IsDraft);

        Assert.IsNotNull(messages[2].Flags);
        Assert.AreEqual(2, messages[2].Flags.Count);
        Assert.IsTrue(messages[2].Flags.Contains(ImapMessageFlag.Seen));
        Assert.IsTrue(messages[2].Flags.Contains("$label1"));
        Assert.IsTrue(messages[2].IsSeen);
      });
    }

    [Test]
    public void TestDeleteUidPlusCapable()
    {
      Delete(true);
    }

    [Test]
    public void TestDeleteUidPlusIncapable()
    {
      Delete(false);
    }

    private void Delete(bool uidPlusCapable)
    {
      var capabilities = uidPlusCapable
        ? new[] {ImapCapability.UidPlus}
        : new ImapCapability[0];

      TestMessage(3,
                  capabilities,
                  ImapSequenceSet.CreateUidRangeSet(1L, 3L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // STORE
        server.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Deleted))\r\n" +
                                     "* FETCH 2 (FLAGS (\\Deleted \\Seen))\r\n" +
                                     "* FETCH 3 (FLAGS (\\Deleted $label1))\r\n" +
                                     "$tag OK done\r\n");
        // EXPUNGE/UID EXPUNGE
        server.EnqueueTaggedResponse("* EXPUNGE 3\r\n" +
                                     "* EXPUNGE 2\r\n" +
                                     "* EXPUNGE 1\r\n" +
                                     "$tag OK done\r\n");

        messageSet.Delete();

        Assert.That(server.DequeueRequest(), Text.EndsWith("STORE 1:3 +FLAGS (\\Deleted)\r\n"));
        if (uidPlusCapable)
          Assert.That(server.DequeueRequest(), Text.EndsWith("UID EXPUNGE 1:3\r\n"));
        else
          Assert.That(server.DequeueRequest(), Text.EndsWith("EXPUNGE\r\n"));

        Assert.AreEqual(0L, messageSet.Mailbox.ExistMessageCount);

        Assert.AreEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, messages[0].Sequence);
        Assert.IsTrue(messages[0].IsDeleted);

        Assert.IsNotNull(messages[0].Flags);
        Assert.AreEqual(1, messages[0].Flags.Count);
        Assert.IsTrue(messages[0].Flags.Contains(ImapMessageFlag.Deleted));
        Assert.IsTrue(messages[0].IsMarkedAsDeleted);

        Assert.AreEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, messages[1].Sequence);
        Assert.IsTrue(messages[1].IsDeleted);

        Assert.IsNotNull(messages[1].Flags);
        Assert.AreEqual(2, messages[1].Flags.Count);
        Assert.IsTrue(messages[1].Flags.Contains(ImapMessageFlag.Deleted));
        Assert.IsTrue(messages[1].Flags.Contains(ImapMessageFlag.Seen));
        Assert.IsTrue(messages[1].IsMarkedAsDeleted);
        Assert.IsTrue(messages[1].IsSeen);

        Assert.AreEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, messages[2].Sequence);
        Assert.IsTrue(messages[2].IsDeleted);

        Assert.IsNotNull(messages[2].Flags);
        Assert.AreEqual(2, messages[2].Flags.Count);
        Assert.IsTrue(messages[2].Flags.Contains(ImapMessageFlag.Deleted));
        Assert.IsTrue(messages[2].Flags.Contains("$label1"));
        Assert.IsTrue(messages[2].IsMarkedAsDeleted);
      });
    }

    [Test]
    public void TestDeleteEmptySet()
    {
      TestMessage(0, CreateEmptySet(),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // do nothing
        messageSet.Delete();
      });
    }

    [Test]
    public void TestCopyToDestinationMailbox()
    {
      CopyOrMoveToDestinationMailbox(false);
    }

    [Test]
    public void TestMoveToDestinationMailbox()
    {
      CopyOrMoveToDestinationMailbox(true);
    }

    private void CopyOrMoveToDestinationMailbox(bool move)
    {
      TestMessage(3, ImapSequenceSet.CreateUidSet(1L, 3L, 5L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" dest\r\n" +
                                     "$tag OK done\r\n");

        var destMailbox = messageSet.Client.GetMailbox("dest");

        server.DequeueRequest();

        // UID COPY
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        if (move)
          // UID STORE
          server.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Deleted))\r\n" +
                                       "* FETCH 2 (FLAGS (\\Deleted \\Seen))\r\n" +
                                       "* FETCH 3 (FLAGS (\\Deleted $label1))\r\n" +
                                       "$tag OK done\r\n");

        if (move)
          messageSet.MoveTo(destMailbox);
        else
          messageSet.CopyTo(destMailbox);

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID COPY 1,3,5 dest\r\n"));

        if (move) {
          Assert.That(server.DequeueRequest(), Text.EndsWith("UID STORE 1,3,5 +FLAGS (\\Deleted)\r\n"));

          Assert.IsNotNull(messages[0].Flags);
          Assert.AreEqual(1, messages[0].Flags.Count);
          Assert.IsTrue(messages[0].Flags.Contains(ImapMessageFlag.Deleted));
          Assert.IsTrue(messages[0].IsMarkedAsDeleted);

          Assert.IsNotNull(messages[1].Flags);
          Assert.AreEqual(2, messages[1].Flags.Count);
          Assert.IsTrue(messages[1].Flags.Contains(ImapMessageFlag.Deleted));
          Assert.IsTrue(messages[1].Flags.Contains(ImapMessageFlag.Seen));
          Assert.IsTrue(messages[1].IsMarkedAsDeleted);
          Assert.IsTrue(messages[1].IsSeen);

          Assert.IsNotNull(messages[2].Flags);
          Assert.AreEqual(2, messages[2].Flags.Count);
          Assert.IsTrue(messages[2].Flags.Contains(ImapMessageFlag.Deleted));
          Assert.IsTrue(messages[2].Flags.Contains("$label1"));
          Assert.IsTrue(messages[2].IsMarkedAsDeleted);
        }
      });
    }

    [Test]
    public void TestMoveToDestinationMailboxCopyFailed()
    {
      TestMessage(3, ImapSequenceSet.CreateUidSet(1L, 3L, 5L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" dest\r\n" +
                                     "$tag OK done\r\n");

        var destMailbox = messageSet.Client.GetMailbox("dest");

        server.DequeueRequest();

        // UID COPY
        server.EnqueueTaggedResponse("$tag NO failed\r\n");

        TestUtils.ExpectExceptionThrown<ImapErrorResponseException>(delegate {
          // STORE won't be issued
          messageSet.MoveTo(destMailbox);
        });

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID COPY 1,3,5 dest\r\n"));
      });
    }

    [Test]
    public void TestCopyToDestinationMailboxItself()
    {
      CopyOrMoveToDestinationMailboxItself(false);
    }

    [Test]
    public void TestMoveToDestinationMailboxItself()
    {
      CopyOrMoveToDestinationMailboxItself(true);
    }

    private void CopyOrMoveToDestinationMailboxItself(bool move)
    {
      TestMessage(3, ImapSequenceSet.CreateUidSet(1L, 3L, 5L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        Assert.AreEqual(3, messageSet.Mailbox.ExistMessageCount);
        Assert.AreEqual(0, messageSet.Mailbox.RecentMessageCount);

        if (!move) {
          // UID COPY
          server.EnqueueTaggedResponse("* 3 RECENT\r\n" +
                                       "* 6 EXISTS\r\n" +
                                       "$tag OK done\r\n");
        }

        if (move)
          messageSet.MoveTo(messageSet.Mailbox);
        else
          messageSet.CopyTo(messageSet.Mailbox);

        if (move) {
          Assert.AreEqual(3, messageSet.Mailbox.ExistMessageCount);
          Assert.AreEqual(0, messageSet.Mailbox.RecentMessageCount);
        }
        else {
          Assert.That(server.DequeueRequest(), Text.EndsWith("UID COPY 1,3,5 " + messageSet.Mailbox.FullName + "\r\n"));

          Assert.AreEqual(6, messageSet.Mailbox.ExistMessageCount);
          Assert.AreEqual(3, messageSet.Mailbox.RecentMessageCount);
        }
      });
    }

    [Test]
    public void TestCopyToDestinationMailboxOfDifferentSession()
    {
      CopyOrMoveToDestinationMailboxOfDifferentSession(false);
    }

    [Test]
    public void TestMoveToDestinationMailboxOfDifferentSession()
    {
      CopyOrMoveToDestinationMailboxOfDifferentSession(true);
    }

    private void CopyOrMoveToDestinationMailboxOfDifferentSession(bool move)
    {
      TestMessage(3, ImapSequenceSet.CreateUidSet(1L, 3L, 5L),
                  delegate(ImapPseudoServer serverSrc, ImapMessageInfoBase srcMessageSet, ImapMessageInfo[] srcMessages) {
        TestUtils.TestAuthenticated(delegate(ImapPseudoServer serverDest, ImapClient clientDest) {
          // LIST
          serverDest.EnqueueTaggedResponse("* LIST () \"\" dest\r\n" +
                                           "$tag OK done\r\n");

          var destMailbox = clientDest.GetMailbox("dest");

          serverDest.DequeueRequest();

          Assert.AreNotSame(srcMessageSet.Client, destMailbox.Client);

          // FETCH
          serverSrc.EnqueueTaggedResponse("$tag NO FETCH failed\r\n");

          TestUtils.ExpectExceptionThrown<ImapErrorResponseException>(delegate {
            if (move)
              srcMessageSet.MoveTo(destMailbox);
            else
              srcMessageSet.CopyTo(destMailbox);
          });

          Assert.That(serverSrc.DequeueRequest(), Text.EndsWith("UID FETCH 1,3,5 (UID)\r\n"));
        });
      });
    }

    [Test, Ignore("not implemented")]
    public void TestCopyToDestinationMailboxUidValidityChanged()
    {
    }

    [Test, Ignore("not implemented")]
    public void TestMoveToDestinationMailboxUidValidityChanged()
    {
    }

    [Test, ExpectedException(typeof(ArgumentNullException))]
    public void TestCopyToDestinationMailboxArgumentNull()
    {
      CopyOrMoveToDestinationMailboxArgumentNull(false);
    }

    [Test, ExpectedException(typeof(ArgumentNullException))]
    public void TestMoveToDestinationMailboxArgumentNull()
    {
      CopyOrMoveToDestinationMailboxArgumentNull(true);
    }

    private void CopyOrMoveToDestinationMailboxArgumentNull(bool move)
    {
      TestMessage(3, ImapSequenceSet.CreateUidSet(1L, 3L, 5L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        if (move)
          messageSet.MoveTo((ImapMailboxInfo)null);
        else
          messageSet.CopyTo((ImapMailboxInfo)null);
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestCopyToDestinationMailboxNonExistent()
    {
      CopyOrMoveToDestinationMailboxNonExistent(false);
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestMoveToDestinationMailboxNonExistent()
    {
      CopyOrMoveToDestinationMailboxNonExistent(true);
    }

    private void CopyOrMoveToDestinationMailboxNonExistent(bool move)
    {
      TestMessage(10,
                  new[] {ImapCapability.ListExtended},
                  ImapSequenceSet.CreateUidRangeSet(1L, 10L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // LIST
        server.EnqueueTaggedResponse("* LIST (\\Subscribed \\NonExistent) \"\" dest\r\n" +
                                     "$tag OK done\r\n");

        var destMailbox = messageSet.Client.GetMailbox("dest", ImapMailboxListOptions.SubscribedOnly);

        server.DequeueRequest();

        Assert.IsFalse(destMailbox.Exists);

        // throws exception
        if (move)
          messageSet.MoveTo(destMailbox);
        else
          messageSet.CopyTo(destMailbox);
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestCopyToDestinationMailboxUnselectable()
    {
      CopyOrMoveToDestinationMailboxUnselectable(false);
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestMoveToDestinationMailboxUnselectable()
    {
      CopyOrMoveToDestinationMailboxUnselectable(true);
    }

    private void CopyOrMoveToDestinationMailboxUnselectable(bool move)
    {
      TestMessage(10,
                  new[] {ImapCapability.ListExtended},
                  ImapSequenceSet.CreateUidRangeSet(1L, 10L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // LIST
        server.EnqueueTaggedResponse("* LIST (\\Noselect) \"\" dest\r\n" +
                                     "$tag OK done\r\n");

        var destMailbox = messageSet.Client.GetMailbox("dest", ImapMailboxListOptions.SubscribedOnly);

        server.DequeueRequest();

        Assert.IsTrue(destMailbox.IsUnselectable);

        // throws exception
        if (move)
          messageSet.MoveTo(destMailbox);
        else
          messageSet.CopyTo(destMailbox);
      });
    }

    [Test]
    public void TestCopyToDestinationMailboxEmptySet()
    {
      CopyOrMoveToDestinationMailboxEmptySet(false);
    }

    [Test]
    public void TestMoveToDestinationMailboxEmptySet()
    {
      CopyOrMoveToDestinationMailboxEmptySet(true);
    }

    private void CopyOrMoveToDestinationMailboxEmptySet(bool move)
    {
      TestMessage(0, CreateEmptySet(),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" dest\r\n" +
                                     "$tag OK done\r\n");

        var destMailbox = messageSet.Client.GetMailbox("dest");

        server.DequeueRequest();

        // do nothing
        if (move)
          messageSet.MoveTo(destMailbox);
        else
          messageSet.CopyTo(destMailbox);
      });
    }

    [Test]
    public void TestCopyToDestinationMailboxMailboxClosed()
    {
      CopyOrMoveToDestinationMailboxMailboxClosed(false);
    }

    [Test]
    public void TestMoveToDestinationMailboxMailboxClosed()
    {
      CopyOrMoveToDestinationMailboxMailboxClosed(true);
    }

    private void CopyOrMoveToDestinationMailboxMailboxClosed(bool move)
    {
      TestMessage(3, ImapSequenceSet.CreateUidSet(1L, 3L, 5L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" dest\r\n" +
                                     "$tag OK done\r\n");

        var destMailbox = messageSet.Client.GetMailbox("dest");

        server.DequeueRequest();

        // CLOSE
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        messageSet.Mailbox.Close();

        Assert.IsFalse(messageSet.Mailbox.IsOpen);

        server.DequeueRequest();

        try {
          if (move)
            messageSet.MoveTo(destMailbox);
          else
            messageSet.CopyTo(destMailbox);

          Assert.Fail("ImapMailboxClosedException not thrown");
        }
        catch (ImapMailboxClosedException ex) {
          Assert.IsNotNull(ex.Mailbox);
          Assert.AreEqual(messageSet.Mailbox.FullName, ex.Mailbox);

          Smdn.Net.TestUtils.SerializeBinary(ex, delegate(ImapMailboxClosedException deserialized) {
            Assert.IsNotNull(deserialized.Mailbox);
            Assert.AreEqual(ex.Mailbox, deserialized.Mailbox);
          });
        }
      });
    }

    [Test]
    public void TestCopyToDestinationName()
    {
      CopyOrMoveToDestinationName(false, false);
    }

    [Test]
    public void TestMoveToDestinationName()
    {
      CopyOrMoveToDestinationName(true, false);
    }

    [Test]
    public void TestCopyToDestinationNameTryCreate()
    {
      CopyOrMoveToDestinationName(false, true);
    }

    [Test]
    public void TestMoveToDestinationNameTryCreate()
    {
      CopyOrMoveToDestinationName(true, true);
    }

    private void CopyOrMoveToDestinationName(bool move, bool tryCreate)
    {
      TestMessage(3, ImapSequenceSet.CreateUidRangeSet(1L, 3L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // UID COPY
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        if (move)
          // UID STORE
          server.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Deleted))\r\n" +
                                       "* FETCH 2 (FLAGS (\\Deleted \\Seen))\r\n" +
                                       "* FETCH 3 (FLAGS (\\Deleted $label1))\r\n" +
                                       "$tag OK done\r\n");

        ImapMailboxInfo createdMailbox = null;

        if (tryCreate) {
          createdMailbox = move
            ? messageSet.MoveTo("dest", true)
            : messageSet.CopyTo("dest", true);
        }
        else {
          if (move)
            messageSet.MoveTo("dest");
          else
            messageSet.CopyTo("dest");
        }

        if (tryCreate)
          Assert.IsNull(createdMailbox);

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID COPY 1:3 dest\r\n"));

        if (move) {
          Assert.That(server.DequeueRequest(), Text.EndsWith("UID STORE 1:3 +FLAGS (\\Deleted)\r\n"));

          Assert.IsNotNull(messages[0].Flags);
          Assert.AreEqual(1, messages[0].Flags.Count);
          Assert.IsTrue(messages[0].Flags.Contains(ImapMessageFlag.Deleted));
          Assert.IsTrue(messages[0].IsMarkedAsDeleted);

          Assert.IsNotNull(messages[1].Flags);
          Assert.AreEqual(2, messages[1].Flags.Count);
          Assert.IsTrue(messages[1].Flags.Contains(ImapMessageFlag.Deleted));
          Assert.IsTrue(messages[1].Flags.Contains(ImapMessageFlag.Seen));
          Assert.IsTrue(messages[1].IsMarkedAsDeleted);
          Assert.IsTrue(messages[1].IsSeen);

          Assert.IsNotNull(messages[2].Flags);
          Assert.AreEqual(2, messages[2].Flags.Count);
          Assert.IsTrue(messages[2].Flags.Contains(ImapMessageFlag.Deleted));
          Assert.IsTrue(messages[2].Flags.Contains("$label1"));
          Assert.IsTrue(messages[2].IsMarkedAsDeleted);
        }
      });
    }

    [Test]
    public void TestCopyToDestinationNameSameMailbox()
    {
      CopyOrMoveToDestinationNameSameMailbox(false);
    }

    [Test]
    public void TestMoveToDestinationNameSameMailbox()
    {
      CopyOrMoveToDestinationNameSameMailbox(true);
    }

    private void CopyOrMoveToDestinationNameSameMailbox(bool move)
    {
      TestMessage(3, ImapSequenceSet.CreateUidRangeSet(1L, 3L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        Assert.AreEqual(3, messageSet.Mailbox.ExistMessageCount);
        Assert.AreEqual(0, messageSet.Mailbox.RecentMessageCount);

        if (!move) {
          // UID COPY
          server.EnqueueTaggedResponse("* 3 RECENT\r\n" +
                                       "* 6 EXISTS\r\n" +
                                       "$tag OK done\r\n");
        }

        if (move)
          messageSet.MoveTo(messageSet.Mailbox.FullName);
        else
          messageSet.CopyTo(messageSet.Mailbox.FullName);

        if (move) {
          Assert.AreEqual(3, messageSet.Mailbox.ExistMessageCount);
          Assert.AreEqual(0, messageSet.Mailbox.RecentMessageCount);
        }
        else {
          Assert.That(server.DequeueRequest(), Text.EndsWith("UID COPY 1:3 " + messageSet.Mailbox.FullName + "\r\n"));

          Assert.AreEqual(6, messageSet.Mailbox.ExistMessageCount);
          Assert.AreEqual(3, messageSet.Mailbox.RecentMessageCount);
        }
      });
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestCopyToDestinationNameArgumentEmpty()
    {
      CopyOrMoveToDestinationNameArgumentEmpty(false);
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestMoveToDestinationNameArgumentEmpty()
    {
      CopyOrMoveToDestinationNameArgumentEmpty(true);
    }

    private void CopyOrMoveToDestinationNameArgumentEmpty(bool move)
    {
      TestMessage(3, ImapSequenceSet.CreateUidSet(1L, 3L, 5L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        if (move)
          messageSet.MoveTo(string.Empty);
        else
          messageSet.CopyTo(string.Empty);
      });
    }

    [Test, ExpectedException(typeof(ArgumentNullException))]
    public void TestCopyToDestinationNameArgumentNull()
    {
      CopyOrMoveToDestinationNameArgumentNull(false);
    }

    [Test, ExpectedException(typeof(ArgumentNullException))]
    public void TestMoveToDestinationNameArgumentNull()
    {
      CopyOrMoveToDestinationNameArgumentNull(true);
    }

    private void CopyOrMoveToDestinationNameArgumentNull(bool move)
    {
      TestMessage(3, ImapSequenceSet.CreateUidSet(1L, 3L, 5L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        if (move)
          messageSet.MoveTo((string)null);
        else
          messageSet.CopyTo((string)null);
      });
    }

    [Test]
    public void TestCopyToDestinationNameEmptySet()
    {
      CopyOrMoveToDestinationNameEmptySet(false);
    }

    [Test]
    public void TestMoveToDestinationNameEmptySet()
    {
      CopyOrMoveToDestinationNameEmptySet(true);
    }

    private void CopyOrMoveToDestinationNameEmptySet(bool move)
    {
      TestMessage(0, CreateEmptySet(),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // do nothing
        if (move)
          messageSet.MoveTo("dest");
        else
          messageSet.CopyTo("dest");
      });
    }

    [Test, Ignore("not implemented")]
    public void TestCopyToDestinationNameUidValidityChanged()
    {
    }

    [Test, Ignore("not implemented")]
    public void TestMoveToDestinationNameUidValidityChanged()
    {
    }

    [Test]
    public void TestCopyToDestinationNameMailboxClosed()
    {
      CopyOrMoveToDestinationNameMailboxClosed(false);
    }

    [Test]
    public void TestMoveToDestinationNameMailboxClosed()
    {
      CopyOrMoveToDestinationNameMailboxClosed(true);
    }

    private void CopyOrMoveToDestinationNameMailboxClosed(bool move)
    {
      TestMessage(10, ImapSequenceSet.CreateUidRangeSet(1L, 10L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // CLOSE
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        messageSet.Mailbox.Close();

        Assert.IsFalse(messageSet.Mailbox.IsOpen);

        server.DequeueRequest();

        try {
          if (move)
            messageSet.MoveTo("dest");
          else
            messageSet.CopyTo("dest");

          Assert.Fail("ImapMailboxClosedException not thrown");
        }
        catch (ImapMailboxClosedException ex) {
          Assert.IsNotNull(ex.Mailbox);
          Assert.AreEqual(messageSet.Mailbox.FullName, ex.Mailbox);

          Smdn.Net.TestUtils.SerializeBinary(ex, delegate(ImapMailboxClosedException deserialized) {
            Assert.IsNotNull(deserialized.Mailbox);
            Assert.AreEqual(ex.Mailbox, deserialized.Mailbox);
          });
        }
      });
    }

    [Test, ExpectedException(typeof(ImapErrorResponseException))]
    public void TestCopyToDestinationNameNoResponse()
    {
      CopyOrMoveToDestinationNameNoResponse(false);
    }

    [Test, ExpectedException(typeof(ImapErrorResponseException))]
    public void TestMoveToDestinationNameNoResponse()
    {
      CopyOrMoveToDestinationNameNoResponse(true);
    }

    private void CopyOrMoveToDestinationNameNoResponse(bool move)
    {
      TestMessage(10, ImapSequenceSet.CreateUidRangeSet(1L, 10L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // UID COPY
        server.EnqueueTaggedResponse("$tag NO error occurred\r\n");

        // throws exception
        if (move)
          messageSet.MoveTo("dest", false);
        else
          messageSet.CopyTo("dest", false);
      });
    }

    [Test]
    public void TestCopyToDestinationNameNoResponseDontTryCreate()
    {
      CopyOrMoveToDestinationNameNoResponseDontTryCreate(false);
    }

    [Test]
    public void TestMoveToDestinationNameNoResponseDontTryCreate()
    {
      CopyOrMoveToDestinationNameNoResponseDontTryCreate(true);
    }

    private void CopyOrMoveToDestinationNameNoResponseDontTryCreate(bool move)
    {
      TestMessage(10, ImapSequenceSet.CreateUidRangeSet(1L, 10L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // UID COPY
        server.EnqueueTaggedResponse("$tag NO [TRYCREATE] mailbox not exist\r\n");

        try {
          if (move)
            messageSet.MoveTo("dest", false);
          else
            messageSet.CopyTo("dest", false);

          Assert.Fail("ImapMailboxNotFoundException not thrown");
        }
        catch (ImapMailboxNotFoundException ex) {
          Assert.IsNotNull(ex.Mailbox);
          Assert.AreEqual("dest", ex.Mailbox);

          Smdn.Net.TestUtils.SerializeBinary(ex, delegate(ImapMailboxNotFoundException deserialized) {
            Assert.IsNotNull(deserialized.Mailbox);
            Assert.AreEqual(ex.Mailbox, deserialized.Mailbox);
          });
        }
      });
    }

    [Test]
    public void TestCopyToDestinationNameNoResponseTryCreate()
    {
      CopyOrMoveToDestinationNameNoResponseTryCreate(false);
    }

    [Test]
    public void TestMoveToDestinationNameNoResponseTryCreate()
    {
      CopyOrMoveToDestinationNameNoResponseTryCreate(true);
    }

    private void CopyOrMoveToDestinationNameNoResponseTryCreate(bool move)
    {
      TestMessage(3, ImapSequenceSet.CreateUidRangeSet(1L, 3L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // UID COPY
        server.EnqueueTaggedResponse("$tag NO [TRYCREATE] mailbox not exist\r\n");
        // CREATE
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // UID COPY
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" dest\r\n" +
                                     "$tag OK done\r\n");
        if (move)
          // UID STORE
          server.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Deleted))\r\n" +
                                       "* FETCH 2 (FLAGS (\\Deleted \\Seen))\r\n" +
                                       "* FETCH 3 (FLAGS (\\Deleted $label1))\r\n" +
                                       "$tag OK done\r\n");

        var createdMailbox = move
          ? messageSet.MoveTo("dest", true)
          : messageSet.CopyTo("dest", true);

        Assert.IsNotNull(createdMailbox);
        Assert.AreEqual("dest", createdMailbox.Name);
        Assert.AreEqual("dest", createdMailbox.FullName);

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID COPY 1:3 dest\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("CREATE dest\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("UID COPY 1:3 dest\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("LIST \"\" dest\r\n"));

        if (move) {
          Assert.That(server.DequeueRequest(), Text.EndsWith("UID STORE 1:3 +FLAGS (\\Deleted)\r\n"));

          Assert.IsNotNull(messages[0].Flags);
          Assert.AreEqual(1, messages[0].Flags.Count);
          Assert.IsTrue(messages[0].Flags.Contains(ImapMessageFlag.Deleted));
          Assert.IsTrue(messages[0].IsMarkedAsDeleted);

          Assert.IsNotNull(messages[1].Flags);
          Assert.AreEqual(2, messages[1].Flags.Count);
          Assert.IsTrue(messages[1].Flags.Contains(ImapMessageFlag.Deleted));
          Assert.IsTrue(messages[1].Flags.Contains(ImapMessageFlag.Seen));
          Assert.IsTrue(messages[1].IsMarkedAsDeleted);
          Assert.IsTrue(messages[1].IsSeen);

          Assert.IsNotNull(messages[2].Flags);
          Assert.AreEqual(2, messages[2].Flags.Count);
          Assert.IsTrue(messages[2].Flags.Contains(ImapMessageFlag.Deleted));
          Assert.IsTrue(messages[2].Flags.Contains("$label1"));
          Assert.IsTrue(messages[2].IsMarkedAsDeleted);
        }
      });
    }

    [Test]
    public void TestCopyToDestinationNameNoResponseTryCreateNameContainsWildcard()
    {
      CopyOrMoveToDestinationNameNoResponseTryCreateNameContainsWildcard(true);
    }

    [Test]
    public void TestMoveToDestinationNameNoResponseTryCreateNameContainsWildcard()
    {
      CopyOrMoveToDestinationNameNoResponseTryCreateNameContainsWildcard(false);
    }

    private void CopyOrMoveToDestinationNameNoResponseTryCreateNameContainsWildcard(bool move)
    {
      TestMessage(3, ImapSequenceSet.CreateUidRangeSet(1L, 3L),
                  delegate(ImapPseudoServer server, ImapMessageInfoBase messageSet, ImapMessageInfo[] messages) {
        // UID COPY
        server.EnqueueTaggedResponse("$tag NO [TRYCREATE] mailbox not exist\r\n");
        // CREATE
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // UID COPY
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // LIST
        server.EnqueueTaggedResponse("* LIST (\\Marked) \".\" Box*\r\n" +
                                     "* LIST () \".\" Box\r\n" +
                                     "* LIST () \".\" Box.Child\r\n" +
                                     "* LIST () \".\" Boxes\r\n" +
                                     "$tag OK done\r\n");
        if (move)
          // UID STORE
          server.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Deleted))\r\n" +
                                       "* FETCH 2 (FLAGS (\\Deleted \\Seen))\r\n" +
                                       "* FETCH 3 (FLAGS (\\Deleted $label1))\r\n" +
                                       "$tag OK done\r\n");

        var createdMailbox = move
          ? messageSet.MoveTo("Box*", true)
          : messageSet.CopyTo("Box*", true);

        Assert.IsNotNull(createdMailbox);
        Assert.AreEqual("Box*", createdMailbox.Name);
        Assert.AreEqual("Box*", createdMailbox.FullName);
        Assert.AreEqual(1, createdMailbox.Flags.Count);
        CollectionAssert.Contains(createdMailbox.Flags, ImapMailboxFlag.Marked);

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID COPY 1:3 \"Box*\"\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("CREATE \"Box*\"\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("UID COPY 1:3 \"Box*\"\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("LIST \"\" Box*\r\n"));

        if (move) {
          Assert.That(server.DequeueRequest(), Text.EndsWith("UID STORE 1:3 +FLAGS (\\Deleted)\r\n"));

          Assert.IsNotNull(messages[0].Flags);
          Assert.AreEqual(1, messages[0].Flags.Count);
          Assert.IsTrue(messages[0].Flags.Contains(ImapMessageFlag.Deleted));
          Assert.IsTrue(messages[0].IsMarkedAsDeleted);

          Assert.IsNotNull(messages[1].Flags);
          Assert.AreEqual(2, messages[1].Flags.Count);
          Assert.IsTrue(messages[1].Flags.Contains(ImapMessageFlag.Deleted));
          Assert.IsTrue(messages[1].Flags.Contains(ImapMessageFlag.Seen));
          Assert.IsTrue(messages[1].IsMarkedAsDeleted);
          Assert.IsTrue(messages[1].IsSeen);

          Assert.IsNotNull(messages[2].Flags);
          Assert.AreEqual(2, messages[2].Flags.Count);
          Assert.IsTrue(messages[2].Flags.Contains(ImapMessageFlag.Deleted));
          Assert.IsTrue(messages[2].Flags.Contains("$label1"));
          Assert.IsTrue(messages[2].IsMarkedAsDeleted);
        }
      });
    }
  }
}
